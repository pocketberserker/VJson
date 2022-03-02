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
            return serializer.Serialize(input);
        }

        public override T Deserialize<T>(object input)
        {
            using(var ms = new MemoryStream((byte[])input))
            {
                var d = new JsonDeserializer(typeof(T));
                return (T)d.Deserialize(ms);
            }
        }

        public override string ToString() => "VJson";
    }
}
