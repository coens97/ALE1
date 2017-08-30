using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ale1.Parser.Test
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void ParserTestAll()
        {
            var vectors = ParserTestVectors.GetVectors;
            foreach (var test in vectors)
            {
                var result = TextToTree.Parse(test.Item1);
                Assert.AreEqual(test.Item2, result, "blabla");
            }
        }
    }
}
