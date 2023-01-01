using System;
using System.IO;
using VJson;

namespace Benchmarks.Serializers
{
    public class VJsonSerializer : Serializer
    {
        public override object Serialize<T>(T input)
        {
            var serializer = new JsonSerializer(typeof(T));
            return serializer.SerializeToBytes(input);
        }

        public override T Deserialize<T>(object input)
        {
            var d = new JsonDeserializer(typeof(T));
            return (T)d.DeserializeFromBytes((byte[])input);
        }

        public override string ToString() => "VJson";
    }
}
