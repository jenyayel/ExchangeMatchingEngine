using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EME.Application.ActorFramework
{
    /// <summary>
    /// Simple interface that all shims should implement
    /// </summary>
    public interface IShimHandler
    {
        void Run(PairSocket shim, CancellationToken token);
    }
}
