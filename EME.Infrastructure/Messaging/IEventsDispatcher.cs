﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Infrastructure.Messaging
{
    public interface IEventsDispatcher
    {
        void Send(string type, string message);
    }
}
