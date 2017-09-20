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

        [TestMethod]
        public void TestBitArraySring()
        {
            var tests = new Tuple<int, int, string>[] // Digit, length, expected
            {
                new Tuple<int, int, string>(5, 4, "0101"),
                new Tuple<int, int, string>(5, 10, "0000000101"),
                new Tuple<int, int, string>(1, 1, "1"),
                new Tuple<int, int, string>(0, 1, "0"),
                new Tuple<int, int, string>(7, 3, "111")
            };

            foreach (var test in tests)
            {
                var bits = BitarrayUtility.IntToBits(test.Item2, test.Item1);
                Assert.AreEqual(test.Item2, bits.Count);
                Assert.AreEqual(test.Item3, BitarrayUtility.BitsToString(bits));
            }
        }

        [TestMethod]
        public void TestBitArraySringBidirectional()
        {
            var tests = new[]
            {
                "10",
                "01",
                "1111",
                "0010100",
                "00001",
                "100010"
            };

            foreach (var test in tests)
            {
                var bits = BitarrayUtility.StringToBits(test);
                var newString = BitarrayUtility.BitsToString(bits);
                Assert.AreEqual(test, newString);
            }
        }

        [TestMethod]
        public void TestHex()
        {
            // input string, testbits, bool expected results
            // testbits are alphabetical
            var tests = new Tuple<string, string>[]
            {
                new Tuple<string, string>("10100010", "A2"), // Test from document is reversed 
                new Tuple<string, string>("01", "01"),
                new Tuple<string, string>("101", "05"),
                new Tuple<string, string>("11111111", "FF"),
                new Tuple<string, string>("111111111", "FF01")
            };

            foreach (var test in tests)
            {
                var bits = BitarrayUtility.StringToBits(test.Item1);
                var hex = BitarrayUtility.BitsToHex(bits);
                Assert.AreEqual(test.Item2, hex,
                    $"Bits to hex didn't get expected for {test.Item1}");
            }
        }

        [TestMethod]
        public void TestTruthInTree()
        {
            // input string, testbits, bool expected results
            // testbits are alphabetical
            var tests = new Tuple<string, string, bool>[]
            {
                new Tuple<string, string, bool>("&(a,b)", "11", true),
                new Tuple<string, string, bool>("&(a,b)", "01", false),
                new Tuple<string, string, bool>("&(a,a)", "1", true),
                new Tuple<string, string, bool>("~(a)", "1", false),
                new Tuple<string, string, bool>("~(|(a,b))", "00", true),
                new Tuple<string, string, bool>("~(|(a,b))", "10", false),
                new Tuple<string, string, bool>("&(&(a,b),|(c,d))", "1110", true),
                new Tuple<string, string, bool>("~(|(&(a,b),>(b,=(c,d))))", "0110", true)
            };

            foreach (var test in tests)
            {
                var tree = TextToTree.Parse(test.Item1);
                var bits = BitarrayUtility.StringToBits(test.Item2);
                var result = TreeToTruthTable.TestTreeValue(tree, bits);
                Assert.AreEqual(test.Item3, result,
                    $"{test.Item1} with inputs {test.Item2} expected {test.Item3}");
            }
        }

        [TestMethod]
        public void TestTruthTable()
        {
            // input string, testbits, bool expected results
            // testbits are alphabetical
            var tests = new Tuple<string, string>[]
            {
                new Tuple<string, string>("&(a,b)", "1000"),
                new Tuple<string, string>("|(a,b)", "1110"),
                new Tuple<string, string>("~(a)", "01"),
                new Tuple<string, string>(">(a,b)", "1011"),
                new Tuple<string, string>("=(a,b)", "1001"),
                new Tuple<string, string>("&(|(a,a),b)", "1000"),
                new Tuple<string, string>("&(=(a,b),c)", "10000010")
            };

            foreach (var test in tests)
            {
                var tree = TextToTree.Parse(test.Item1);
                var truthtable = TreeToTruthTable.CreateTruthTable(tree);
                var bits = BitarrayUtility.BitsToString(truthtable.Values);
                Assert.AreEqual(test.Item2, bits,
                    $"Truthtable is not as expected for {test.Item1}");
            }
        }
    }
}
