using Autofac;
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
        public static IContainer CreateDefaultContainer(string commandsEndpoint, string eventsEndpoint)
        {
            var _builder = new ContainerBuilder();

            _builder
                .RegisterType<Application>()
                .As<IApplication>()
                .SingleInstance()
                .WithParameter("endpoint", commandsEndpoint);
            
            _builder
                .RegisterInstance(NetMQContext.Create())
                .As<NetMQContext>()
                .SingleInstance();

            _builder
               .RegisterType<NetMQPublisher>()
               .As<IMessageBusPublisher>()
               .InstancePerLifetimeScope()
               .WithParameter("endpoint", eventsEndpoint);

            _builder
               .RegisterType<MatchingEngine>()
               .As<IMatchingEngine>()
               .InstancePerLifetimeScope();

            _builder
               .RegisterGeneric(typeof(InMemoryRepository<>))
               .As(typeof(IRepository<>))
               .InstancePerLifetimeScope();

            return _builder.Build();
        }
    }
}
