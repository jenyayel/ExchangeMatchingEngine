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

namespace EME.Application
{
    public class Application : IApplication
    {
        private const string c_inprocAddress = "inproc://bus"; 

        private readonly string c_commandsEndpoint;
        private readonly ILifetimeScope c_scope;
        private readonly IComponentContext c_componentContext;

        public Application(ILifetimeScope scope, IComponentContext componentContext, string endpoint)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            if (componentContext == null) throw new ArgumentNullException("componentContext");
            if (String.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");

            c_scope = scope;
            c_componentContext = componentContext;
            c_commandsEndpoint = endpoint;

            Trace.WriteLine("Application starting...");
        }

        public void Run()
        {
            Task.WaitAll(
                Task.Factory.StartNew(() => { startCommandsSocket(); }));
        }

        private void startCommandsSocket()
        {
            using (var _scope = c_scope.BeginLifetimeScope())
            {
                Trace.WriteLine("Starting commands socket...");
                var _mqContext = c_componentContext.Resolve<NetMQContext>();
                var _socket = _mqContext.CreateResponseSocket();
                _socket.Bind(c_commandsEndpoint);

                Trace.WriteLine("Commands socket started at: " + c_commandsEndpoint);

                while (true)
                {
                    var _message = _socket.ReceiveString();
                    Trace.WriteLine("Command: " + _message);

                    // TODO: find a better way to parse message types
                    var _separatorIndex = _message.IndexOf(":");
                    if (_separatorIndex != -1)
                    {
                        var _type = _message.Substring(0, _separatorIndex);
                        var _object = _message.Substring(_separatorIndex + 1);

                        if (_type == typeof(PlaceLimitOrderCommand).Name)
                        {
                            var _order = _object.FromJSON<PlaceLimitOrderCommand>();
                        }
                        else if (_type == typeof(PlaceMarketOrderCommand).Name)
                        {
                            var _order = _object.FromJSON<PlaceMarketOrderCommand>();
                        }
                        else
                            throw new InvalidOperationException("Type " + _type + " not supported");

                        Trace.WriteLine("Type: " + _type);
                        Trace.WriteLine("Object: " + _object);
                    }
                    Thread.Sleep(100);
                }
            }
        }

        private void startProcessors()
        {

        }
    }
}
