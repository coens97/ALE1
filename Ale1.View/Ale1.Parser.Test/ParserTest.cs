using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ale1.Parser.Test
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
        public void ParserToText()
        {
            var vectors = ParserTestVectors.GetVectors;
            foreach (var test in vectors)
            {
                var textResult = TreeToText.ToText(test.Item2);

                Assert.AreEqual(test.Item1, textResult,
                    "The TreeToText method does not give the expected result");
                Assert.AreNotEqual("A result never given", textResult, "Can't compare type string"); // If assumption holds that compare string works
            }
        }
    }
}
