using Autofac;
using EME.Application.ActorFramework;
using EME.Infrastructure.Messaging;
using EME.Infrastructure.Persistance;
using EME.Services;
using NetMQ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Application
{
    public static class IocConfig
    {
        private const string c_inprocEvetsEndpoint = "inproc://eventsbus";

        public static IContainer CreateDefaultContainer(string commandsEndpoint, string eventsEndpoint)
        {
            var _builder = new ContainerBuilder();

            _builder
                .RegisterType<Application>()
                .As<IApplication>()
                .SingleInstance()
                .WithParameter("commandsEndpoint", commandsEndpoint)
                .WithParameter("eventsEndpoint", eventsEndpoint)
                .WithParameter("internalEventsEndpoint", c_inprocEvetsEndpoint);
            
            _builder
                .RegisterInstance(NetMQContext.Create())
                .As<NetMQContext>()
                .SingleInstance();

            _builder
               .RegisterType<NetMQEventsDispatcher>()
               .As<IEventsDispatcher>()
               .WithParameter("endpoint", c_inprocEvetsEndpoint);

            _builder
               .RegisterType<MatchingEngine>()
               .As<IMatchingEngine>();

            _builder
               .RegisterType<EngineShimHandler>()
               .As<IShimHandler>();

            _builder
               .RegisterGeneric(typeof(InMemoryRepository<>))
               .As(typeof(IRepository<>))
               .SingleInstance();

            return _builder.Build();
        }
    }
}
