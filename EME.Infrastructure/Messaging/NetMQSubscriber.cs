using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Infrastructure.Messaging
{
    public class NetMQSubscriber : IMessageBusSubscriber, IDisposable
    {
        private NetMQContext m_context;
        private SubscriberSocket m_socket;

        public NetMQSubscriber(NetMQContext context, string endpoint, params string[] topics)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (String.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");

            m_context = context;
            m_socket = m_context.CreateSubscriberSocket();
            m_socket.Bind(endpoint);

            if (topics == null || topics.Length == 0)
                m_socket.Subscribe(String.Empty);
            else
                foreach (var topic in topics)
                    m_socket.Subscribe(topic);
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
            if (m_socket == null)
                return;

            m_socket.Dispose();
            m_context.Dispose();
            // Suppress finalization of this disposed instance
            if (disposing)
                GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="NetMQPublisher" /> class.
        /// Disposable types implement a finalizer. Handle cases when Dispose method was not invoked from calling method. 
        /// <see cref="http://msdn.microsoft.com/en-us/library/ms182269.aspx"/>
        /// </summary>
        ~NetMQSubscriber()
        {
            this.Dispose(false);
        }
    }
}
