using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EME.Application;
using NetMQ;
using NetMQ.Sockets;
using Autofac;
using EME.Infrastructure.Serialization;
using EME.Application.Commands;
using System.Threading;

namespace EME.Tests.Application
{
    [TestClass]
    public class AcceptanceTests
    {
        private const string commandsEndpoint = "tcp://localhost:8881";
        private const string eventsEndpoint = "tcp://:8882";


        //[TestMethod]
        //public void FinalAcceptanceTest()
        //{
        //    var _container = IocConfig
        //      .CreateDefaultContainer(commandsEndpoint, eventsEndpoint);

        //    var _application = _container.Resolve<IApplication>();
        //    _application.Run();

        //    using (var _client = _container.Resolve<NetMQContext>().CreateRequestSocket())
        //    {
        //        _client.Connect(commandsEndpoint);
        //        _client.SendMore(OrderCommand.LIMIT_ORDER);
        //        _client.Send(new OrderCommand
        //        {
        //            Type = 0,
        //            Shares = 10,
        //            Symbol = "MSFT",
        //            Price = 50
        //        }.ToJSON());
        //    }

        //    _application.Stop();
        //}

        [TestMethod]
        public void RoutingBySymbolTest()
        {
            var _container = IocConfig
              .CreateDefaultContainer(commandsEndpoint, eventsEndpoint);

            var _application = _container.Resolve<IApplication>();
            _application.Run();

            using (var _client = _container.Resolve<NetMQContext>().CreatePushSocket())
            {
                _client.Connect(commandsEndpoint);
                _client.SendMore(OrderCommand.LIMIT_ORDER);
                _client.Send(new OrderCommand
                {
                    Type = 0,
                    Shares = 10,
                    Symbol = "MSFT",
                    Price = 50
                }.ToJSON());
                
                _client.SendMore(OrderCommand.LIMIT_ORDER);
                _client.Send(new OrderCommand
                {
                    Type = 0,
                    Shares = 10,
                    Symbol = "GOOG",
                    Price = 50
                }.ToJSON());
            }
            Thread.Sleep(1000);
            _application.Stop();
            Assert.IsTrue(true);
        }

    }
}
