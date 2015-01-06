using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Application.Commands
{
    public class PlaceLimitOrderCommand : OrderCommand
    {
        public double Price { get; set; }
    }
}
