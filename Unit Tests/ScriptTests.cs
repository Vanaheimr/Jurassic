using System.Text.RegularExpressions;
using Jurassic;
using Jurassic.Library;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ScriptTests
    {
        [Test, TestCaseSource(typeof(Test262Suite), "GetTests")]
        public void Js265Test(string includes, string content, bool forceStrictMode, bool isNegative, string negativeReturnType)
        {
            ScriptEngine engine = new ScriptEngine();
            engine.ForceStrictMode = forceStrictMode;
            engine.Execute(includes);
            try
            {
                engine.Execute(content);
            }
            catch (JavaScriptException e)
            {
                if (isNegative)
                {
                    if (negativeReturnType == null)
                        return;

                    string exceptionType = TypeConverter.ToString(((ObjectInstance)e.ErrorObject)["name"]);
                    Assert.IsTrue(Regex.IsMatch(exceptionType, negativeReturnType), "  Expected: {0}\n    But was:  {1}", negativeReturnType, exceptionType);
                }
                else
                    Assert.Fail("An exception was not expected. Exception: {0}", e);
            }
            if (isNegative)
                Assert.Fail("An exception was expected.");
        }
    }
}
