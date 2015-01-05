using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Models.Events
{
    public abstract class OrderPlacedEvent
    {
        public Guid OrderId { get; set; }
        public string Symbol { get; set; }
        public int Shares { get; set; }
        public int Type { get; set; } // 0 for buy, 1 for sell
    }
}
