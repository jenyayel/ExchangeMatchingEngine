using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Models
{
    public class MarketOrdersQueue
    {
        private List<MarketOrder> m_orders; // FIFO style

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
    }
}
