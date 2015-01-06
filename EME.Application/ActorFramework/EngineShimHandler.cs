using EME.Services;
using EME.Infrastructure.Serialization;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EME.Application.Commands;
using EME.Models;
using NetMQ.zmq;
using System.Diagnostics;

namespace EME.Application.ActorFramework
{
    public class EngineShimHandler : IShimHandler
    {
        IMatchingEngine m_engine;

        public EngineShimHandler(IMatchingEngine engine)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            m_engine = engine;
        }

        public void Run(PairSocket shim, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var _plainPayload = shim.ReceiveString();
                var _payload = _plainPayload.FromJSON<OrderCommand>();

                Trace.WriteLine("Processing command at shim: " + _plainPayload);
                
                m_engine.ProcessOrder(
                    _payload.Type == 0 ? OrderType.Buy : OrderType.Sell,
                    _payload.Symbol,
                    _payload.Shares,
                    _payload.Price);

                //shim.Send(account.ToJSON());
            }
        }
    }
}
