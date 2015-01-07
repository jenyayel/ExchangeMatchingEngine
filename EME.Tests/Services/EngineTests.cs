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
            var _aggregatedEvents = new Queue<EventMessageArgs>();
            _eventsDispatcher.MessageReceived += (sender, e) => { _aggregatedEvents.Enqueue(e); };

            // --------------------------------
            // Act and Assert
            // --------------------------------
            _engine.ProcessOrder(OrderType.Buy, "MSFT", 10, 50);
            Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _aggregatedEvents.Dequeue().Type);

            _engine.ProcessOrder(OrderType.Sell, "MSFT", 5, null);
            Assert.AreEqual(typeof(MarketOrderPlacedEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(PartialFilledEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _aggregatedEvents.Dequeue().Type);

            _engine.ProcessOrder(OrderType.Buy, "MSFT", 5, 50);
            Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(PartialFilledEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _aggregatedEvents.Dequeue().Type);

            _engine.ProcessOrder(OrderType.Sell, "MSFT", 10, 50);
            Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _aggregatedEvents.Dequeue().Type);

            _engine.ProcessOrder(OrderType.Buy, "MSFT", 10, 52);
            Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(PartialFilledEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _aggregatedEvents.Dequeue().Type);

            _engine.ProcessOrder(OrderType.Sell, "MSFT", 10, 50);
            Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _aggregatedEvents.Dequeue().Type);

            _engine.ProcessOrder(OrderType.Buy, "MSFT", 5, 48);
            Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _aggregatedEvents.Dequeue().Type);

            _engine.ProcessOrder(OrderType.Sell, "MSFT", 10, null);
            Assert.AreEqual(typeof(MarketOrderPlacedEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(PartialFilledEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _aggregatedEvents.Dequeue().Type);

            _engine.ProcessOrder(OrderType.Buy, "MSFT", 10, null);
            Assert.AreEqual(typeof(MarketOrderPlacedEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(PartialFilledEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _aggregatedEvents.Dequeue().Type);
            Assert.AreEqual(typeof(FilledEvent).Name, _aggregatedEvents.Dequeue().Type);
        }
    }
}
