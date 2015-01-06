using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Autofac;
using EME.Services;
using EME.Infrastructure.Persistance;
using EME.Models;

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

            var _container = _builder.Build();
            var _engine = _container.Resolve<IMatchingEngine>();

            // --------------------------------
            // Act
            // --------------------------------
            _engine.ProcessOrder(OrderType.Buy, "MSFT", 10, 50);
            _engine.ProcessOrder(OrderType.Sell, "MSFT", 5, null);
            _engine.ProcessOrder(OrderType.Buy, "MSFT", 5, 50);
            _engine.ProcessOrder(OrderType.Sell, "MSFT", 10, 50);
            _engine.ProcessOrder(OrderType.Buy, "MSFT", 5, 52);
            _engine.ProcessOrder(OrderType.Sell, "MSFT", 10, 50);
            _engine.ProcessOrder(OrderType.Buy, "MSFT", 5, 48);
            _engine.ProcessOrder(OrderType.Sell, "MSFT", 10, null);
            _engine.ProcessOrder(OrderType.Buy, "MSFT", 10, null);

            // --------------------------------
            // Assert
            // --------------------------------            

        }
    }
}
