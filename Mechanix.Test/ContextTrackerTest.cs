using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanix.Test
{
    [TestClass]
    public class ContextTrackerTest
    {
        [TestMethod]
        public void TestInit1()
        {
            var context = new PhysicalContext<int>(0.1, 1);
            var entity = new PointMass(new AxisStatus(1, 3, 0), new AxisStatus(0, 0, 0), new AxisStatus(0, 0, 0), 1);
            context.AddEntity(0, entity);

            var tracker = new ContextTracker<int, double>(context, c => c[0].X.Position);
            AreEqual(1, tracker[0]);
            AreEqual(0ul, tracker.ObservationBeginTick);
            ThrowsException<ArgumentOutOfRangeException>(() => tracker[2]);

            context.Tick(0.5);
            AreEqual(1, tracker[0]);
            var pos05 = context[0].X.Position;
            AreEqual(pos05, tracker[5]);
            AreEqual(pos05, tracker.GetApproximately(0.5));

            context.Tick(0.5);
            AreEqual(1, tracker[0]);
            AreEqual(pos05, tracker.GetApproximately(0.5));
            AreEqual(pos05, tracker.GetApproximately(0.478));
            AreEqual(pos05, tracker.GetApproximately(0.505));
            AreEqual(context[0].X.Position, tracker[context.Ticks]);
        }

        [TestMethod]
        public void TestDispose()
        {
            var context = new PhysicalContext<int>(0.1, 2);
            var entity = new PointMass(0, 0, 0, 1);
            context.AddEntity(0, entity, c => new Force(1, 0, 0));
            context.AddEntity(1, entity, c => new Force(1, 0, 0));

            context.Tick(1);

            var tracker = new ContextTracker<int, int>(context, c => (int)Math.Round(c[0].X.Velocity + c[1].X.Velocity));
            AreEqual(2, tracker[context.Ticks]);
            AreEqual(10ul, tracker.ObservationBeginTick);
            ThrowsException<ArgumentOutOfRangeException>(() => tracker.GetApproximately(0));
            ThrowsException<ArgumentOutOfRangeException>(() => tracker.GetApproximately(2));

            context.Tick(1);
            AreEqual(2, tracker.GetApproximately(1));
            AreEqual(1, tracker.ObservationBeginTime);
            AreEqual(4, tracker[context.Ticks]);
            AreEqual(20ul, tracker.LastObservedTick);

            tracker.Dispose();

            context.Tick(1);
            AreEqual(20ul, tracker.LastObservedTick);
            ThrowsException<ArgumentOutOfRangeException>(() => tracker[context.Ticks]);
        }

        [TestMethod]
        public void TestStoryboard()
        {
            var context = new PhysicalContext<int>(1, 1);
            var entity = new PointMass(new AxisStatus(0, 1, 0), new AxisStatus(1, 0, 0), new AxisStatus(0, 0, 0), 1);
            context.AddEntity(0, entity);

            var tracker = new ContextTracker<int, double>(context, c => c[0].X.Position);
            AreEqual(1, tracker.Count);
            IsTrue(tracker.Keys.Contains<ulong>(0));

            context.Tick();
            AreEqual(2, tracker.Count);
            IsTrue(tracker.Keys.Contains<ulong>(0));
            IsTrue(tracker.Keys.Contains<ulong>(1));
            IsFalse(tracker.Keys.Contains<ulong>(2));
        }
    }
}
