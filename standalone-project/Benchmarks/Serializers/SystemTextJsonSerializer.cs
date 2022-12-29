using System;
using System.Text.Json;

namespace Benchmarks.Serializers
{
    public class SystemTextJsonSerializer : Serializer
    {
        System.Text.Json.JsonSerializerOptions option = new()
        {
            IncludeFields = true,
        };

        public override object Serialize<T>(T input) => JsonSerializer.Serialize(input, option);

        public override T Deserialize<T>(object input) => JsonSerializer.Deserialize<T>((byte[])input, option);

        public override string ToString() => "System.Text.Json";
    }
}
