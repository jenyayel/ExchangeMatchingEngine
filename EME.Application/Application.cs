using Autofac;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EME.Infrastructure.Serialization;
using EME.Application.Commands;
using NetMQ.Sockets;
using EME.Models;
using EME.Application.ActorFramework;
using EME.Services;

namespace EME.Application
{
    public class Application : IApplication, IDisposable
    {
        private readonly int c_enginesCount = Environment.ProcessorCount;
        private readonly string c_commandsEndpoint;
        private readonly ILifetimeScope c_scope;
        private readonly IComponentContext c_componentContext;
        private readonly NetMQContext c_mqContext;

        private Poller m_commandsPoller;
        private PullSocket m_commandsSocket;
        private CancellationTokenSource m_cancelToken = new CancellationTokenSource();

        private List<Actor> m_actors;

        public Application(ILifetimeScope scope, IComponentContext componentContext, NetMQContext mqContext, string endpoint)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            if (componentContext == null) throw new ArgumentNullException("componentContext");
            if (mqContext == null) throw new ArgumentNullException("mqContext");
            if (String.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");

            c_scope = scope;
            c_componentContext = componentContext;
            c_mqContext = mqContext;
            c_commandsEndpoint = endpoint;

            Trace.WriteLine("Application starting...");
        }

        public void Run()
        {
            Trace.WriteLine("Application starting...");
            createActors();

            Task.Factory.StartNew(() => { startCommandsSocket(); }, m_cancelToken.Token);
        }

        public void Stop()
        {
            if (m_commandsPoller != null) m_commandsPoller.Stop(true);
            if (m_commandsSocket != null) m_commandsSocket.Close();

            m_cancelToken.Cancel();

            foreach (var _actor in m_actors)
                _actor.Dispose();
        }

        private void startCommandsSocket()
        {
            using (var _scope = c_scope.BeginLifetimeScope())
            {
                Trace.WriteLine("Starting commands socket...");

                m_commandsSocket = c_mqContext.CreatePullSocket();
                m_commandsSocket.Bind(c_commandsEndpoint);

                Trace.WriteLine("Commands socket started at: " + c_commandsEndpoint);

                m_commandsPoller = new Poller();
                m_commandsPoller.AddSocket(m_commandsSocket);

                m_commandsSocket.ReceiveReady += (object sender, NetMQSocketEventArgs e) =>
                {
                    NetMQMessage _message = m_commandsSocket.ReceiveMessage();
                    if (_message == null) return;

                    var _commandType = _message[0].ConvertToString();
                    if (_commandType == OrderCommand.LIMIT_ORDER || _commandType == OrderCommand.MARKET_ORDER)
                    {
                        var _plainPayload = _message[1].ConvertToString();
                        Trace.WriteLine("Received command: " + _plainPayload);

                        var _payload = _plainPayload.FromJSON<OrderCommand>();
                        var _selectedActor = _payload.Symbol.GetHashCode() % c_enginesCount;

                        Trace.WriteLine("Selected actor: " + _selectedActor);

                        // send message to selected actor
                        m_actors[_selectedActor].Send(_plainPayload);
                    }
                    else
                        throw NetMQException.Create("Unexpected command", NetMQ.zmq.ErrorCode.EFAULT);
                };

                m_commandsPoller.Start();
            }
        }

        private void createActors()
        {
            m_actors = new List<Actor>();
            Trace.WriteLine("Creating " + c_enginesCount + "...");

            for (int i = 0; i < c_enginesCount; i++)
            {
                var _handler = c_componentContext.Resolve<IShimHandler>();
                m_actors.Add(new Actor(c_mqContext, _handler));
            }
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
