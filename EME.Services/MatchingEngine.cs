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

            Order _placedOrder;
            if (price.HasValue)
                _placedOrder = addLimitOrder(orderType, symbol, shares, price.Value);
            else
                _placedOrder = addMarketOrder(orderType, symbol, shares);

            // look in limit orders book
            matchInOrderBook(_placedOrder, price);

            // look in market orders queue
            //while (_placedOrder.CurrentSharesCount > 0)
            //{

            //}

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
                var _handledShares = 0;
                if (_matchedLimitOrder.CurrentSharesCount >= placedOrder.CurrentSharesCount)
                {
                    _matchedLimitOrder.CurrentSharesCount -= placedOrder.CurrentSharesCount;
                    _handledShares = placedOrder.CurrentSharesCount;
                    placedOrder.CurrentSharesCount = 0;
                }
                else
                {
                    placedOrder.CurrentSharesCount -= _matchedLimitOrder.OriginalSharesCount;
                    _handledShares = _matchedLimitOrder.OriginalSharesCount;
                    _matchedLimitOrder.CurrentSharesCount = 0;
                }

                // persist matched order
                // in case no "shares" remains in order we can remove it from order book
                if (_matchedLimitOrder.CurrentSharesCount == 0)
                {
                    m_ordersBook.Remove(_matchedLimitOrder);
                    m_limitOrdersRepository.Remove(_matchedLimitOrder);
                }
                else
                    m_limitOrdersRepository.Update(_matchedLimitOrder);

                // persist changes in placed order
                // TODO: _placedOrder

                // calculate price
                var _actualPrice = price.HasValue ? (_matchedLimitOrder.Price + price.Value) / 2 : _matchedLimitOrder.Price;

                // fire event for found match
                m_publisher.Send(
                    typeof(PartialFilledEvent).Name,
                    new PartialFilledEvent
                    {
                        BuyOrderId = placedOrder.OrderType == OrderType.Buy ? placedOrder.Id : _matchedLimitOrder.Id,
                        SellOrderId = placedOrder.OrderType == OrderType.Sell ? placedOrder.Id : _matchedLimitOrder.Id,
                        Price = _actualPrice,
                        Shares = _handledShares
                    }.ToJSON());

                // fire event for placed order fill
                if (placedOrder.CurrentSharesCount == 0)
                {
                    m_publisher.Send(
                        typeof(FilledEvent).Name,
                        new FilledEvent
                        {
                            OrderId = placedOrder.Id,
                            AveragePrice = _actualPrice
                        }.ToJSON());
                }

                // fire event for matched order fill
                if (_matchedLimitOrder.CurrentSharesCount == 0)
                {
                    m_publisher.Send(
                        typeof(FilledEvent).Name,
                        new FilledEvent
                        {
                            OrderId = _matchedLimitOrder.Id,
                            AveragePrice = _actualPrice
                        }.ToJSON());
                }
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
