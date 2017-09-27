using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ale1.Functional;

namespace Ale1.Parser.Test
{
    [TestClass]
    public class NandifyTest
    {
        [TestMethod]
        public void TestNand()
        {
            var tests = new Tuple<string, string>[]
            {
                new Tuple<string, string>("&(a,b)", "%(%(a,b),%(a,b))"),
                new Tuple<string, string>("%(a,b)", "%(a,b)"),
                new Tuple<string, string>("|(a,b)", "%(%(a,a),%(b,b))"),
                new Tuple<string, string>(">(a,b)", "%(%(%(a,a),%(a,a)),%(b,b))"),
                new Tuple<string, string>("=(a,b)", "%(%(%(%(%(a,a),%(b,b)),%(%(a,a),%(b,b))),%(%(%(a,a),%(b,b)),%(%(a,a),%(b,b)))),%(%(%(a,b),%(a,b)),%(%(a,b),%(a,b))))")
            };

            foreach (var test in tests)
            {
                var tree = TextToTree.Parse(test.Item1);
                var nandified = NandifyTree.Nandify(tree);
                var result = TreeToText.ToText(nandified);
                Assert.AreEqual(test.Item2, result, "Not expected proposition");
                var hashcode = BitarrayUtility.BitsToHex(TreeToTruthTable.CreateTruthTable(tree).Values);
                var nandhashcode = BitarrayUtility.BitsToHex(TreeToTruthTable.CreateTruthTable(nandified).Values);
                Assert.AreEqual(hashcode, nandhashcode, $"Truthtables are not the same for {test.Item1} and {test.Item2} ");
            }
        }
    }
}
