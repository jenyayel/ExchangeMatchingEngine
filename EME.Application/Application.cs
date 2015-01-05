using Autofac;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EME.Application
{
    public class Application : IApplication
    {
        private readonly string m_commandsEndpoint;
        private readonly ILifetimeScope m_scope;
        private readonly IComponentContext m_componentContext;

        public Application(ILifetimeScope scope, IComponentContext componentContext, string endpoint)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            if (componentContext == null) throw new ArgumentNullException("componentContext");
            if (String.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");

            m_scope = scope;
            m_componentContext = componentContext;
            m_commandsEndpoint = endpoint;
        }

        public void Run()
        {
            startCommandsSocket();
        }

        private void startCommandsSocket()
        {
            Task.Factory.StartNew(() =>
            {
                using (var _scope = m_scope.BeginLifetimeScope())
                {
                    var _mqContext = m_componentContext.Resolve<NetMQContext>();
                    var _socket = _mqContext.CreateResponseSocket();
                    _socket.Bind(m_commandsEndpoint);

                    while (true)
                    {
                        Console.WriteLine("Got message: " + _socket.ReceiveString());
                        Thread.Sleep(100);
                    }
                }
            });
        }

        private void startProcessors()
        {
            
        }
    }
}
