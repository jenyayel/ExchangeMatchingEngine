using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Models
{
    public enum OrderType
    {
        Buy = 0,
        Sell = 1
    }

    public abstract class Order : BaseEntity
    {
        private int m_currentSharesCount;
        
        public string Symbol { get; private set; }

        /// <summary>
        /// Number of shares when the order was created
        /// </summary>
        public int OriginalSharesCount { get; private set; }

        /// <summary>
        /// Updated number of shares after matching was done
        /// </summary>
        public int CurrentSharesCount
        {
            get { return m_currentSharesCount; }
            set
            {
                if (m_currentSharesCount < 0 || m_currentSharesCount > OriginalSharesCount)
                    throw new ArgumentOutOfRangeException("value");
                m_currentSharesCount = value;
            }
        }

        public OrderType OrderType { get; private set; }

        /// <summary>
        /// Creates a new instance of order - not from persisted storage
        /// </summary>
        protected Order(OrderType orderType, string symbol, int shares)
        {
            if (String.IsNullOrEmpty(symbol)) throw new ArgumentNullException("symbol");
            if (shares < 1) throw new ArgumentOutOfRangeException("shares");

            this.Id = Guid.NewGuid();
            this.OrderType = orderType;
            this.Symbol = symbol;
            this.CurrentSharesCount = this.OriginalSharesCount = shares;
        }

        public override string ToString()
        {
            return String.Format("[{0}]: {1} {2}", Symbol, OrderType, OriginalSharesCount);
        }
    }
}
