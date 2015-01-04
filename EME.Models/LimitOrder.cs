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

        public int PartitionKey { get; private set; }

        public LimitOrder(OrderType orderType, string symbol, int shares, decimal price)
            : base(orderType, symbol, shares)
        {
            if (price <= 0) throw new ArgumentOutOfRangeException("price");

            this.Price = price;
            this.PartitionKey = symbol.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{0} {1}", base.ToString(), Price.ToString("C"));
        }
    }

    public class PriceComparer : IComparer<LimitOrder>
    {
        private bool m_isDescending;

        public PriceComparer(bool isDescending = false)
        {
            m_isDescending = isDescending;
        }

        public int Compare(LimitOrder x, LimitOrder y)
        {
            if (LimitOrder.ReferenceEquals(x, y))
                return 0;
            else if (x == null)
                return m_isDescending ? -1 : 1;
            else if (y == null)
                return m_isDescending ? 1 : -1;
            else if (x.Price > y.Price)
                return m_isDescending ? -1 : 1;
            else if (x.Price < y.Price)
                return m_isDescending ? 1 : -1;
            else
                return 0;
        }
    }
}
