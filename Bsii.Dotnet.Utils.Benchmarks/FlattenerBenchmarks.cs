using BenchmarkDotNet.Attributes;
using Bsii.Dotnet.Utils.Reflection;
using System;
using System.Collections.Generic;

namespace Bsii.Dotnet.Utils.Benchmarks
{
    [MemoryDiagnoser]
    public class FlattenerBenchmarks
    {
        class SmallClass
        {
            public int Prop1 { get; set; }
            public double Prop2 { get; set; }
            public short Prop3 { get; set; }
            public string Prop4 { get; set; }
            public DateTime Prop5 { get; set; }
            public TimeSpan Prop6 { get; set; }
            public NestedClass Prop7 { get; set; }
            public NestedClass Prop8 { get; set; }
            public Dictionary<string, string> Prop9 { get; set; }
            public Dictionary<string, NestedClass> Prop10 { get; set; }
        }

        class NestedClass
        {
            public int Prop1 { get; set; }
            public double Prop2 { get; set; }
        }

        private SmallClass _smallClassInstance;

        [GlobalSetup]
        public void Setup()
        {
            _smallClassInstance = new SmallClass
            {
                Prop1 = 1,
                Prop2 = 2.4,
                Prop3 = -3,
                Prop4 = "Hello",
                Prop5 = DateTime.UtcNow,
                Prop6 = TimeSpan.FromMilliseconds(765),
                Prop7 = new NestedClass { Prop1 = 9, Prop2 = -762.1 },
                Prop8 = new NestedClass { Prop1 = 8124, Prop2 = 772.5 },
                Prop9 = new Dictionary<string, string> { ["Hello"] = "World", ["Test"] = "String" },
                Prop10 = new Dictionary<string, NestedClass> { ["Abc"] = new NestedClass { Prop1 = 11, Prop2 = 741.2 }, ["Def"] = new NestedClass { Prop1 = 89, Prop2 = -999.2 } }
            };
        }


        [Benchmark]
        public Dictionary<string, object> FlattenSmallObject() => ObjectFlattener.FlattenToDictionary(_smallClassInstance);
    }
}
