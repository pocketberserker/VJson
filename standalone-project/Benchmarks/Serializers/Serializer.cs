using System;


namespace Benchmarks.Serializers
{
    public abstract class Serializer
    {
        public abstract object Serialize<T>(T input);
        public abstract T Deserialize<T>(object input);
    }
}
