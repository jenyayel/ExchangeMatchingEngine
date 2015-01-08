using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Models
{
    public class MarketOrdersQueue
    {
        private List<MarketOrder> m_orders; // FIFO style, but list becuase it is easer to iterate

        public MarketOrdersQueue()
            : this(new List<MarketOrder>())
        {
        }

        public MarketOrdersQueue(List<MarketOrder> orders)
        {
            if (orders == null) throw new ArgumentNullException("orders");
            m_orders = orders;
        }

        public void AddOrder(MarketOrder order)
        {
            if (order == null) throw new ArgumentNullException("order");
            m_orders.Add(order);
        }

        /// <summary>
        /// Finds a matching order for specified symbol and order type.
        /// </summary>
        /// <remarks>
        /// Currently the search is done brutally.
        /// </remarks>
        public MarketOrder FindMatch(OrderType orderType, string symbol)
        {
            if (String.IsNullOrEmpty(symbol)) throw new ArgumentNullException("symbol");

            return m_orders.FirstOrDefault(o => o.Symbol == symbol && 
                o.CurrentSharesCount > 0 
                && o.OrderType == orderType);
        }

        public void Remove(MarketOrder order)
        {
            if (order == null) throw new ArgumentNullException("order");
            m_orders.Remove(order);
        }
    }
}
