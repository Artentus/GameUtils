using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Artentus.GameUtils.Network
{
    public enum NotificationType
    {
        ClientConnect,
        ClientDisconnect,
        ServerShuttown,
        Timeout,
        ClientList,
    }
}
