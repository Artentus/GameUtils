using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Artentus.GameUtils.Network
{
    public static class PackageSerializer
    {
        static readonly BinaryFormatter Formatter;

        static PackageSerializer()
        {
            Formatter = new BinaryFormatter();
        }

        public static void SerializeTo<T>(this T package, Stream stream) where T : IPackage
        {
            Formatter.Serialize(stream, package);
        }

        public static T DeserializeFrom<T>(Stream stream) where T : IPackage
        {
            return (T)Formatter.Deserialize(stream);
        }
    }
}
 