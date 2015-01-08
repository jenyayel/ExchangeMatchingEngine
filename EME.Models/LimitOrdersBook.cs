using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Models
{
    public class LimitOrdersBook
    {
        private List<LimitOrder> m_buyOrders; // high to low prices
        private List<LimitOrder> m_sellOrders; // low to high prices

        public LimitOrdersBook()
            : this(new List<LimitOrder>(), new List<LimitOrder>())
        {
        }

        public LimitOrdersBook(List<LimitOrder> orderedBuyOrders, List<LimitOrder> orderedSellOrders)
        {
            if (orderedBuyOrders == null) throw new ArgumentNullException("orderedBuyOrders");
            if (orderedSellOrders == null) throw new ArgumentNullException("orderedSellOrders");

            m_buyOrders = orderedBuyOrders;
            m_sellOrders = orderedSellOrders;
        }

        /// <summary>
        /// Adds order at correct place. 
        /// </summary>
        public void AddOrder(LimitOrder order)
        {
            if (order == null) throw new ArgumentNullException("order");

            var _list = order.OrderType == OrderType.Buy ? m_buyOrders : m_sellOrders;
            var _index = _list.BinarySearch(order, new PriceComparer(order.OrderType == OrderType.Buy));
            _list.Insert(_index < 0 ? ~_index : _index, order);
        }

        /// <summary>
        /// Finds a matching order for specified (or not) price.
        /// </summary>
        /// <remarks>
        /// Currently the search is done brutally.
        /// </remarks>
        /// <returns>The order with most expensive or most cheap price for a given symbol, 
        /// but not lower or higher (respectively) than a given price.</returns>
        public LimitOrder FindMatch(OrderType typeToSearch, string symbol, double? price = null)
        {
            if (String.IsNullOrEmpty(symbol)) throw new ArgumentNullException("symbol");

            var _list = typeToSearch == OrderType.Buy ? m_buyOrders : m_sellOrders;

            var _priceMatch = new Func<double, double, bool>((listPrice, askedPrice) =>
            {
                if (typeToSearch == OrderType.Buy) return listPrice >= askedPrice;
                else return listPrice <= askedPrice;
            });
            
            return _list.FirstOrDefault(o => o.Symbol == symbol && o.CurrentSharesCount > 0 &&
                (!price.HasValue || _priceMatch(o.Price, price.Value)));
        }

        public void Remove(LimitOrder order)
        {
            if (order == null) throw new ArgumentNullException("order");

            var _list = order.OrderType == OrderType.Buy ? m_buyOrders : m_sellOrders;
            _list.Remove(order);
        }
    }
}
