using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanix.Laws.Test
{
    [TestClass]
    public class StokesDragLawTest
    {
        [TestMethod]
        public void TestLaw1()
        {
            var context = new PhysicalContext<int>(1, 2);
            context.AddEntity(0, new PointMass(new AxisStatus(0, 1, 0), new AxisStatus(0, -2, 0), new AxisStatus(0, 0, 0), 0));
            context.AddEntity(1, new PointMass(3, 4, 0, 0));

            var law = StokesDragLaw.GetLaw(0, 10);
            AreEqual(new Force(-10, 20, 0), law(context));

            law = StokesDragLaw.GetLaw(1, 3);
            AreEqual(new Force(0, 0, 0), law(context));
        }
    }
}
