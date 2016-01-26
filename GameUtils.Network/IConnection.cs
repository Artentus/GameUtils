using System.Net;
using System.Runtime.Serialization;

namespace Artentus.GameUtils.Network
{
    public interface IConnection : ISerializable
    {
        float Latency { get; }

        IPAddress Address { get; }

        bool Connected { get; }
    }
}
