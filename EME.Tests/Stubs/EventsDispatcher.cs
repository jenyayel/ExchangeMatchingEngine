using EME.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EME.Tests.Stubs
{
    internal class EventsDispatcher : IEventsDispatcher
    {
        public event EventHandler<EventMessageArgs> MessageReceived;

        public void Send(string type, string message)
        {
            if (MessageReceived != null)
                MessageReceived(this, new EventMessageArgs { Type = type, Message = message });
        }
    }

    internal class EventMessageArgs : EventArgs
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }
}
