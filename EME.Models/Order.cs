using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Models
{
    public abstract class Order
    {
        public Guid OrderId { get; private set; }

        public string Symbol { get; private set; }

        public int SharesCount { get; private set; }

        public OrderType OrderType { get; private set; }

        public Order(OrderType orderType, string symbol, int shares)
        {
            if (String.IsNullOrEmpty(symbol)) throw new ArgumentNullException("symbol");
            if (shares < 1) throw new ArgumentOutOfRangeException("shares");

            this.OrderId = Guid.NewGuid();
            this.OrderType = orderType;
            this.Symbol = symbol;
            this.SharesCount = shares;
        }

        public override string ToString()
        {
            return String.Format("[{0}]: {1} {2}", Symbol, OrderType, SharesCount);
        }
    }
}
