using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Models
{
    /// <summary>
    /// A limit order is an order to buy a security at no more than a specific price, 
    /// or to sell a security at no less than a specific price
    /// </summary>
    public class LimitOrder : Order
    {
        public decimal Price { get; private set; }

        public LimitOrder(OrderType orderType, string symbol, int shares, decimal price)
            : base(orderType, symbol, shares)
        {
            if (price <= 0) throw new ArgumentOutOfRangeException("price");
            
            this.Price = price;
        }
    }
}
