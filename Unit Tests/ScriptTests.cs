using System.Collections.Generic;
using System.IO;
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

        private static IEnumerable<TestCaseData> ES5ConformSource()
        {
            var suite = new ES5ConformTestSuite();
            return suite.Tests.OrderBy(t => t.Name).Select(t =>
            {
                var tcd = new TestCaseData(t).SetName(t.Name);
                if (suite.IsIgnored(t))
                    tcd.Ignore(suite.IgnoreReason(t));

                return tcd;
            });
        }

        [Test, TestCaseSource("sputnikSource")]
        public void sputnik(Test test)
        {
            test.Execute();
        }

        private static IEnumerable<TestCaseData> sputnikSource()
        {
            var suite = new sputnikTestSuite();
            return suite.Tests.OrderBy(t => t.Name).Select(t =>
            {
                var tcd = new TestCaseData(t).SetName(t.Name);
                if (suite.IsIgnored(t))
                    tcd = tcd.Ignore(suite.IgnoreReason(t));

                var content = File.ReadAllText(t.Path);
                if (content.Contains("@negative"))
                    tcd = tcd.Throws(typeof(Jurassic.JavaScriptException));
                int assertionStart = content.IndexOf("@assertion:") + "assertion: ".Length;
                tcd = tcd.SetProperty("_ASSERTION", content.Substring(assertionStart, content.IndexOfAny(new char[] { '\r', '\n' }, assertionStart) - assertionStart));
                int descriptionStart = content.IndexOf("@description:") + "@description: ".Length;
                return tcd.SetDescription(content.Substring(descriptionStart, content.IndexOfAny(new char[] { '\r', '\n' }, descriptionStart) - descriptionStart));
            });
        }
    }
}
