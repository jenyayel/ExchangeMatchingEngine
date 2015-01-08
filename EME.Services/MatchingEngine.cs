using EME.Infrastructure.Messaging;
using EME.Infrastructure.Persistance;
using EME.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EME.Infrastructure.Serialization;
using EME.Models.Events;

namespace EME.Services
{
    public class MatchingEngine : IMatchingEngine
    {
        private LimitOrdersBook m_ordersBook;
        private MarketOrdersQueue m_ordersQueue;

        private IEventsDispatcher m_publisher;
        private IRepository<LimitOrder> m_limitOrdersRepository;
        private IRepository<MarketOrder> m_marketOrdersRepository;

        public MatchingEngine(
            IEventsDispatcher publisher,
            IRepository<LimitOrder> limitOrdersRepository,
            IRepository<MarketOrder> marketOrdersRepository)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (limitOrdersRepository == null) throw new ArgumentNullException("limitOrdersRepository");
            if (marketOrdersRepository == null) throw new ArgumentNullException("marketOrdersRepository");

            m_publisher = publisher;
            m_limitOrdersRepository = limitOrdersRepository;
            m_marketOrdersRepository = marketOrdersRepository;

            // TODO: load from repository
            m_ordersBook = new LimitOrdersBook();
            m_ordersQueue = new MarketOrdersQueue();
        }

        /// <summary>
        /// Process all busness logic for newly placed order
        /// </summary>
        /// <param name="orderType">The type of order that was just placed</param>
        /// <param name="symbol"></param>
        /// <param name="shares">Number of shares for new order</param>
        /// <param name="price">The price for each share in newly placed order</param>
        public void ProcessOrder(OrderType orderType, string symbol, int shares, double? price)
        {
            if (String.IsNullOrEmpty(symbol)) throw new ArgumentNullException("symbol");
            if (shares < 1) throw new ArgumentOutOfRangeException("shares");

            // create, persist and fire event for newly received order
            Order _placedOrder;
            if (price.HasValue)
                _placedOrder = addLimitOrder(orderType, symbol, shares, price.Value);
            else
                _placedOrder = addMarketOrder(orderType, symbol, shares);

            // look for "oposite" matchings in limit orders book
            matchInOrderBook(_placedOrder, price);

            // look in market orders queue
            // TODO: not clear whether market orders should be also checked in market orders queue,
            // if so what will be the closed price?
            // anyway, we assume that it is not supposed to happen
            if (_placedOrder is LimitOrder)
                matchInOrdersQueue((LimitOrder)_placedOrder);

        }

        /// <summary>
        /// Find ALL matches for a specified order in order book, persist changes and fires events
        /// </summary>
        /// <param name="placedOrder"></param>
        private void matchInOrderBook(Order placedOrder, double? price)
        {
            while (placedOrder.CurrentSharesCount > 0)
            {
                var _matchedLimitOrder = m_ordersBook.FindMatch(
                    placedOrder.OrderType == OrderType.Buy ? OrderType.Sell : OrderType.Buy, placedOrder.Symbol, price);

                if (_matchedLimitOrder == null) break;

                // update shares number in both orders (requested and matched)
                var _handledShares = transferOrdersShares(placedOrder, _matchedLimitOrder);

                // persist matched order
                // TODO: in case no "shares" remains in order we can remove it from order book
                m_limitOrdersRepository.Update(_matchedLimitOrder);

                // TODO: persist changes in placed order
                // m_limitOrdersRepository.Update(placedOrder);

                // calculate price
                var _actualPrice = price.HasValue ? (_matchedLimitOrder.Price + price.Value) / 2 : _matchedLimitOrder.Price;

                // fire event for found match
                processedOrdersEvents(placedOrder, _matchedLimitOrder, _handledShares, _actualPrice);
            }
        }
        
