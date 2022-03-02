using System;
using System.Text.Json;

namespace Benchmarks.Serializers
{
    public class SystemTextJsonSerializer : Serializer
    {
        public override object Serialize<T>(T input) => JsonSerializer.Serialize(input);

        public override T Deserialize<T>(object input) => JsonSerializer.Deserialize<T>((byte[])input);

        public override string ToString() => "System.Text.Json";
    }
}
