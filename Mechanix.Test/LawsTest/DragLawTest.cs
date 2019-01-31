using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanix.Laws.Test
{
    [TestClass]
    public class DragLawTest
    {
        [TestMethod]
        public void TestLaw1()
        {
            var context = new PhysicalContext<int>(1, 2);
            context.AddEntity(0, new PointMass(new AxisStatus(0, 3), new AxisStatus(0, 0), new AxisStatus(0, 4), 0));
            context.AddEntity(1, new PointMass(3, 4, 0, 0));

            var law = DragLaw.GetLaw(0, 10);
            AreEqual(new Force(-150, 0, -200), law(context));

            law = DragLaw.GetLaw(1, 3);
            AreEqual(new Force(0, 0, 0), law(context));
        }
    }
}
