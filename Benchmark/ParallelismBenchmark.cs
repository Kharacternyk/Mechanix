using System;
using BenchmarkDotNet.Attributes;
using Mechanix;

namespace Benchmark
{
    public class ParallelismBenchmark
    {
        const double dt = 0.1;
        readonly Random rand = new Random();

        readonly System.Threading.Tasks.ParallelOptions _paralelOptions = new System.Threading.Tasks.ParallelOptions();

        [Params(true, false)]
        public bool ParallelEntities { get; set; }

        [Params(true, false)]
        public bool ParallelSubscribers { get; set; }

        PhysicalContext<int> context1;
        PhysicalContext<int> context1000;

        [GlobalSetup]
        public void Setup()
        {
            context1 = new PhysicalContext<int>(dt, 1);
            context1000 = new PhysicalContext<int>(dt, 1000);

            context1.AddEntity
            (
                0,
                new PointMass(rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), 1),
                c => new Force(-c[0].X.Position, -c[0].Y.Position, -c[0].Z.Position)
            );
            var depValue = new ContextDependentValue<int, double>(0, context1, (c, p) => c[0].Velocity + p);

            for (int i = 0; i < 1000; ++i)
            {
                context1000.AddEntity
                (
                    i,
                    new PointMass(rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), 1),
                    getForce(i)
                );
                getDepValue(i);
            }


            Func<PhysicalContext<int>, Force> getForce(int i)
            {
                return c => new Force(-c[i].X.Position, -c[i].Y.Position, -c[i].Z.Position);
            }
            ContextDependentValue<int, double> getDepValue(int i)
            {
                return new ContextDependentValue<int, double>(0, context1000, (c, p) => c[i].Velocity + p);
            }
        }

        [Benchmark]
        public void SingleEntity()
        {
            context1.EntitiesParallelOptions = ParallelEntities ? _paralelOptions : null;
            context1.SubscribersParallelOptions = ParallelSubscribers ? _paralelOptions : null;
            context1.Tick(1);
        }

        [Benchmark]
        public void Entities1000()
        {
            context1000.EntitiesParallelOptions = ParallelEntities ? _paralelOptions : null;
            context1000.SubscribersParallelOptions = ParallelSubscribers ? _paralelOptions : null;
            context1000.Tick(1);
        }
    }
}
