using Ale1.Functional;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ale1.Parser.Test
{
    [TestClass]
    public class SimplifierTest
    {
        [TestMethod]
        public void TestArraySlice()
        {
            Assert.IsTrue(Enumerable.SequenceEqual(new[] { FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(true) },
                new[] { FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(true) }), "Test equal arrays");
            Assert.IsFalse(Enumerable.SequenceEqual(new[] { FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(true) },
                new[] { FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(false) }), "Test unequal arrays");
            // This tuple almost deserve a class
            var tests = new Tuple<int, int, FSharpOption<bool>[], FSharpOption<bool>[]>[]{
                new Tuple<int, int, FSharpOption<bool>[], FSharpOption<bool>[]>(
                    0,1, 
                    new [] {FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(true)},
                    new FSharpOption<bool>[0]),
                new Tuple<int, int, FSharpOption<bool>[], FSharpOption<bool>[]>(
                    1,2,
                    new [] { FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(true)},
                    new [] { FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(true) }),
                new Tuple<int, int, FSharpOption<bool>[], FSharpOption<bool>[]>(
                    1,2,
                    new [] { FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(true)},
                    new [] { FSharpOption<bool>.Some(false)})
            };
            //TruthTableSimplifier.sliceRow(0,1, test)
            foreach (var row in tests)
            {
                var result = TruthTableSimplifier.sliceRow(row.Item1, row.Item2, row.Item3);
                Assert.IsTrue(Enumerable.SequenceEqual(row.Item4, result));
            }
        }

        [TestMethod]
        public void TestArraySliceInsert()
        {
            // This tuple almost deserve a class
            var tests = new Tuple<int, int, FSharpOption<bool>[]>[]{
                new Tuple<int, int, FSharpOption<bool>[]>(
                    0,1,
                    new [] {FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(true)}),
                new Tuple<int, int, FSharpOption<bool>[]>(
                    1,2,
                    new [] { FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(true)}),
                new Tuple<int, int, FSharpOption<bool>[]>(
                    1,2,
                    new [] { FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(false)}),
                new Tuple<int, int, FSharpOption<bool>[]>(
                    2,1,
                    new [] { FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(false)}),
                new Tuple<int, int, FSharpOption<bool>[]>(
                    3,0,
                    new [] { FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(true)}),
                new Tuple<int, int, FSharpOption<bool>[]>(
                    1,0,
                    new [] { FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(true)}),
                new Tuple<int, int, FSharpOption<bool>[]>(
                    3,1,
                    new [] { FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(true), FSharpOption<bool>.Some(false), FSharpOption<bool>.Some(true)})
            };
            //TruthTableSimplifier.sliceRow(0,1, test)
            foreach (var row in tests)
            {
                var firstValue = row.Item3[row.Item1];
                var secondValue = row.Item3[row.Item2];
                // Slice value out
                var slicedresult = TruthTableSimplifier.sliceRow(row.Item1, row.Item2, row.Item3);
                // Put value back in
                var result = TruthTableSimplifier.insertInRow(row.Item1, firstValue, row.Item2, secondValue, slicedresult);
                Assert.IsTrue(Enumerable.SequenceEqual(row.Item3, result), 
                    $"Failed at {row.Item1}, {row.Item2}, {TruthTableSimplifier.rowToString(row.Item3)} got {TruthTableSimplifier.rowToString(result)}");
            }
        }
    }
}