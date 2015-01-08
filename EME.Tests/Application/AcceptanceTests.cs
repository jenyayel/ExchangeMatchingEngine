using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EME.Application;
using NetMQ;
using NetMQ.Sockets;
using Autofac;
using EME.Infrastructure.Serialization;
using EME.Application.Commands;
using System.Threading;
using EME.Models.Events;

namespace EME.Tests.Application
{
    [TestClass]
    public class AcceptanceTests
    {
        private const string commandsEndpoint = "tcp://localhost:8881";
        private const string eventsEndpoint = "tcp://localhost:8882";

        [TestMethod]
        public void FinalAcceptanceTest()
        {
            var _container = IocConfig
              .CreateDefaultContainer(commandsEndpoint, eventsEndpoint);

            var _context = _container.Resolve<NetMQContext>();

            using (var _application = _container.Resolve<IApplication>())
            {
                _application.Run();

                using (var _commandsClient = _context.CreatePushSocket())
                {
                    _commandsClient.Connect(commandsEndpoint);

                    using (var _eventsClient = _context.CreatePullSocket())
                    {
                        _eventsClient.Connect(eventsEndpoint);

                        // -- command 1 --
                        _commandsClient.SendMore(OrderCommand.LIMIT_ORDER);
                        _commandsClient.Send(new OrderCommand
                        {
                            Type = 0,
                            Shares = 10,
                            Symbol = "MSFT",
                            Price = 50
                        }.ToJSON());
                        Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());

                        // -- command 2 --
                        _commandsClient.SendMore(OrderCommand.MARKET_ORDER);
                        _commandsClient.Send(new OrderCommand
                        {
                            Type = 1,
                            Shares = 5,
                            Symbol = "MSFT"
                        }.ToJSON());
                        Assert.AreEqual(typeof(MarketOrderPlacedEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());
                        Assert.AreEqual(typeof(PartialFilledEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());
                        Assert.AreEqual(typeof(FilledEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());

                        // -- command 3 --
                        _commandsClient.SendMore(OrderCommand.LIMIT_ORDER);
                        _commandsClient.Send(new OrderCommand
                        {
                            Type = 1,
                            Shares = 5,
                            Symbol = "MSFT",
                            Price = 50
                        }.ToJSON());
                        Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());
                        Assert.AreEqual(typeof(PartialFilledEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());
                        Assert.AreEqual(typeof(FilledEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());
                        Assert.AreEqual(typeof(FilledEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());

                        // -- command 4 --
                        _commandsClient.SendMore(OrderCommand.LIMIT_ORDER);
                        _commandsClient.Send(new OrderCommand
                        {
                            Type = 1,
                            Shares = 10,
                            Symbol = "MSFT",
                            Price = 50
                        }.ToJSON());
                        Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());

                        // -- command 5 --
                        _commandsClient.SendMore(OrderCommand.LIMIT_ORDER);
                        _commandsClient.Send(new OrderCommand
                        {
                            Type = 0,
                            Shares = 10,
                            Symbol = "MSFT",
                            Price = 52
                        }.ToJSON());
                        Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());
                        var _message = _eventsClient.ReceiveMessage();
                        Assert.AreEqual(typeof(PartialFilledEvent).Name, _message[0].ConvertToString());
                        Assert.AreEqual(51, _message[1].ConvertToString().FromJSON<PartialFilledEvent>().Price);
                        Assert.AreEqual(typeof(FilledEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());
                        Assert.AreEqual(typeof(FilledEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());

                        // -- command 6 --
                        _commandsClient.SendMore(OrderCommand.LIMIT_ORDER);
                        _commandsClient.Send(new OrderCommand
                        {
                            Type = 1,
                            Shares = 10,
                            Symbol = "MSFT",
                            Price = 50
                        }.ToJSON());
                        Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());

                        // -- command 7 --
                        _commandsClient.SendMore(OrderCommand.LIMIT_ORDER);
                        _commandsClient.Send(new OrderCommand
                        {
                            Type = 0,
                            Shares = 10,
                            Symbol = "MSFT",
                            Price = 48
                        }.ToJSON());
                        Assert.AreEqual(typeof(LimitOrderPlacedEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());

                        // -- command 8 --
                        _commandsClient.SendMore(OrderCommand.MARKET_ORDER);
                        _commandsClient.Send(new OrderCommand
                        {
                            Type = 1,
                            Shares = 10,
                            Symbol = "MSFT"
                        }.ToJSON());
                        Assert.AreEqual(typeof(MarketOrderPlacedEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());
                        _message = _eventsClient.ReceiveMessage();
                        Assert.AreEqual(typeof(PartialFilledEvent).Name, _message[0].ConvertToString());
                        Assert.AreEqual(48, _message[1].ConvertToString().FromJSON<PartialFilledEvent>().Price);
                        Assert.AreEqual(typeof(FilledEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());
                        Assert.AreEqual(typeof(FilledEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());

                        // -- command 9 --
                        _commandsClient.SendMore(OrderCommand.MARKET_ORDER);
                        _commandsClient.Send(new OrderCommand
                        {
                            Type = 0,
                            Shares = 10,
                            Symbol = "MSFT"
                        }.ToJSON());
                        Assert.AreEqual(typeof(MarketOrderPlacedEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());
                        _message = _eventsClient.ReceiveMessage();
                        Assert.AreEqual(typeof(PartialFilledEvent).Name, _message[0].ConvertToString());
                        Assert.AreEqual(50, _message[1].ConvertToString().FromJSON<PartialFilledEvent>().Price);
                        Assert.AreEqual(typeof(FilledEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());
                        Assert.AreEqual(typeof(FilledEvent).Name, _eventsClient.ReceiveMessage()[0].ConvertToString());
                    }                    
                }
            }
        }
    }
}
