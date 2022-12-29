using System;
using System.Collections.Generic;
using System.Text;
using Benchmarks.Serializers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    sealed class SomeObject
    {
        public string X;
        public int Y;
    }

    [Config(typeof(BenchmarkConfig))]
    public class AllSerializerBenchmark
    {
        [ParamsSource(nameof(Serializers))]
        public Serializer Serializer;

        public IEnumerable<Serializer> Serializers => new Serializer[]
        {
            new VJsonSerializer(),
            new SystemTextJsonSerializer(),
            new SpanJsonSerializer(),
            new NetJSONSerializer(),
        };

        private readonly byte[] ib = Encoding.UTF8.GetBytes("255");
        private readonly byte b = 255;
        private readonly long l = 255L;

        private readonly byte[] sb = Encoding.UTF8.GetBytes("\"test\"");
        private readonly string s = "test";

        private readonly SomeObject o = new SomeObject { X = "test", Y = 100 };
        private readonly byte[] so = Encoding.UTF8.GetBytes("{\"X\":\"test\",\"Y\":100}");

        [Benchmark]
        public object DeserializeToByte() => this.Serializer.Deserialize<byte>(ib);

        [Benchmark]
        public object DeserializeToLong() => this.Serializer.Deserialize<long>(ib);

        [Benchmark]
        public object DeserializeToString() => this.Serializer.Deserialize<string>(sb);

        [Benchmark]
        public object DeserializeToObject() => this.Serializer.Deserialize<SomeObject>(so);

        [Benchmark]
        public object SerializeByte() => this.Serializer.Serialize(b);

        [Benchmark]
        public object SerializeLong() => this.Serializer.Serialize(l);

        [Benchmark]
        public object SerializeString() => this.Serializer.Serialize(s);

        [Benchmark]
        public object SerializeObject() => this.Serializer.Serialize(o);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<AllSerializerBenchmark>();
        }
    }
}
