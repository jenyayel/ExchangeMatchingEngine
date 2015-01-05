using EME.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Services
{
    public interface IMatchingEngine
    {
        void ProcessOrder(OrderType orderType, string symbol, int shares, double? price);
    }
}
