using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Newtonsoft.Json
{
    public static class MemorySerializationExtensions
    {
        public static T MemoryClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }
    }
}