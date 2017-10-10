using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ale1.Functional;

namespace Ale1.Parser.Test
{
    [TestClass]
    public class DisjunctiveTest
    {
        [TestMethod]
        public void TestDisjunctiveHash()
        {
            var tests = new[]
            {
                "|(a,b)",
                "|(dank,memes)",
                "|(a,|(b,c))",
                "~(a)",
                "&(a,b)",
                "=(a,b)",
                ">(a,b)",
                "~(>(a,b))",
                "%(a,b)",
                "=(a,&(b,c))",
                //"|(a,|(~(b),b))", // tautology cant be tested this way
                //"&(a,&(~(b),b))", // not tautology 
                "=(|(a,b),c)",
                ">(>(a,b),c)",
                ">(>(a,b),>(c,d))",
                ">(=(|(a,b),c),&(d,e))",
                "~(>(=(|(a,b),c),&(d,e)))"
            };
            foreach (var test in tests)
            {
                // check both disjunctive functions
                // create initial proposition
                var truthable = TreeToTruthTable.CreateTruthTable(TextToTree.Parse(test));
                var simple = TruthTableSimplifier.toSimpleTruthTable(truthable);
                // disjunctive forms
                var disjunctive = TruthTableToDisjunctive.TruthTableToDisjunctive(truthable);
                var simpledisjunctive = SimpleTruthTableToDisjunctive.TruthTableToDisjunctive(simple);
                // make new truthtables
                var truthabledis = TreeToTruthTable.CreateTruthTable(TextToTree.Parse(disjunctive));
                var simpletruthabledis = TreeToTruthTable.CreateTruthTable(TextToTree.Parse(simpledisjunctive));
                // Hex values
                var expected = BitarrayUtility.BitsToHex(truthable.Values);
                var dishex = BitarrayUtility.BitsToHex(truthabledis.Values);
                var simpledishex = BitarrayUtility.BitsToHex(simpletruthabledis.Values);
                Assert.AreEqual(expected, dishex, $"Truthable: {test}");
                Assert.AreEqual(expected, simpledishex, $"Simple truthable: {test}");
            }
        }
    }
}
