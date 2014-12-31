using EME.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Contracts
{
    public interface IMarketOrdersProcessor : IOrdersProcessor<MarketOrder>
    {
    }
}
