using System;
using System.Runtime.Serialization;

namespace Artentus.GameUtils.Network.Udp
{
    [Serializable]
    internal class UdpMessage : SystemMessage
    {
        public bool IsConnectMessage { get; private set; }

        public UdpMessage(bool isConnectMessage)
        {
            IsConnectMessage = isConnectMessage;
        }

        protected UdpMessage(SerializationInfo info, StreamingContext context)
        {
            IsConnectMessage = info.GetBoolean("IsConnectMessage");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("IsConnectMessage", IsConnectMessage);
        }
    }
}
