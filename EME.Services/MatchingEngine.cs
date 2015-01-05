using EME.Infrastructure.Messaging;
using EME.Infrastructure.Persistance;
using EME.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EME.Infrastructure.Extensions;
using EME.Models.Events;

namespace EME.Services
{
    public class MatchingEngine : IMatchingEngine
    {
        private LimitOrdersBook m_ordersBook;
        private MarketOrdersQueue m_ordersQueue;

        private IMessageBusPublisher m_publisher;
        private IRepository<LimitOrder> m_limitOrdersRepository;
        private IRepository<MarketOrder> m_marketOrdersRepository;

        public MatchingEngine(
            IMessageBusPublisher publisher,
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

        public void ProcessOrder(OrderType orderType, string symbol, int shares, double? price)
        {
            if (price.HasValue)
            {
                var _order = new LimitOrder(orderType, symbol, shares, price.Value);

                // add to book
                m_ordersBook.AddOrder(_order);

                // persist
                m_limitOrdersRepository.Add(_order);

                // fire event
                m_publisher.Send(new LimitOrderPlacedEvent
                {
                    OrderId = _order.OrderId,
                    Shares = _order.SharesCount,
                    Symbol = _order.Symbol,
                    Price = _order.Price
                }.ToJSONMessage());
            }
            else
            {
                var _order = new MarketOrder(orderType, symbol, shares);

                // add to queue
                m_ordersQueue.AddOrder(_order);

                // persist
                m_marketOrdersRepository.Add(_order);

                // fire event
                m_publisher.Send(new MarketOrderPlacedEvent
                {
                    OrderId = _order.OrderId,
                    Shares = _order.SharesCount,
                    Symbol = _order.Symbol
                }.ToJSONMessage());
            }
        }
    }
}
