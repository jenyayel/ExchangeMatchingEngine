using NetMQ;
using NetMQ.Sockets;
using NetMQ.zmq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EME.Application.ActorFramework
{
    /// <summary>
    /// The Actor represents one end of a two way pipe between 2 PairSocket(s). Where
    /// the actor may be passed messages, that are sent to the other end of the pipe
    /// which I am calling the "shim"
    /// </summary>
    public class Actor : IOutgoingSocket, IReceivingSocket, IDisposable
    {
        private const string c_inprocEndpointFormat = "inproc://zactor-{0}-{1}";

        private readonly PairSocket m_pairSocket;
        private readonly Shim m_shim;
        private CancellationTokenSource m_cancelToken = new CancellationTokenSource();
        private Random m_rand = new Random();

        public Actor(NetMQContext context, IShimHandler shimHandler)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (shimHandler == null) throw new ArgumentNullException("shimHandler");

            m_pairSocket = context.CreatePairSocket();
            m_shim = new Shim(shimHandler, context.CreatePairSocket());
            m_pairSocket.Options.SendHighWatermark = 1000;
            m_pairSocket.Options.SendHighWatermark = 1000;

            //now binding and connect pipe ends
            var _endpoint = String.Empty;
            while (true)
            {
                Action bindAction = () =>
                {
                    _endpoint = getEndPointName();
                    m_pairSocket.Bind(_endpoint);
                };

                try
                {
                    bindAction();
                    break;
                }
                catch (NetMQException nex)
                {
                    if (nex.ErrorCode == ErrorCode.EFAULT)
                        bindAction();
                }

            }

            m_shim.Pipe.Connect(_endpoint);

            //Create Shim thread handler
            createShimThread();
        }

        private void createShimThread()
        {
            Task.Factory.StartNew(
                () => this.m_shim.Handler.Run(this.m_shim.Pipe, m_cancelToken.Token),
                m_cancelToken.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default)
                .ContinueWith(ant =>
                {
                    if (ant.Exception == null) return;

                    Exception baseException = ant.Exception.Flatten().GetBaseException();
                    if (baseException.GetType() == typeof(NetMQException))
                    {
                        Trace.WriteLine(string.Format("NetMQException caught : {0}",
                            baseException.Message));
                    }
                    else if (baseException.GetType() == typeof(ObjectDisposedException))
                    {
                        Trace.WriteLine(string.Format("ObjectDisposedException caught : {0}",
                            baseException.Message));
                    }
                    else
                    {
                        Trace.WriteLine(string.Format("Exception caught : {0}",
                            baseException.Message));
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private string getEndPointName()
        {
            return String.Format(c_inprocEndpointFormat,
                m_rand.Next(0, 10000), m_rand.Next(0, 10000));
        }

        ~Actor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            //cancel shim thread
            m_cancelToken.Cancel();

            // release other disposable objects
            if (disposing)
            {
                if (m_pairSocket != null) m_pairSocket.Dispose();
                if (m_shim != null) m_shim.Dispose();
            }
        }

        public void Send(ref Msg msg, SendReceiveOptions options)
        {
            m_pairSocket.Send(ref msg, options);
        }

        public void Receive(ref Msg msg, SendReceiveOptions options)
        {
            m_pairSocket.Receive(ref msg, options);
        }
    }
}