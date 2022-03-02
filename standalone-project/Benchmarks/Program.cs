using System;
using System.Collections.Generic;
using System.Text;
using Benchmarks.Serializers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class Deserializer
    {
        [ParamsSource(nameof(Serializers))]
        public Serializer Serializer;

        public IEnumerable<Serializer> Serializers => new Serializer[]
        {
            new VJsonSerializer(),
            new SystemTextJsonSerializer(),
            new SpanJsonSerializer(),
        };

        private readonly byte[] i = Encoding.UTF8.GetBytes("255");

        [Benchmark]
        public object IntegerToByte() => this.Serializer.Deserialize<byte>(i);

        [Benchmark]
        public object IntegerToLong() => this.Serializer.Deserialize<long>(i);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Deserializer>();
        }
    }
}
