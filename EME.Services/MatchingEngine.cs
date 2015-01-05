using EME.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Services
{
    public class MatchingEngine : IMatchingEngine
    {
        private LimitOrdersBook m_ordersBook;

        public MatchingEngine()
        {
            m_ordersBook = new LimitOrdersBook();
        }

        public void ProcessOrder(OrderType orderType, string symbol, int shares, decimal? price)
        {
            if (price.HasValue)
            {
                var _order = new LimitOrder(orderType, symbol, shares, price.Value);

            }
            else
            {
                var _order = new MarketOrder(orderType, symbol, shares);

            }
        }
    }
}
