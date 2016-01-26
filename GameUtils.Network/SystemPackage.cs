using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Artentus.GameUtils.Network
{
    [Serializable]
    internal class SystemPackage : IPackage
    {
        public Type ContentType { get; private set; }

        public byte[] Content { get; private set; }

        public long Identifier { get; set; }

        public IConnection[] Connections { get; private set; }

        public void SerializeContent<T>(T content)
        {
            if (!(content is SystemMessage))
                throw new ArgumentException("T");

            ContentType = content.GetType();
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, content);
                Content = ms.ToArray();
            }
        }

        public T DeserializeContent<T>()
        {
            if (!typeof(SystemMessage).IsAssignableFrom(typeof(T)))
                throw new ArgumentException("T");

            using (var ms = new MemoryStream(Content))
            {
                return (T)new BinaryFormatter().Deserialize(ms);
            }
        }

        public SystemPackage(IConnection[] connections)
        {
            if (connections == null)
                throw new ArgumentNullException("connections");

            Connections = connections;
        }

        protected SystemPackage(SerializationInfo info, StreamingContext context)
        {
            ContentType = (Type)info.GetValue("ContentType", typeof(Type));
            Content = (byte[])info.GetValue("Content", typeof(byte[]));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ContentType", ContentType, typeof(Type));
            info.AddValue("Content", Content, typeof(byte[]));
        }
    }
}
