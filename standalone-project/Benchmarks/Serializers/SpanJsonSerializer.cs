using System;
using SpanJson;

namespace Benchmarks.Serializers
{
    public class SpanJsonSerializer : Serializer
    {
        public override object Serialize<T>(T input) => JsonSerializer.Generic.Utf8.Serialize(input);

        public override T Deserialize<T>(object input) => JsonSerializer.Generic.Utf8.Deserialize<T>((byte[])input);

        public override string ToString() => "SpanJson";
    }
}
