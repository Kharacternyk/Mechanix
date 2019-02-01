using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanix.Test
{
    [TestClass]
    public class ForceTest
    {
        [TestMethod]
        public void TestOperators()
        {
            Force f1 = new Force(1, 3, -2);
            Force f2 = new Force(0, -1, 1);

            AreEqual(new Force(1, 2, -1), f1 + f2);
            AreEqual(new Force(1, 2, -1), f1.Add(f2));
            AreEqual(new Force(1, 2, -1), f2.Add(f1));

            AreEqual(new Force(1, 4, -3), f1 - f2);
            AreEqual(new Force(1, 4, -3), f1.Substract(f2));
            AreEqual(new Force(1, 4, -3), Force.Zero - f2.Substract(f1));

            AreEqual(Force.Zero, f1.Multiply(0));
            AreEqual(Force.Zero, f1 * 0);
            AreEqual(new Force(2, 6, -4), f1 * 2);
        }

        [TestMethod]
        public void TestSerialization()
        {
            var f = new Force(12, 222, 0.2);

            var formatter = new BinaryFormatter();
            var stream = new System.IO.MemoryStream();

            formatter.Serialize(stream, f);
            stream.Position = 0;
            var newf = formatter.Deserialize(stream);

            AreEqual(f, newf);
        }
    }
}
