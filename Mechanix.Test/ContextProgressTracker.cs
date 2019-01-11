using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanix.Test
{
    [TestClass]
    public class ContextProgressTracker
    {
        [TestMethod]
        public void Test()
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
    }
}
