using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Models
{
    /// <summary>
    /// A market order is a buy or sell order to be executed immediately at current market prices.
    /// </summary>
    public class MarketOrder : Order
    {
        public MarketOrder(OrderType orderType, string symbol, int shares)
            : base(orderType, symbol, shares)
        {
        }
    }
}
