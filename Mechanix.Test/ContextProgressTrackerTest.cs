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
            var context = new PhysicalContext<int>(1, 1);
            context.AddEntity(0, new PointMass());

            context.Tick();
            ThrowsException<ArgumentOutOfRangeException>(() => new ContextProgressTracker<int>(context, 10, 0));

            context.Tick();
            var tracker = new ContextProgressTracker<int>(context, 10, 3, 5, 12);
            var reached = 0;
            tracker.OnCheckPoint += (c, _) => reached++;

            context.Tick(12);
            AreEqual(2, reached);
        }
    }
}
