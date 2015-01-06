using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EME.Application;
using NetMQ;
using NetMQ.Sockets;
using Autofac;
using EME.Infrastructure.Serialization;
using EME.Application.Commands;

namespace EME.Tests.Application
{
    [TestClass]
    public class AcceptanceTests : IDisposable
    {
        private const string commandsEndpoint = "tcp://localhost:8881";
        private const string eventsEndpoint = "tcp://:8882";

        private NetMQContext m_context;
        private NetMQSocket m_client;

        [TestInitialize]
        public void Init()
        {
            IocConfig
               .CreateDefaultContainer(commandsEndpoint, eventsEndpoint)
               .Resolve<IApplication>()
               .Run();

            m_context = NetMQContext.Create();
            m_client = m_context.CreateRequestSocket();
            m_client.Connect(commandsEndpoint);
        }

        [TestMethod]
        public void FinalAcceptanceTest()
        {
            m_client.Send(new PlaceLimitOrderCommand
            {
                Type = 0,
                Shares = 10,
                Symbol = "MSFT",
                Price = 50
            }.ToJSONMessage());
        }

        public void Dispose()
        {
            if (m_client != null) m_client.Dispose();
            if (m_context != null) m_context.Dispose();
        }
    }
}
