using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanix.Test
{
    [TestClass]
    public class PhysicalContextTest
    {
        [TestMethod]
        public void TestUniformForce()
        {
            var context = new PhysicalContext<int>(0.1, 1);
            var entity = new PointMass
            (
                new AxisStatus(1, 2),
                new AxisStatus(2, 3),
                new AxisStatus(3, 4),
                1
            );
            context.AddEntity
            (
                0,
                entity,
                c => new Force(3, 4, 5)
            );

            AreEqual(entity, context[0]);

            context.Tick();
            AreEqual(entity.Next(0.1, new Force(3, 4, 5)), context[0]);
        }

        [TestMethod]
        public void TestVelicityPropForce()
        {
            var context = new PhysicalContext<int>(0.23, 1);
            var entity = new PointMass
            (
                new AxisStatus(1, 0),
                new AxisStatus(0, 1),
                new AxisStatus(0, 0),
                1
            );
            context.AddEntity
            (
                0,
                entity,
                c => new Force(c[0].X.Velocity, c[0].Y.Velocity, c[0].Z.Velocity) 
            );

            AreEqual(entity, context[0]);

            context.Tick();
            AreEqual(entity.Next(0.23, new Force(0, 1, 0)), context[0]);
        }

        [TestMethod]
        public void TestTickSequential()
        {
            var context = new PhysicalContext<int>(0.23, 1, SimulationParams.None);
            var entity = new PointMass
            (
                new AxisStatus(1, 0),
                new AxisStatus(0, 1),
                new AxisStatus(0, 0),
                1
            );
            context.AddEntity
            (
                0,
                entity,
                c => new Force(c[0].X.Velocity, c[0].Y.Velocity, c[0].Z.Velocity)
            );

            AreEqual(entity, context[0]);

            context.Tick();
            AreEqual(entity.Next(0.23, new Force(0, 1, 0)), context[0]);
        }

        [TestMethod]
        public void TestZeroForce()
        {
            var context = new PhysicalContext<int>(3, 1);
            var entity = new PointMass
            (
                new AxisStatus(0, 1),
                new AxisStatus(0, 1),
                new AxisStatus(0, 1),
                1
            );
            context.AddEntity
            (
                0,
                entity,
                c => new Force(-1, -2, -3),
                c => new Force(1, 2, 3)
            );

            AreEqual(entity, context[0]);
            context.Tick();
            AreEqual(entity.Next(3), context[0]);
            context.Tick();
            AreEqual(entity.Next(3).Next(3), context[0]);
        }

        [TestMethod]
        public void TestThrowingException()
        {
            var context = new PhysicalContext<int>(1, 1);
            var contextSeq = new PhysicalContext<int>(1, 1, SimulationParams.None);
            var entity = new PointMass
            (
                new AxisStatus(0, 1),
                new AxisStatus(0, 1),
                new AxisStatus(0, 1),
                1
            );

            ThrowsException<UninitializedPhysicalContextException<int>>(() => context.Tick());
            ThrowsException<UninitializedPhysicalContextException<int>>(() => contextSeq.Tick());

            context.AddEntity
            (
                0,
                entity,
                // Unexisting key {1} is here
                c => new Force (c[1].X.Position, 0, 0)
            );
            contextSeq.AddEntity
            (
                0,
                entity,
                // Unexisting key {1} is here
                c => new Force(c[1].X.Position, 0, 0)
            );

            ThrowsException<AggregateException>(() => context.Tick());
            ThrowsException<UnexistingEntityException<int>>(() => contextSeq.Tick());
            ThrowsException<FilledPhysicalContextException<int>>(() => context.AddEntity(1, entity));
            ThrowsException<FilledPhysicalContextException<int>>(() => contextSeq.AddEntity(1, entity));
        }

        [TestMethod]
        public void TestBindedEntities()
        {
            var context = new PhysicalContext<int>(1, 2);
            var entity1 = new PointMass
            (
                new AxisStatus(2, 0),
                new AxisStatus(0, 0),
                new AxisStatus(0, 0),
                1
            );
            var entity2 = new PointMass
            (
                new AxisStatus(0, 0),
                new AxisStatus(3, 0),
                new AxisStatus(0, 0),
                1
            );

            context.AddEntity
            (
                1,
                entity1,
                c => new Force(c[2].Y.Position, 0, 0)
            );
            context.AddEntity
            (
                2,
                entity2,
                c => new Force(0, c[1].X.Position, 0)
            );

            context.Tick();
            AreEqual(entity1.Next(1, new Force(3, 0, 0)), context[1]);
            AreEqual(entity2.Next(1, new Force(0, 2, 0)), context[2]);
        }

        [TestMethod]
        public void TestBindedEntities2()
        {
            var context = new PhysicalContext<int>(1, 2);
            var entity1 = new PointMass
            (
                new AxisStatus(0, 1),
                new AxisStatus(0, 1),
                new AxisStatus(0, 1),
                1
            );
            var entity2 = new PointMass
            (
                new AxisStatus(0, 2),
                new AxisStatus(0, 2),
                new AxisStatus(0, 2),
                1
            );

            context.AddEntity
            (
                1,
                entity1,
                (c) => new Force(c[2].X.Velocity, c[2].Y.Velocity, c[2].Z.Velocity)
            );
            context.AddEntity
            (
                2,
                entity2,
                c => new Force(c[1].X.Velocity, c[1].Y.Velocity, c[1].Z.Velocity)
            );

            context.Tick();
            entity1 = entity1.Next(1, new Force(2, 2, 2));
            entity2 = entity2.Next(1, new Force(1, 1, 1));
            AreEqual(entity1, context[1]);
            AreEqual(entity2, context[2]);

            context.Tick();
            entity1 = entity1.Next(1, new Force(3, 3, 3));
            entity2 = entity2.Next(1, new Force(3, 3, 3));
            AreEqual(entity1, context[1]);
            AreEqual(entity2, context[2]);

            context.Tick();
            entity1 = entity1.Next(1, new Force(6, 6, 6));
            entity2 = entity2.Next(1, new Force(6, 6, 6));
            AreEqual(entity1, context[1]);
            AreEqual(entity2, context[2]);
        }

        [TestMethod]
        public void TestTimer()
        {
            var context = new PhysicalContext<int>(0.3, 1);
            context.AddEntity(0, new PointMass());
            AreEqual(0, context.Timer);
            context.Tick();
            context.Tick();
            AreEqual(0.6, context.Timer);
            context.Tick(1);
            AreEqual(1.5, context.Timer);
        }

        [TestMethod]
        public void TestIReadonlyDictionaryInterface()
        {
            var context = new PhysicalContext<int>(1, 2);
            var entity1 = new PointMass
            (
                new AxisStatus(1, 0),
                new AxisStatus(1, 0),
                new AxisStatus(1, 0),
                1
            );
            var entity2 = new PointMass
            (
                new AxisStatus(2, 0),
                new AxisStatus(2, 0),
                new AxisStatus(2, 0),
                1
            );
            context.AddEntity(1, entity1);
            context.AddEntity(2, entity2, c =>
            {
                double val = 0;
                foreach (var key in context.Keys) val += context[key].X.Position;
                return new Force(val, val, val);
            });
        }

        [TestMethod]
        public void TestLocking()
        {
            var context = new PhysicalContext<int>(1, 1);
            var entity = new PointMass();

            context.AddEntity(0, entity, c => { context.Tick(); return new Force(); });

            try
            {
                context.Tick();
            }
            catch (AggregateException e)
            {
                IsTrue(e.InnerExceptions.First() is LockedPhysicalContextException<int>);
            }
        }

        [TestMethod]
        public void TestOnTick()
        {
            var context = new PhysicalContext<int>(1, 1);
            var entity = new PointMass
            (
                new AxisStatus(0, 1),
                new AxisStatus(0, 1),
                new AxisStatus(0, 1),
                1
            );
            context.AddEntity
            (
                0,
                entity,
                c => new Force(0, 1, 2)
            );

            var count = 0;
            context.Tick();

            context.OnTick += (c, e) => count++;
            context.Tick();
            AreEqual(1, count);

            context.OnTick += (c, e) => count++;
            context.Tick();
            AreEqual(3, count);

            context.OnTick += (c, e) => ((PhysicalContext<int>)c).Tick();
            ThrowsException<LockedPhysicalContextException<int>>(() => context.Tick());
        }

        [TestMethod]
        public void TestTickPredicate()
        {
            var context = new PhysicalContext<int>(1, 1);
            var entity = new PointMass
            (
                new AxisStatus(1, 0),
                new AxisStatus(0, 1),
                new AxisStatus(0, 0),
                1
            );
            context.AddEntity
            (
                0,
                entity
            );

            var source = new System.Threading.CancellationTokenSource();
            source.Cancel();

            context.Tick(1, _ => !source.IsCancellationRequested);
            AreEqual(entity, context[0]);
            AreEqual(0u, context.Ticks);

            var isGoalReached = context.Tick(10, c => c.Ticks < 5);
            AreEqual(5u, context.Ticks);
            IsFalse(isGoalReached);

            isGoalReached = context.Tick(10, c => true);
            AreEqual(15u, context.Ticks);
            IsTrue(isGoalReached);
        }
    }
}
