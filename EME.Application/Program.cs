using Autofac;
using System;
using System.Diagnostics;

namespace EME.Application
{
    class Program
    {
        private const string commandsEndpoint = "tcp://:8881";
        private const string eventsEndpoint = "tcp://:8882";

        static void Main(string[] args)
        {
            Console.Clear();

            Trace.Listeners.Add(new ConsoleTraceListener());

            using (var _app = IocConfig
                .CreateDefaultContainer(commandsEndpoint, eventsEndpoint)
                .Resolve<IApplication>())
            {
                _app.Run();
                Console.ReadKey();
            }
        }
    }
}
