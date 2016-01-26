using System;
using System.Runtime.Serialization;

namespace Artentus.GameUtils.Network
{
    public interface IPackage : ISerializable
    {
        Type ContentType { get; }

        byte[] Content { get; }

        long Identifier { get; set; }

        void SerializeContent<T>(T content);

        T DeserializeContent<T>();
    }
}
