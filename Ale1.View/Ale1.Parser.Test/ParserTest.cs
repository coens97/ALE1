﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ale1.Functional.Test
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void ParserTestAll()
        {
            var vectors = ParserTestVectors.GetVectors
                .Concat(ParserTestVectors.GetSpecialVectors);
            foreach (var test in vectors)
            {
                var result = TextToTree.Parse(test.Item1);
                var textResult = TreeToText.ToText(result);
                var expectedTextResult = TreeToText.ToText(test.Item2);

                Assert.AreEqual(expectedTextResult, textResult, "The expected Tree is not generated");
            }
        }

        [TestMethod]
        public void ParserTestSyntax() // test for syntax errors
        {
            var vectors = ParserTestVectors.GetSyntaxErrorVectors;
            foreach (var test in vectors)
            {
                Assert.ThrowsException<ArgumentException>(() => TextToTree.Parse(test), 
                    $"Syntax error should have been given for: {test}");
            }
        }

        [TestMethod]
        public void ParserToTextInfix()
        {
            var infuxTest = new Tuple<string, string>[]
            {
                new Tuple<string, string>("~(abc)","~(abc)"),
                new Tuple<string, string>("&(1,a)","(1&a)"),
                new Tuple<string, string>("&(abc,bde)","(abc&bde)"),
                new Tuple<string, string>("&(~(dank memes),|(fontys,think bigger))","(~(dankmemes)&(fontys|thinkbigger))")
            };
            foreach (var test in infuxTest)
            {
                var tree = TextToTree.Parse(test.Item1); // text -> tree -> text
                var textResult = TreeToText.ToTextInfix(tree);

                Assert.AreEqual(test.Item2, textResult,
                    "The TreeToText method does not give the expected result");
                Assert.AreNotEqual("A result never given", textResult, "Can't compare type string"); // If assumption holds that compare string works
            }
        }
    }
}
