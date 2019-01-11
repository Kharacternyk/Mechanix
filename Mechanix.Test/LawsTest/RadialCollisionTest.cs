using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanix.Laws.Test
{
    [TestClass]
    public class RadialCollisionTest
    {
        [TestMethod]
        public void TestLaw1()
        {
            var context = new PhysicalContext<int>(1, 2);
            context.AddEntity(0, new PointMass(0, 0, 0, 0));
            context.AddEntity(1, new PointMass(3, 4, 0, 0));

            var law = RadialCollisionLaw.GetLaw(1, 0, criticalDistance: 3, elasticityCoefficient: 1);
            AreEqual(new Force(0, 0, 0), law(context));

            law = RadialCollisionLaw.GetLaw(1, 0, criticalDistance: 10, elasticityCoefficient: 1);
            AreEqual(new Force(3, 4, 0), law(context));

            law = RadialCollisionLaw.GetLaw(1, 0, criticalDistance: 5, elasticityCoefficient: 1);
            AreEqual(new Force(0, 0, 0), law(context));

            law = RadialCollisionLaw.GetLaw(1, 0, criticalDistance: 6, elasticityCoefficient: 50);
            AreEqual(new Force(30, 40, 0), law(context));
        }
    }
}
