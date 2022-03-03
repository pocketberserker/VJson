using System.Text;

namespace Benchmarks.Serializers
{
    public class NetJSONSerializer : Serializer
    {
        public override object Serialize<T>(T input) => NetJSON.NetJSON.Serialize(input);

        public override T Deserialize<T>(object input)
        {
            var str = Encoding.UTF8.GetString((byte[])input);
            return NetJSON.NetJSON.Deserialize<T>(str);
        }

        public override string ToString() => "NetJSON";
    }
}
