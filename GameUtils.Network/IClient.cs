﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Artentus.GameUtils.Network
{
    public interface IClient : IPackageTransmitter, IPackageReceiver
    {
        void Connect(IPAddress address);

        void Disconnect();
    }
}