        private void matchInOrdersQueue(LimitOrder placedOrder)
        {
            while (placedOrder.CurrentSharesCount > 0)
            {
                var _matchedMarketOrder = m_ordersQueue.FindMatch(
                    placedOrder.OrderType == OrderType.Buy ? OrderType.Sell : OrderType.Buy, placedOrder.Symbol);

                if (_matchedMarketOrder == null) break;

                // update shares number in both orders (requested and matched)
                var _handledShares = transferOrdersShares(placedOrder, _matchedMarketOrder);

                // persist matched order
                // TODO: in case no "shares" remains in order we can remove it from queue
                m_marketOrdersRepository.Update(_matchedMarketOrder);
                
                // persist changes in placed order
                m_limitOrdersRepository.Update(placedOrder);
                
                // fire event for found match
                processedOrdersEvents(placedOrder, _matchedMarketOrder, _handledShares, placedOrder.Price);
            }
        }

        /// <summary>
        /// Transfer shares from one order to another
        /// </summary>
        /// <returns>Number of transfered shares</returns>
        private int transferOrdersShares(Order placedOrder, Order matchedOrder)
        {
            var _handledShares = 0;
            if (matchedOrder.CurrentSharesCount >= placedOrder.CurrentSharesCount)
            {
                matchedOrder.CurrentSharesCount -= placedOrder.CurrentSharesCount;
                _handledShares = placedOrder.CurrentSharesCount;
                placedOrder.CurrentSharesCount = 0;
            }
            else
            {
                placedOrder.CurrentSharesCount -= matchedOrder.OriginalSharesCount;
                _handledShares = matchedOrder.OriginalSharesCount;
                matchedOrder.CurrentSharesCount = 0;
            }
            return _handledShares;
        }

        private void processedOrdersEvents(Order placedOrder, Order matchedOrder, int transferedShares, double price)
        {
            m_publisher.Send(
                typeof(PartialFilledEvent).Name,
                new PartialFilledEvent
                {
                    BuyOrderId = placedOrder.OrderType == OrderType.Buy ? placedOrder.Id : matchedOrder.Id,
                    SellOrderId = placedOrder.OrderType == OrderType.Sell ? placedOrder.Id : matchedOrder.Id,
                    Price = price,
                    Shares = transferedShares
                }.ToJSON());

            // fire event for placed order fill
            if (placedOrder.CurrentSharesCount == 0)
            {
                m_publisher.Send(
                    typeof(FilledEvent).Name,
                    new FilledEvent
                    {
                        OrderId = placedOrder.Id,
                        AveragePrice = price
                    }.ToJSON());
            }

            // fire event for matched order fill
            if (matchedOrder.CurrentSharesCount == 0)
            {
                m_publisher.Send(
                    typeof(FilledEvent).Name,
                    new FilledEvent
                    {
                        OrderId = matchedOrder.Id,
                        AveragePrice = price
                    }.ToJSON());
            }
        }

        /// <summary>
        /// Add to list of limit orders, persist it and fire event
        /// </summary>
        /// <returns>The added order</returns>
        private LimitOrder addLimitOrder(OrderType orderType, string symbol, int shares, double price)
        {
            var _order = new LimitOrder(orderType, symbol, shares, price);

            // add to book
            m_ordersBook.AddOrder(_order);

            // persist
            m_limitOrdersRepository.Add(_order);

            // fire event
            m_publisher.Send(
                typeof(LimitOrderPlacedEvent).Name,
                new LimitOrderPlacedEvent
                {
                    OrderId = _order.Id,
                    Shares = _order.OriginalSharesCount,
                    Symbol = _order.Symbol,
                    Price = _order.Price
                }.ToJSON());

            return _order;
        }

        /// <summary>
        /// Add to list of market orders, persist it and fire event
        /// </summary>
        /// <returns>The added order</returns>
        private MarketOrder addMarketOrder(OrderType orderType, string symbol, int shares)
        {
            var _order = new MarketOrder(orderType, symbol, shares);

            // add to queue
            m_ordersQueue.AddOrder(_order);

            // persist
            m_marketOrdersRepository.Add(_order);

            // fire event
            m_publisher.Send(
                typeof(MarketOrderPlacedEvent).Name,
                new MarketOrderPlacedEvent
                {
                    OrderId = _order.Id,
                    Shares = _order.OriginalSharesCount,
                    Symbol = _order.Symbol
                }.ToJSON());

            return _order;
        }

    }
}
