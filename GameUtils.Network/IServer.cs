using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Artentus.GameUtils.Network
{
    public interface IServer : IPackageTransmitter
    {
        void Shutdown();
    }
}
