using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanix.Test
{
    [TestClass]
    public class PointMassTest
    {
        [TestMethod]
        public void TestNext_mass1()
        {
            var pm = new PointMass
            (
                new AxisStatus(1, 2, 3), 
                new AxisStatus(2, 3, 4), 
                new AxisStatus(3, 4, 5),
                1
            );

            var pmnext = pm.Next(1, new Force(1, 1, 1), new Force(-1, 2, 0));
            AreEqual(0, pmnext.X.Acceleration);
            AreEqual(3, pmnext.Y.Acceleration);
            AreEqual(1, pmnext.Z.Acceleration);
            AreEqual(5, pmnext.X.Velocity);
            AreEqual(7, pmnext.Y.Velocity);
            AreEqual(9, pmnext.Z.Velocity);
            AreEqual(3, pmnext.X.Position);
            AreEqual(5, pmnext.Y.Position);
            AreEqual(7, pmnext.Z.Position);

            pmnext = pm.Next(0.5, new Force(4, 4, 4));
            AreEqual(4, pmnext.X.Acceleration);
            AreEqual(4, pmnext.Y.Acceleration);
            AreEqual(4, pmnext.Z.Acceleration);
            AreEqual(3.5, pmnext.X.Velocity);
            AreEqual(5, pmnext.Y.Velocity);
            AreEqual(6.5, pmnext.Z.Velocity);
            AreEqual(2, pmnext.X.Position);
            AreEqual(3.5, pmnext.Y.Position);
            AreEqual(5, pmnext.Z.Position);
        }

        [TestMethod]
        public void TestNext_mass0_5()
        {
            var pm = new PointMass
            (
                new AxisStatus(1, 2, 3),
                new AxisStatus(1, 2, 3),
                new AxisStatus(1, 2, 3),
                0.5
            );

            var pmnext = pm.Next(2, new Force(1, 0, 0), new Force(0, 2, 0), new Force(0, 0, 3));
            AreEqual(2, pmnext.X.Acceleration);
            AreEqual(4, pmnext.Y.Acceleration);
            AreEqual(6, pmnext.Z.Acceleration);
            AreEqual(8, pmnext.X.Velocity);
            AreEqual(8, pmnext.Y.Velocity);
            AreEqual(8, pmnext.Z.Velocity);
            AreEqual(5, pmnext.X.Position);
            AreEqual(5, pmnext.Y.Position);
            AreEqual(5, pmnext.Z.Position);
        }
    }
}
