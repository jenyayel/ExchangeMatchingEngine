using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Application
{
    public interface IApplication : IDisposable
    {
        void Run();
    }
}
