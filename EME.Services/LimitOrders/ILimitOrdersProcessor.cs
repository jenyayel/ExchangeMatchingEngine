﻿using EME.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Services
{
    public interface ILimitOrdersProcessor
    {
        LimitOrder ProcessOrder(OrderType orderType, string symbol, int shares, decimal price);
    }
}