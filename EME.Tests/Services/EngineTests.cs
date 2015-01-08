using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Autofac;
using EME.Services;
using EME.Infrastructure.Persistance;
using EME.Models;
using EME.Tests.Stubs;
using EME.Infrastructure.Messaging;
using System.Collections.Generic;
using EME.Models.Events;
using EME.Infrastructure.Serialization;

namespace EME.Tests.Services
{
    [TestClass]
    public class EngineTests
    {
        [TestMethod]
        public void AcceptanceTest()
        {
            // --------------------------------
            // Arrange
            // --------------------------------
            var _builder = new ContainerBuilder();

            _builder
               .RegisterType<MatchingEngine>()
               .As<IMatchingEngine>();

            _builder
               .RegisterGeneric(typeof(InMemoryRepository<>))
               .As(typeof(IRepository<>));

            _builder
               .RegisterType<EventsDispatcher>()
               .As<IEventsDispatcher>()
               .SingleInstance();

            var _container = _builder.Build();
            var _engine = _container.Resolve<IMatchingEngine>();
            var _eventsDispatcher = (EventsDispatcher)_container.Resolve<IEventsDispatcher>();
            var _eventsQueue = new Queue<EventMessageArgs>();
            _eventsDispatcher.MessageReceived += (sender, e) => { _eventsQueue.Enqueue(e); };

            // --------------------------------
            // Act and Assert
            // --------------------------------
            
            // -- command 1 --
            _engine.ProcessOrder(OrderType.Buy, "MSFT", 10, 50);
            Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _eventsQueue.Dequeue().Type);

            // -- command 2 --
            _engine.ProcessOrder(OrderType.Sell, "MSFT", 5, null);
            Assert.AreEqual(typeof(MarketOrderPlacedEvent).Name, _eventsQueue.Dequeue().Type);
            Assert.AreEqual(typeof(PartialFilledEvent).Name, _eventsQueue.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _eventsQueue.Dequeue().Type);

            // -- command 3 --
            // original: _engine.ProcessOrder(OrderType.Buy, "MSFT", 5, 50);
            _engine.ProcessOrder(OrderType.Sell, "MSFT", 5, 50);
            Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _eventsQueue.Dequeue().Type);
            Assert.AreEqual(typeof(PartialFilledEvent).Name, _eventsQueue.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _eventsQueue.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _eventsQueue.Dequeue().Type);

            // -- command 4 --
            _engine.ProcessOrder(OrderType.Sell, "MSFT", 10, 50);
            Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _eventsQueue.Dequeue().Type);

            // -- command 5 --
            _engine.ProcessOrder(OrderType.Buy, "MSFT", 10, 52);
            Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _eventsQueue.Dequeue().Type);
            Assert.AreEqual(typeof(PartialFilledEvent).Name, _eventsQueue.Peek().Type);
            Assert.AreEqual(51, _eventsQueue.Dequeue().Message.FromJSON<PartialFilledEvent>().Price);
            Assert.AreEqual(typeof(FilledEvent).Name, _eventsQueue.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _eventsQueue.Dequeue().Type);

            // -- command 6 --
            _engine.ProcessOrder(OrderType.Sell, "MSFT", 10, 50);
            Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _eventsQueue.Dequeue().Type);
            
            // -- command 7 --
            _engine.ProcessOrder(OrderType.Buy, "MSFT", 10, 48);
            Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _eventsQueue.Dequeue().Type);

            // -- command 8 --
            _engine.ProcessOrder(OrderType.Sell, "MSFT", 10, null);
            Assert.AreEqual(typeof(MarketOrderPlacedEvent).Name, _eventsQueue.Dequeue().Type);
            Assert.AreEqual(typeof(PartialFilledEvent).Name, _eventsQueue.Peek().Type);
            Assert.AreEqual(48, _eventsQueue.Dequeue().Message.FromJSON<PartialFilledEvent>().Price);
            Assert.AreEqual(typeof(FilledEvent).Name, _eventsQueue.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _eventsQueue.Dequeue().Type);

            // -- command 9 --
            _engine.ProcessOrder(OrderType.Buy, "MSFT", 10, null);
            Assert.AreEqual(typeof(MarketOrderPlacedEvent).Name, _eventsQueue.Dequeue().Type);
            Assert.AreEqual(typeof(PartialFilledEvent).Name, _eventsQueue.Peek().Type);
            Assert.AreEqual(50, _eventsQueue.Dequeue().Message.FromJSON<PartialFilledEvent>().Price);
            Assert.AreEqual(typeof(FilledEvent).Name, _eventsQueue.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _eventsQueue.Dequeue().Type);
        }
    }
}
