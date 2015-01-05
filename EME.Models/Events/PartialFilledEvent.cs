using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Models.Events
{
    public class PartialFilledEvent
    {
        public Guid BuyOrderId { get; set; }
        public Guid SellOrderId { get; set; }
        public int Shares { get; set; }
        public double Price { get; set; }
    }
}
