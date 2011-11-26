using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ScriptTests
    {
        [Test, TestCaseSource("ES5ConformSource")]
        public void ES5Conform(Test test)
        {
            test.Execute();
        }

        private static IEnumerable<Test> ES5ConformSource()
        {
            var suite = new ES5ConformTestSuite();
            return suite.Tests.OrderBy(t => t.Name);
        }

        [Test, TestCaseSource("sputnikSource")]
        public void sputnik(Test test)
        {
            test.Execute();
        }

        private static IEnumerable<Test> sputnikSource()
        {
            var suite = new sputnikTestSuite();
            return suite.Tests.OrderBy(t => t.Name);
        }
    }
}
