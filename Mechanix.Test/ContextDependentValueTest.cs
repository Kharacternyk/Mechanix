using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanix.Test
{
    [TestClass]
    public class ContextDependentValueTest
    {
        [TestMethod]
        public void TestObserving()
        {
            var context = new PhysicalContext<int>(1, 1);
            var entity = new PointMass(new AxisStatus(3, 0, 0), new AxisStatus(1, 1, 0), new AxisStatus(0, 0, 0), 1);
            context.AddEntity(0, entity);

            var max0X = new ContextDependentValue<int, double>
            (
                context[0].X.Position,
                context,
                (c, old) => c[0].X.Position > old ? new double?(c[0].X.Position) : null
            );
            context.Tick();
            context.Tick();

            var max0Y = new ContextDependentValue<int, double>
            (
                context[0].Y.Position,
                context,
                (c, old) => c[0].Y.Position > old ? new double?(c[0].Y.Position) : null
            );

            AreEqual(0ul, max0X.ObservationBeginTick);
            AreEqual(2ul, max0X.LastObservedTick);
            AreEqual(0ul, max0X.LastValueChangeTick);
            AreEqual(3ul, max0X.Value);

            context.Tick();
            context.Tick();

            AreEqual(2ul, max0Y.ObservationBeginTick);
            AreEqual(4ul, max0Y.LastObservedTick);
            AreEqual(4ul, max0Y.LastValueChangeTick);
            AreEqual(5ul, max0Y.Value);

            max0Y.Dispose();

            context.Tick();
            context.Tick();

            AreEqual(2ul, max0Y.ObservationBeginTick);
            AreEqual(4ul, max0Y.LastObservedTick);
            AreEqual(4ul, max0Y.LastValueChangeTick);
            AreEqual(5ul, max0Y.Value);
        }
    }
}
