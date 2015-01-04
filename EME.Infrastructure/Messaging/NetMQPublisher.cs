﻿using NetMQ;
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
        ~NetMQPublisher()
        {
            this.Dispose(false);
        }
    }
}