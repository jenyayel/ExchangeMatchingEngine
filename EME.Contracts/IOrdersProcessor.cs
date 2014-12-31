using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Contracts
{
    public interface IOrdersProcessor<T>
    {
        void AddOrder(T order);
    }
}
