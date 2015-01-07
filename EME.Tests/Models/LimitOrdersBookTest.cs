using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EME.Models;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace EME.Tests.Models
{
    [TestClass]
    public class LimitOrdersBookTest
    {
        Random m_random = new Random(DateTime.Now.Millisecond);

        [TestMethod]
        public void BuyListOrderTest()
        {
            // --------------------------------
            // Arrange
            // --------------------------------
            LimitOrdersBook _book = new LimitOrdersBook();

            // --------------------------------
            // Act
            // --------------------------------
            for (int i = 0; i < 10; i++)
                _book.AddOrder(new LimitOrder(
                    OrderType.Buy,
                    "GOOG",
                    10,
                    m_random.NextDouble() * (100 - 0.5) + 0.5));

            // add a few with same price
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 0.4));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 0.4));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 50));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 50));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 101));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 101));

            // --------------------------------
            // Assert
            // --------------------------------
            var _actualList = (IList<LimitOrder>)typeof(LimitOrdersBook)
                .GetField("m_buyOrders", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_book);
            var _expectedList = _actualList
                .OrderByDescending(o => o.Price);

            Assert.IsTrue(_expectedList.SequenceEqual(_actualList), "Buy list is not ordered");
        }


        [TestMethod]
        public void SellListOrderTest()
        {
            // --------------------------------
            // Arrange
            // --------------------------------
            LimitOrdersBook _book = new LimitOrdersBook();

            // --------------------------------
            // Act
            // --------------------------------
            for (int i = 0; i < 10; i++)
                _book.AddOrder(new LimitOrder(
                    OrderType.Sell,
                    "MSFT",
                    10,
                    m_random.NextDouble() * (50 - 0.9) + 0.9));

            // add a few with same price
            _book.AddOrder(new LimitOrder(OrderType.Sell, "MSFT", 10, 0.5));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "MSFT", 10, 0.5));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "MSFT", 10, 25));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "MSFT", 10, 25));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "MSFT", 10, 51));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "MSFT", 10, 51));

            // --------------------------------
            // Assert
            // --------------------------------
            var _actualList = (IList<LimitOrder>)typeof(LimitOrdersBook)
                .GetField("m_sellOrders", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_book);
            var _expectedList = _actualList
                .OrderBy(o => o.Price);

            Assert.IsTrue(_expectedList.SequenceEqual(_actualList), "Sell list is not ordered");
        }
        
        [TestMethod]
        public void FindBuyOrderWithPriceTest()
        {
            // --------------------------------
            // Arrange
            // --------------------------------
            LimitOrdersBook _book = new LimitOrdersBook();

            // --------------------------------
            // Act
            // --------------------------------
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 1));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "MSFT", 10, 5));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "MSFT", 10, 7.5));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 10));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "MSFT", 10, 20));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 30));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 40));

            // --------------------------------
            // Assert
            // --------------------------------
            Assert.AreEqual(40, _book.FindMatch(OrderType.Buy, "GOOG", 10).Price);
            Assert.AreEqual(20, _book.FindMatch(OrderType.Buy, "MSFT", 8).Price);
            Assert.AreEqual(20, _book.FindMatch(OrderType.Buy, "MSFT", 20).Price);
            Assert.IsNull(_book.FindMatch(OrderType.Buy, "GOOG", 100), "Found price that much higher than any in list");
        }

        [TestMethod]
        public void FindSellOrderWithPriceTest()
        {
            // --------------------------------
            // Arrange
            // --------------------------------
            LimitOrdersBook _book = new LimitOrdersBook();

            // --------------------------------
            // Act
            // --------------------------------
            _book.AddOrder(new LimitOrder(OrderType.Sell, "GOOG", 10, 1));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "MSFT", 10, 5));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "MSFT", 10, 7.5));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "GOOG", 10, 10));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "MSFT", 10, 20));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "GOOG", 10, 30));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "GOOG", 10, 40));

            // --------------------------------
            // Assert
            // --------------------------------
            Assert.AreEqual(1, _book.FindMatch(OrderType.Sell, "GOOG", 10).Price);
            Assert.AreEqual(5, _book.FindMatch(OrderType.Sell, "MSFT", 7.4).Price);
            Assert.AreEqual(1, _book.FindMatch(OrderType.Sell, "GOOG", 40).Price);
            Assert.IsNull(_book.FindMatch(OrderType.Sell, "GOOG", 0.5), "Found price that much higher than any in list");
        }

        [TestMethod]
        public void FindBuyOrderWithNoPriceTest()
        {
            // --------------------------------
            // Arrange
            // --------------------------------
            LimitOrdersBook _book = new LimitOrdersBook();

            // --------------------------------
            // Act
            // --------------------------------
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 1));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "MSFT", 10, 5));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "MSFT", 10, 7.5));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 10));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "MSFT", 10, 20));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 30));
            _book.AddOrder(new LimitOrder(OrderType.Buy, "GOOG", 10, 40));

            // --------------------------------
            // Assert
            // --------------------------------
            Assert.AreEqual(40, _book.FindMatch(OrderType.Buy, "GOOG").Price);
            Assert.AreEqual(20, _book.FindMatch(OrderType.Buy, "MSFT").Price);
            Assert.IsNull(_book.FindMatch(OrderType.Buy, "KUKU"));
        }

        [TestMethod]
        public void FindSellOrderWithNoPriceTest()
        {
            // --------------------------------
            // Arrange
            // --------------------------------
            LimitOrdersBook _book = new LimitOrdersBook();

            // --------------------------------
            // Act
            // --------------------------------
            _book.AddOrder(new LimitOrder(OrderType.Sell, "GOOG", 10, 1));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "MSFT", 10, 5));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "MSFT", 10, 7.5));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "GOOG", 10, 10));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "MSFT", 10, 20));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "GOOG", 10, 30));
            _book.AddOrder(new LimitOrder(OrderType.Sell, "GOOG", 10, 40));

            // --------------------------------
            // Assert
            // --------------------------------
            Assert.AreEqual(1, _book.FindMatch(OrderType.Sell, "GOOG").Price);
            Assert.AreEqual(5, _book.FindMatch(OrderType.Sell, "MSFT").Price);
            Assert.IsNull(_book.FindMatch(OrderType.Sell, "KUKU"));
        }
    }
}
