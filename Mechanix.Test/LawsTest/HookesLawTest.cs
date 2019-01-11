using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanix.Laws.Test
{
    [TestClass]
    public class HookesLawTest
    {
        [TestMethod]
        public void TestLaw1()
        {
            var context = new PhysicalContext<int>(1, 2);
            context.AddEntity(0, new PointMass(0, 0, 0, 0));
            context.AddEntity(1, new PointMass(3, 4, 0, 0));

            var law = HookesLaw.GetLaw(1, 0, undeformedDistance: 5, elasticityCoefficient: 1);
            AreEqual(new Force(0, 0, 0), law(context));

            law = HookesLaw.GetLaw(1, 0, undeformedDistance: 10, elasticityCoefficient: 1);
            AreEqual(new Force(3, 4, 0), law(context));

            law = HookesLaw.GetLaw(1, 0, undeformedDistance: 10, elasticityCoefficient: 2);
            AreEqual(new Force(6, 8, 0), law(context));

            law = HookesLaw.GetLaw(1, 0, undeformedDistance: 0, elasticityCoefficient: 0.1);
            AreEqual(new Force(-0.3, -0.4, 0), law(context));
        }

        [TestMethod]
        public void TestLaw2()
        {
            var context = new PhysicalContext<int>(1, 2);
            context.AddEntity(0, new PointMass(1, 1, 1, 0));
            context.AddEntity(1, new PointMass(1, 1, 2, 0));

            var law = HookesLaw.GetLaw(1, 0, undeformedDistance: 0, elasticityCoefficient: 1);
            AreEqual(new Force(0, 0, -1), law(context));

            law = HookesLaw.GetLaw(1, 0, undeformedDistance: 3, elasticityCoefficient: 2);
            AreEqual(new Force(0, 0, 4), law(context));

            law = HookesLaw.GetLaw(1, 0, undeformedDistance: 12, elasticityCoefficient: 0);
            AreEqual(new Force(0, 0, 0), law(context));
        }
    }
}
