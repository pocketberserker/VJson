using System;
using System.Collections.Generic;
using System.Text;
using Benchmarks.Serializers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
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

        private readonly byte[] i = Encoding.UTF8.GetBytes("255");
        private readonly string s = "test";
        private readonly byte b = 255;
        private readonly long l = 255L;

        [Benchmark]
        public object IntegerToByte() => this.Serializer.Deserialize<byte>(i);

        [Benchmark]
        public object IntegerToLong() => this.Serializer.Deserialize<long>(i);

        [Benchmark]
        public object SerializeByte() => this.Serializer.Serialize(b);

        [Benchmark]
        public object SerializeLong() => this.Serializer.Serialize(l);

        [Benchmark]
        public object SerializeString() => this.Serializer.Serialize(s);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<AllSerializerBenchmark>();
        }
    }
}
