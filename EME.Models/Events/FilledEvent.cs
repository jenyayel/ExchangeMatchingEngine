using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Models.Events
{
    public class FilledEvent
    {
        public Guid OrderId { get; set; }
        public double AveragePrice { get; set; }
    }
}
