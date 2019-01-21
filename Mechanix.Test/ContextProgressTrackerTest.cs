using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanix.Test
{
    [TestClass]
    public class ContextProgressTrackerTest
    {
        [TestMethod]
        public void TestGoal()
        {
            var context = new PhysicalContext<int>(1, 1);
            context.AddEntity(0, new PointMass());

            context.Tick();
            context.Tick();

            var tracker = new ContextProgressTracker<int>(context, 10);

            AreEqual(0, tracker.Progress);

            bool goal = false;
            tracker.OnGoal += (c, e) => goal = true;

            context.Tick(8);
            AreEqual(1, tracker.Progress);
            IsFalse(tracker.IsActive);
            IsTrue(goal);
        }

        [TestMethod]
        public void TestCheckPoints()
        {
            var context = new PhysicalContext<int>(0.5, 1);
            context.AddEntity(0, new PointMass());

            context.Tick();

            ThrowsException<ArgumentOutOfRangeException>(() => new ContextProgressTracker<int>(context, 10, 0));
            ThrowsException<ArgumentOutOfRangeException>(() => new ContextProgressTracker<int>(context, 10, 12));

            context.Tick();
            var trackerInt = new ContextProgressTracker<int>(context, 10, 3, 9);
            var trackerDouble = ContextProgressTracker<int>.FromTime(context, 10, 3, 9);
            int reachedInt = 0, reachedDouble = 0;
            trackerInt.OnCheckPoint += (c, _) => reachedInt++;
            trackerDouble.OnCheckPoint += (c, _) => reachedDouble++;

            for (int i = 0; i < 12; i++) context.Tick();
            AreEqual(2, reachedInt);
            AreEqual(1, reachedDouble);
        }
    }
}
