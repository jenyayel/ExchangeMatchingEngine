using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Infrastructure.Messaging
{
    public interface IMessageBusPublisher
    {
        void Send(string message);

        void Send(byte[] message);
    }
}
