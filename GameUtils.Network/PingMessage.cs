using System;
using System.Runtime.Serialization;

namespace Artentus.GameUtils.Network
{
    [Serializable]
    internal class PingMessage : SystemMessage
    {
        public DateTime TimeStamp { get; private set; }

        public PingMessage(DateTime timeStamp)
        {
            TimeStamp = timeStamp;
        }

        protected PingMessage(SerializationInfo info, StreamingContext context)
        {
            TimeStamp = info.GetDateTime("TimeStamp");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TimeStamp", TimeStamp);
        }
    }
}
