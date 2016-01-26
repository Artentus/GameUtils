using System;
using System.Runtime.Serialization;

namespace Artentus.GameUtils.Network
{
    [Serializable]
    internal abstract class SystemMessage : ISerializable
    {
        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}
