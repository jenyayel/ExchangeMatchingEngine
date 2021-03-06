﻿using NetMQ;
using System;
using System.Collections.Generic;
using EME.Infrastructure.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Application.Commands
{
    public class OrderCommand
    {
        public const string LIMIT_ORDER = "PlaceLimitOrderCommand";
        public const string MARKET_ORDER = "PlaceMarketOrderCommand";

        public string Symbol { get; set; }
        public int Shares { get; set; }
        
        /// <summary>
        /// 0 for buy, 1 for sell
        /// </summary>
        public int Type { get; set; }
        public double? Price { get; set; }
    }
}
