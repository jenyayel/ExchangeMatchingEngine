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
    public class Application : IApplication
    {
        private readonly int c_enginesCount = Environment.ProcessorCount;

        private readonly IComponentContext c_componentContext;
        private readonly NetMQContext c_mqContext;

        private readonly string c_commandsEndpoint;
        private readonly string c_eventsEndpoint;
        private readonly string c_internalEventsEndpoint;

        private PullSocket m_commandsSocket;
        private PullSocket m_internalEventsSocket;

        private CancellationTokenSource m_cancelToken = new CancellationTokenSource();

        private List<Actor> m_actors;

        public Application(
            IComponentContext componentContext,
            NetMQContext mqContext,
            string commandsEndpoint,
            string eventsEndpoint,
            string internalEventsEndpoint)
        {
            if (componentContext == null) throw new ArgumentNullException("componentContext");
            if (mqContext == null) throw new ArgumentNullException("mqContext");
            if (String.IsNullOrEmpty(commandsEndpoint)) throw new ArgumentNullException("commandsEndpoint");
            if (String.IsNullOrEmpty(eventsEndpoint)) throw new ArgumentNullException("eventsEndpoint");
            if (String.IsNullOrEmpty(internalEventsEndpoint)) throw new ArgumentNullException("internalEventsEndpoint");

            c_componentContext = componentContext;
            c_mqContext = mqContext;
            c_commandsEndpoint = commandsEndpoint;
            c_eventsEndpoint = eventsEndpoint;
            c_internalEventsEndpoint = internalEventsEndpoint;
        }

        public void Run()
        {
            Trace.WriteLine("Application starting...");

            // events socket thread with another socket that 
            // listens to internal events
            Task.Factory.StartNew(
                () => startEventsSocket(),
                m_cancelToken.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            
            // wait for internal socket to spinup before connecing engines to it
            Thread.Sleep(500);

            createActors();

            // commands socket thread
            Task.Factory.StartNew(
                () => startCommandsSocket(),
                m_cancelToken.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

        }

        private void startCommandsSocket()
        {
            Trace.WriteLine("Starting commands socket...");

            m_commandsSocket = c_mqContext.CreatePullSocket();
            m_commandsSocket.Bind(c_commandsEndpoint);

            Trace.WriteLine("Commands socket started at: " + c_commandsEndpoint);

            while (!m_cancelToken.IsCancellationRequested)
            {
                NetMQMessage _message = m_commandsSocket.ReceiveMessage();
                if (_message == null) continue;

                var _commandType = _message[0].ConvertToString();
                if (_commandType == OrderCommand.LIMIT_ORDER || _commandType == OrderCommand.MARKET_ORDER)
                {
                    var _plainPayload = _message[1].ConvertToString();
                    Trace.WriteLine("Received command: " + _plainPayload);

                    var _payload = _plainPayload.FromJSON<OrderCommand>();

                    // some basic validations
                    if (_commandType == OrderCommand.LIMIT_ORDER && !_payload.Price.HasValue)
                        throw NetMQException.Create("Must supply price for 'Limit order'", NetMQ.zmq.ErrorCode.EFAULT);

                    // each symbol is asigned for specific actor => pick a relevant actor 
                    var _selectedActor = _payload.Symbol.GetHashCode() % c_enginesCount;

                    Trace.WriteLine("Selected actor: " + _selectedActor);

                    // send message to selected actor
                    m_actors[_selectedActor].Send(_plainPayload);
                }
                else
                    throw NetMQException.Create("Unexpected command", NetMQ.zmq.ErrorCode.EFAULT);
            }
        }

        private void startEventsSocket()
        {
            Trace.WriteLine("Starting events socket...");
            m_commandsSocket = c_mqContext.CreatePullSocket();
            m_commandsSocket.Bind(c_internalEventsEndpoint);
                        
            while (!m_cancelToken.IsCancellationRequested)
            {
                NetMQMessage _message = m_commandsSocket.ReceiveMessage();
                if (_message == null) continue;

                var _eventType = _message[0].ConvertToString();
                var _eventPayload = _message[1].ConvertToString();

                //TODO: send event outside
                Trace.WriteLine("Received internal event " + _eventType);
                
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
            this.Dispose(true);
        }

        /// <summary>
        /// Releases resources. 
        /// </summary>
        /// <param name="disposing"><c>true</c> instructs garbage collector not call the finalizer of the object.; <c>false</c> only release resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            m_cancelToken.Cancel();

            if (m_commandsSocket != null) m_commandsSocket.Close();
            foreach (var _actor in m_actors) _actor.Dispose();
            if (m_internalEventsSocket != null) m_internalEventsSocket.Dispose();

            // Suppress finalization of this disposed instance
            if (disposing)
                GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="NetMQEventsDispatcher" /> class.
        /// Disposable types implement a finalizer. Handle cases when Dispose method was not invoked from calling method. 
        /// <see cref="http://msdn.microsoft.com/en-us/library/ms182269.aspx"/>
        /// </summary>
        ~Application()
        {
            this.Dispose(false);
        }
    }
}
