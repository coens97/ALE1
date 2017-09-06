using Ale1.Functional;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ale1.Parser.Test
{
    [TestClass]
    public class TruthTableTest
    {
        [TestMethod]
        public void TestTreeVariables()
        {
            var tests = new Tuple<string, string[]>[] {
                new Tuple<string, string[]>("&(a,a)", new [] { "a" }),
                new Tuple<string, string[]>(" &  ( a , a )  ", new [] { "a" }),
                new Tuple<string, string[]>("~(dank memes)", new [] { "dankmemes" }),
                new Tuple<string, string[]>("&(Dab,|(F,a))", new [] { "a", "Dab", "F" }),
                new Tuple<string, string[]>("&(car,|(bike,&(Train, car)))", new [] { "bike", "car", "Train" })
            };

            foreach (var test in tests)
            {
                var tree = TextToTree.Parse(test.Item1);
                var results = TreeToTruthTable.AllTreeVariables(tree);
                Assert.IsTrue(Enumerable.SequenceEqual(test.Item2, results), 
                    $"To tree variable doesn't give expected result, expected [{string.Join(",", test.Item2)}] but got {results.ToString()}");
                Assert.IsFalse(Enumerable.SequenceEqual(test.Item2, new[] { "This should always fail" }), "Compare lists fails");
            }
        }
    }
}
