using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Infrastructure.Messaging
{
    public class NetMQPublisher : IMessageBusPublisher, IDisposable
    {
        private NetMQContext m_context;
        private PublisherSocket m_socket;

        public NetMQPublisher(NetMQContext context, string endpoint)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (String.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");

            m_context = context;
            m_socket = m_context.CreatePublisherSocket();

            m_socket.Connect(endpoint);
        }

        public void Send(string message)
        {
            if (String.IsNullOrEmpty(message)) throw new ArgumentNullException("message");
            m_socket.Send(message);
        }

        public void Send(byte[] message)
        {
            if (message == null) throw new ArgumentNullException("message");
            m_socket.Send(message);
        }

        public void Dispose()
        {
            if (m_socket != null) m_socket.Close();            
        }
    }
}
