using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Mechanix;

namespace Benchmark
{
    public class SharedValueBenchmark
    {
        const double dt = 0.1;
        readonly Random rand = new Random();

        readonly System.Threading.Tasks.ParallelOptions _paralelOptions = new System.Threading.Tasks.ParallelOptions();

        [Params(true, false)]
        public bool ParallelEntities { get; set; }

        PhysicalContext<int> context1000;
        PhysicalContext<int> context1000Shared;

        [GlobalSetup]
        public void Setup()
        {
            context1000 = new PhysicalContext<int>(dt, 1000);
            context1000Shared = new PhysicalContext<int>(dt, 1000);
            var valShared = new ContextSharedValue<int, double>(context1000Shared, c => Math.Pow(Math.Log(c.Timer) + Math.Log10(c.Timer), Math.E));

            for (int i = 0; i < 1000; ++i)
            {
                context1000.AddEntity
                (
                    i,
                    new PointMass(rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), 1),
                    getForce(i)
                );
                context1000Shared.AddEntity
                (
                    i,
                    new PointMass(rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), 1),
                    getSharedForce(i)
                );
            }

            Func<PhysicalContext<int>, Force> getForce(int i)
            {
                return c =>
                {
                    var val = Math.Pow(Math.Log(c.Timer) + Math.Log10(c.Timer), Math.E);
                    return new Force(-c[i].X.Position * val, -c[i].Y.Position * val, -c[i].Z.Position * val);
                };
            }

            Func<PhysicalContext<int>, Force> getSharedForce(int i)
            {
                return c => new Force(-c[i].X.Position * valShared.Value, -c[i].Y.Position * valShared.Value, -c[i].Z.Position * valShared.Value);
            }
        }

        [Benchmark]
        public void NonShared()
        {
            context1000.EntitiesParallelOptions = ParallelEntities ? _paralelOptions : null;
            context1000.Tick(1);
        }

        [Benchmark]
        public void Shared()
        {
            context1000Shared.EntitiesParallelOptions = ParallelEntities ? _paralelOptions : null;
            context1000Shared.Tick(1);
        }
    }
}
