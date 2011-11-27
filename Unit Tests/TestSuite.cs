using System;
using System.Collections.Generic;
using System.IO;
using Jurassic;
using Jurassic.Library;
using NUnit.Framework;

namespace UnitTests
{
    public abstract class TestSuite
    {
        private string baseDir;
        protected readonly List<string> testFiles = new List<string>();

        /// <summary>
        /// Creates a new TestSuite instance.
        /// </summary>
        /// <param name="baseDir"> The directory containing the test files. </param>
        public TestSuite(string baseDir)
        {
            if (string.IsNullOrWhiteSpace(baseDir))
                throw new ArgumentNullException("baseDir");
            this.baseDir = Path.Combine(@"..\..\..\Unit Tests\Script-Tests", baseDir);
        }

        /// <summary>
        /// Gets the name of the test suite.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Gets the compatibility mode the tests were designed for.
        /// </summary>
        public virtual CompatibilityMode CompatibilityMode
        {
            get { return CompatibilityMode.Latest; }
        }

        /// <summary>
        /// Gets the base directory containing the test scripts.
        /// </summary>
        public string BaseDirectory
        {
            get { return this.baseDir; }
        }

        /// <summary>
        /// Gets an enumerable list of all the tests in the suite.
        /// </summary>
        public abstract IEnumerable<Test> Tests { get; }

        /// <summary>
        /// Loads the list of tests.  The TotalTestCount property is populated after calling this
        /// method.
        /// </summary>
        protected void EnumerateTests()
        {
            this.testFiles.Clear();
            EnumerateScripts(this.testFiles, this.baseDir);
        }

        /// <summary>
        /// Enumerates all the javascript files in a directory.
        /// </summary>
        /// <param name="allPaths"> The list to all test file paths to. </param>
        /// <param name="dir"> The directory to enumerate. </param>
        private void EnumerateScripts(List<string> allPaths, string dir)
        {
            // Execute all the javascript files.
            foreach (string filePath in Directory.EnumerateFiles(dir, "*.js"))
                allPaths.Add(filePath);

            // Recurse.
            foreach (string dirPath in Directory.EnumerateDirectories(dir))
                EnumerateScripts(allPaths, dirPath);
        }

        /// <summary>
        /// Gets the text encoding of the script files in the test suite.
        /// </summary>
        public abstract System.Text.Encoding ScriptEncoding
        {
            get;
        }

        public abstract FunctionInstance Setup(ScriptEngine engine);

        public void ThrowIfIgnored(Test test)
        {
            if (IsIgnored(test))
                NUnit.Framework.Assert.Ignore(IgnoreReason(test));
        }

        public abstract bool IsIgnored(Test test);
        public abstract string IgnoreReason(Test test);
    }

    public class Test
    {
        private TestSuite suite;
        private string path;

        /// <summary>
        /// Creates a new test.
        /// </summary>
        /// <param name="suite"> The test suite the test is part of. </param>
        /// <param name="path"> The file name of the test script. </param>
        public Test(TestSuite suite, string path)
        {
            if (suite == null)
                throw new ArgumentNullException("suite");
            if (path == null)
                throw new ArgumentNullException("path");
            this.suite = suite;
            this.path = path;
        }

        /// <summary>
        /// Gets the test suite the test is part of.
        /// </summary>
        public TestSuite Suite
        {
            get { return this.suite; }
        }

        /// <summary>
        /// The name of the test.
        /// </summary>
        public string Name
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(this.path); }
        }

        /// <summary>
        /// The file path of the test.
        /// </summary>
        public string Path
        {
            get { return this.path; }
        }

        /// <summary>
        /// Gets or sets a description of what the test is doing.
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Executes this test.
        /// </summary>
        public void Execute()
        {
            ScriptEngine engine = new ScriptEngine();
            engine.CompatibilityMode = suite.CompatibilityMode;
            var run = suite.Setup(engine);
            engine.Execute(new FileScriptSource(path, suite.ScriptEncoding));
            if (run != null)
                run.CallLateBound(engine.Global);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class ES5ConformTestSuite : TestSuite
    {
        private readonly Dictionary<string, string> buggyTests;
        private readonly Dictionary<string, string> wontFixTests;

        /// <summary>
        /// Creates a new ES5ConformTestSuite instance.
        /// </summary>
        public ES5ConformTestSuite()
            : base("es5conform")
        {
            // Create an array of buggy tests.
            this.buggyTests = new Dictionary<string, string>
            {
                //{"7.8.4-1-s",            "Bug in test - http://es5conform.codeplex.com/workitem/28578"},
                //{"10.6-13-b-3-s",        "incorrect property check - http://es5conform.codeplex.com/workitem/29141"},
                //{"10.6-13-c-3-s",        "incorrect property check - http://es5conform.codeplex.com/workitem/29141"},
                {"11.4.1-4.a-4-s",       "assumes this refers to global object - http://es5conform.codeplex.com/workitem/29151"},
                {"11.4.1-5-1-s",         "assumes delete var produces ReferenceError - http://es5conform.codeplex.com/workitem/29084"},
                {"11.4.1-5-2-s",         "assumes delete var produces ReferenceError - http://es5conform.codeplex.com/workitem/29084"},
                {"11.4.1-5-3-s",         "assumes delete var produces ReferenceError - http://es5conform.codeplex.com/workitem/29084"},
                {"11.13.1-1-7-s",        "assumes this is undefined - http://es5conform.codeplex.com/workitem/29152"},
                {"11.13.1-4-2-s",        "gets global object incorrectly - http://es5conform.codeplex.com/workitem/29087"},
                {"11.13.1-4-27-s",       "gets global object incorrectly - http://es5conform.codeplex.com/workitem/29087"},
                {"11.13.1-4-3-s",        "gets global object incorrectly - http://es5conform.codeplex.com/workitem/29087"},
                {"11.13.1-4-4-s",        "gets global object incorrectly - http://es5conform.codeplex.com/workitem/29087"},
                //{"12.2.1-1-s",           "assumes EvalError should be SyntaxError - http://es5conform.codeplex.com/workitem/29089"},
                //{"12.2.1-2-s",           "assumes EvalError should be SyntaxError - http://es5conform.codeplex.com/workitem/29089"},
                //{"12.2.1-3-s",           "assumes EvalError should be SyntaxError - http://es5conform.codeplex.com/workitem/29089"},
                //{"12.2.1-4-s",           "assumes EvalError should be SyntaxError - http://es5conform.codeplex.com/workitem/29089"},
                //{"12.2.1-5-s",           "assumes EvalError should be SyntaxError - http://es5conform.codeplex.com/workitem/29089"},
                //{"12.2.1-6-s",           "assumes EvalError should be SyntaxError - http://es5conform.codeplex.com/workitem/29089"},
                //{"12.2.1-7-s",           "assumes EvalError should be SyntaxError - http://es5conform.codeplex.com/workitem/29089"},
                //{"12.2.1-8-s",           "assumes EvalError should be SyntaxError - http://es5conform.codeplex.com/workitem/29089"},
                //{"12.2.1-9-s",           "assumes EvalError should be SyntaxError - http://es5conform.codeplex.com/workitem/29089"},
                //{"12.2.1-10-s",          "assumes EvalError should be SyntaxError - http://es5conform.codeplex.com/workitem/29089"},
                //{"12.14-5",              "Bug in test - http://es5conform.codeplex.com/workitem/28580"},
                //{"13.1-3-3-s",           "missing return statement - http://es5conform.codeplex.com/workitem/29100"},
                //{"13.1-3-4-s",           "missing return statement - http://es5conform.codeplex.com/workitem/29100"},
                //{"13.1-3-5-s",           "missing return statement - http://es5conform.codeplex.com/workitem/29100"},
                //{"13.1-3-6-s",           "missing return statement - http://es5conform.codeplex.com/workitem/29100"},
                //{"13.1-3-9-s",           "missing return statement - http://es5conform.codeplex.com/workitem/29100"},
                //{"13.1-3-10-s",          "missing return statement - http://es5conform.codeplex.com/workitem/29100"},
                //{"13.1-3-11-s",          "missing return statement - http://es5conform.codeplex.com/workitem/29100"},
                //{"13.1-3-12-s",          "missing return statement - http://es5conform.codeplex.com/workitem/29100"},
                {"15.2.3.3-4-188",        "assumes Function.prototype.name does not exist - http://es5conform.codeplex.com/workitem/28594"},
                //{"15.3.2.1-11-6-s",      "missing return statement - http://es5conform.codeplex.com/workitem/29103"},
                {"15.4.4.14-9.a-1",       "placeholder test - http://es5conform.codeplex.com/workitem/29102"},
                //{"15.4.4.17-4-9",        "asserts Array.prototype.some returns -1 - http://es5conform.codeplex.com/workitem/29143"},
                //{"15.4.4.17-8-10",       "mixes up true and false - http://es5conform.codeplex.com/workitem/29144"},
                //{"15.4.4.19-9-3",        "references undefined variable - http://es5conform.codeplex.com/workitem/29088"},
                //{"15.4.4.21-9-c-ii-4-s", "asserts null should be passed to the callback function - http://es5conform.codeplex.com/workitem/29085"},
                //{"15.4.4.22-9-c-ii-4-s", "asserts null should be passed to the callback function - http://es5conform.codeplex.com/workitem/29085"},
                //{"15.4.4.22-9-1",        "copy and paste error - http://es5conform.codeplex.com/workitem/29146"},
                //{"15.4.4.22-9-7",        "deleted array should still be traversed - http://es5conform.codeplex.com/workitem/26872"},
                //{"15.12.2-0-3",          "precondition does not return true - http://es5conform.codeplex.com/workitem/28581"},
                //{"15.12.3-0-3",          "precondition does not return true - http://es5conform.codeplex.com/workitem/28581"},
            };

            // Create an array of "won't fix" tests.
            this.wontFixTests = new Dictionary<string, string>
            {
                {"15.4.4.14-9-9",        "Array sizes of >= 2^31 are not currently supported."},
                {"15.4.4.15-8-9",        "Array sizes of >= 2^31 are not currently supported."},
            };
        }

        /// <summary>
        /// Gets the name of the test suite.
        /// </summary>
        public override string Name
        {
            get { return "ECMAScript 5 Conformance Suite"; }
        }

        /// <summary>
        /// Gets the text encoding of the script files in the test suite.
        /// </summary>
        public override System.Text.Encoding ScriptEncoding
        {
            get { return System.Text.Encoding.Default; }
        }

        /// <summary>
        /// Gets an enumerable list of all the tests in the suite.
        /// </summary>
        public override IEnumerable<Test> Tests
        {
            get
            {
                EnumerateTests();
                foreach (var path in this.testFiles)
                    yield return new Test(this, path);
            }
        }

        public override bool IsIgnored(Test test)
        {
            return buggyTests.ContainsKey(test.Name) || wontFixTests.ContainsKey(test.Name);
        }

        public override string IgnoreReason(Test test)
        {
            if (buggyTests.ContainsKey(test.Name))
                return buggyTests[test.Name];
            else
                return wontFixTests[test.Name];
        }

        public override FunctionInstance Setup(ScriptEngine engine)
        {
            engine.SetGlobalFunction("fail", new Action<string>(str => NUnit.Framework.Assert.Fail(str)));
            return engine.Evaluate<FunctionInstance>(@"// Create the ES5Harness.registerTest function.
                var _registeredTests = [];
                ES5Harness = {};
                ES5Harness.registerTest = function(test) { _registeredTests.push(test) };

                ES5Harness.runTests = function()
                {
                    for (var i = 0; i < _registeredTests.length; i ++)
                    {
                        if (_registeredTests[i].precondition)
                        {
                            if (_registeredTests[i].precondition() !== true)
                                fail('Precondition for test ' + _registeredTests[i].id + ' failed');
                        }
                        if (_registeredTests[i].test() != true)
                            fail('Test ' + _registeredTests[i].id + ' failed');
                    }
                }

                // Create the fnExists helper function.
                function fnExists() {
                    for (var i=0; i<arguments.length; i++) {
                        if (typeof(arguments[i]) !== ""function"") return false;
                    }
                    return true;
                }

                // Create the fnSupportsStrict helper function.
                function fnSupportsStrict() { return true; }

                // Create the fnGlobalObject helper function.
                function fnGlobalObject() { return (function () {return this}).call(null); }

                // Create the compareArray, compareValues and isSubsetOf helper functions.
                function compareArray(aExpected, aActual) {
                  if (aActual.length != aExpected.length) {
                    return false;
                  }

                  aExpected.sort();
                  aActual.sort();

                  var s;
                  for (var i = 0; i < aExpected.length; i++) {
                    if (aActual[i] !== aExpected[i]) {
                      return false;
                    }
                  }
  
                  return true;
                }

                function compareValues(v1, v2)
                {
                  if (v1 === 0 && v2 === 0)
                    return 1 / v1 === 1 / v2;
                  if (v1 !== v1 && v2 !== v2)
                    return true;
                  return v1 === v2;
                }

                function isSubsetOf(aSubset, aArray) {
                  if (aArray.length < aSubset.length) {
                    return false;
                  }

                  var sortedSubset = [].concat(aSubset).sort();
                  var sortedArray = [].concat(aArray).sort();

                  nextSubsetMember:
                  for (var i = 0, j = 0; i < sortedSubset.length; i++) {
                    var v = sortedSubset[i];
                    while (j < sortedArray.length) {
                      if (compareValues(v, sortedArray[j++])) {
                        continue nextSubsetMember;
                      }
                    }

                    return false;
                  }

                  return true;
                }

                // One test uses 'window' as a synonym for the global object.
                window = this; ES5Harness.runTests");
        }
    }

    public class sputnikTestSuite : TestSuite
    {
        private Dictionary<string, string> buggyTests;
        private Dictionary<string, string> wontFixTests;
        private Dictionary<string, string> includes;

        /// <summary>
        /// Creates a new ES5ConformTestSuite instance.
        /// </summary>
        public sputnikTestSuite()
            : base(@"sputnik\Conformance")
        {
            // Create an array of buggy tests.
            this.buggyTests = new Dictionary<string, string>
            {
                {"S7.8.4_A6.4_T1",       " Asserts '\\X' should throw, which is wrong."},
                {"S7.8.4_A6.4_T2",       " Asserts '\\X' should throw, which is wrong."},
                {"S7.8.4_A7.4_T1",       " Asserts '\\U' should throw, which is wrong."},
                {"S7.8.4_A7.4_T2",       " Asserts '\\U' should throw, which is wrong."},
                {"S7.9_A9_T3",           " 'do { } while (false) true' is not valid, even though all the browsers think it is."},
                {"S7.9_A9_T4",           " 'do { } while (false) true' is not valid, even though all the browsers think it is."},
                {"S12.6.4_A14_T1",       " Assumes that function f() {}.prototype is enumerable (it isn't)."},
                {"S15.8.2.16_A7",        " Accuracy of Math functions is not guaranteed."},
                {"S15.8.2.18_A7",        " Accuracy of Math functions is not guaranteed."},

                // Implementation_Diagnostics
                {"S8.4_D2.2",            " Asserts 'test'[-1] should throw but it should return undefined."},
                {"S11.4.3_D1.2",         " Asserts that typeof RegExp should return 'object' (it should be 'function')."},
                {"S12.6.4_D1",           " Newly added properties are not guarenteed to be included in enumeration."},
                {"S13.2_D1.2",           " Implementations are not required to join identical function instances."},
                {"S13_D1_T1",            " Appears to assume that function declarations do not get moved to the top of the scope."},
                {"S14_D7",               " Function declarations do not and should not respect the current scope."},
                {"S15.5.4.11_D1.1_T1",   " Asserts that String.prototype.replace() should fail if two arguments aren't supplied."},
                {"S15.5.4.11_D1.1_T2",   " Asserts that String.prototype.replace() should fail if two arguments aren't supplied."},
                {"S15.5.4.11_D1.1_T3",   " Asserts that String.prototype.replace() should fail if two arguments aren't supplied."},
                {"S15.5.4.11_D1.1_T4",   " Asserts that String.prototype.replace() should fail if two arguments aren't supplied."},
                {"S15.7.4.5_A1.2_D02",   " Asserts that toFixed(20.1) should fail, but it shouldn't."},
                {"S15.7.4.5_D1.2_T01",   " Asserts that toFixed(20.1) should fail, but it shouldn't."},

                // To loong running (forever loop?)
                {"S12.6.3_A10.1",        "Won't stop running and takes more and more ram."},
                {"S12.6.3_A10",        "Won't stop running and takes more and more ram."},
            };

            // Create an array of "won't fix" tests.
            this.wontFixTests = new Dictionary<string, string>
            {
                {"S7.8.4_A4.3_T1",       " Forbids octal escape sequence in strings (only enabled in compatibility mode)."},
                {"S7.8.4_A4.3_T2",       " Forbids octal escape sequence in strings (only enabled in compatibility mode)."},
                {"S7.8.5_A1.4_T2",       " Regular expression engine needs work."},
                {"S7.8.5_A2.4_T2",       " Regular expression engine needs work."},
                {"S9.4_A3_T1",           " A rewrite of DateInstance is required to fix this one."},
                {"S9.9_A1",              " Asserts that for (var x in undefined) throws a TypeError (not implemented by browsers, changed in ECMAScript 5)."},
                {"S9.9_A2",              " Asserts that for (var x in undefined) throws a TypeError (not implemented by browsers, changed in ECMAScript 5)."},
                {"S11.1.5_A4.1",         " Asserts that keywords are not allowed in object literals (they are in ECMAScript 5)."},
                {"S11.1.5_A4.2",         " Asserts that keywords are not allowed in object literals (they are in ECMAScript 5)."},
                {"S11.8.2_A2.3_T1",      " Asserts relational operator should evaluate right-to-left (spec bug fixed in ECMAScript 5)."},
                {"S11.8.3_A2.3_T1",      " Asserts relational operator should evaluate right-to-left (spec bug fixed in ECMAScript 5)."},
                {"S15.3.4.2_A1_T1",      " Assumes (function() { }).toString() can be compiled using eval()."},
                {"S15.3.4.3_A6_T4",      " Asserts that apply throws a TypeError if the second argument is not an array.  This was changed in ECMAScript 5."},
                {"S15.4.4.2_A2_T1",      " Array.prototype.toString() is generic in ECMAScript 5."},
                {"S15.4.4.3_A2_T1",      " Array.prototype.toLocaleString() is generic in ECMAScript 5."},
                {"S15.4.4.10_A3_T3",     " Arrays > 2^31 are not supported yet."},
                {"S15.4.4.12_A3_T3",     " Arrays > 2^31 are not supported yet."},
                {"S15.4.4.7_A4_T2",      " Arrays > 2^31 are not supported yet."},
                {"S15.4.4.7_A4_T3",      " Arrays > 2^31 are not supported yet."},
                {"S15.10.2.10_A2.1_T3",  " Regular expression engine needs work."},
                {"S15.10.2.10_A5.1_T1",  " Regular expression engine needs work."},
                {"S15.10.2.11_A1_T2",    " Regular expression engine needs work."},
                {"S15.10.2.11_A1_T3",    " Regular expression engine needs work."},
                {"S15.10.2.11_A1_T5",    " Regular expression engine needs work."},
                {"S15.10.2.11_A1_T7",    " Regular expression engine needs work."},
                {"S15.10.2.12_A1_T1",    " Regular expression engine needs work."},
                {"S15.10.2.12_A1_T2",    " Regular expression engine needs work."},
                {"S15.10.2.12_A2_T1",    " Regular expression engine needs work."},
                {"S15.10.2.12_A2_T2",    " Regular expression engine needs work."},
                {"S15.10.2.13_A1_T1",    " Regular expression engine needs work."},
                {"S15.10.2.13_A1_T2",    " Regular expression engine needs work."},
                {"S15.10.2.13_A1_T17",   " Regular expression engine needs work."},
                {"S15.10.2.13_A2_T1",    " Regular expression engine needs work."},
                {"S15.10.2.13_A2_T2",    " Regular expression engine needs work."},
                {"S15.10.2.13_A2_T8",    " Regular expression engine needs work."},
                {"S15.10.2.5_A1_T4",     " Regular expression engine needs work."},
                {"S15.10.4.1_A8_T2",     " Regular expression engine needs work."},
                {"S15.10.6.2_A1_T6",     " Regular expression engine needs work."},
                {"S15.10.6.2_A5_T3",     " Regular expression engine needs work."},
                {"S15.10.6_A2",          " Asserts that Object.prototype.toString.call(/abc/) === '[object Object]'.  This was changed in ECMAScript 5."},
                {"S15.11.1.1_A1_T1",     " Assumes that Error().message doesn't exist (spec bug fixed in ECMAScript 5)."},
                {"S15.11.2.1_A1_T1",     " Assumes that Error().message doesn't exist (spec bug fixed in ECMAScript 5)."},

                {"S11.6.2_A4_T7",        " 1 / Number.MAX_VALUE - 1 / Number.MAX_VALUE = +0  Actual: -0"},

                {"S12.1_A1",             " Function declarations are technically not be allowed except in a top-level context."},
                {"S12.5_A9_T1",          " Function declarations are technically not be allowed except in a top-level context."},
                {"S12.5_A9_T2",          " Function declarations are technically not be allowed except in a top-level context."},
                {"S12.6.1_A13_T1",       " Function declarations are technically not be allowed except in a top-level context."},
                {"S12.6.1_A13_T2",       " Function declarations are technically not be allowed except in a top-level context."},
                {"S12.6.2_A13_T1",       " Function declarations are technically not be allowed except in a top-level context."},
                {"S12.6.2_A13_T2",       " Function declarations are technically not be allowed except in a top-level context."},
                {"S12.6.4_A13_T1",       " Function declarations are technically not be allowed except in a top-level context."},
                {"S12.6.4_A13_T2",       " Function declarations are technically not be allowed except in a top-level context."},

                // Implementation_Diagnostics
                {"S15.1.2.2_D1.2",       " Forbids octal values in parseInt.  This is a de-facto standard (only enabled in compatibility mode)."},
            };
        }

        /// <summary>
        /// Gets the name of the test suite.
        /// </summary>
        public override string Name
        {
            get { return "Sputnik"; }
        }

        /// <summary>
        /// Gets the compatibility mode the tests were designed for.
        /// </summary>
        public override CompatibilityMode CompatibilityMode
        {
            get { return CompatibilityMode.ECMAScript3; }
        }

        /// <summary>
        /// Gets the text encoding of the script files in the test suite.
        /// </summary>
        public override System.Text.Encoding ScriptEncoding
        {
            get { return System.Text.Encoding.UTF8; }
        }

        /// <summary>
        /// Gets an enumerable list of all the tests in the suite.
        /// </summary>
        public override IEnumerable<Test> Tests
        {
            get
            {
                EnumerateTests();
                foreach (var path in this.testFiles)
                    yield return new Test(this, path);
            }
        }

        public override bool IsIgnored(Test test)
        {
            return buggyTests.ContainsKey(test.Name) || wontFixTests.ContainsKey(test.Name);
        }

        public override string IgnoreReason(Test test)
        {
            if (buggyTests.ContainsKey(test.Name))
                return buggyTests[test.Name];
            else
                return wontFixTests[test.Name];
        }

        public override FunctionInstance Setup(ScriptEngine engine)
        {
            // Load the include files (if they haven't already been loaded).
            if (includes == null)
                includes = BuildIncludes(Path.Combine(BaseDirectory, @"..\lib"));

            engine.SetGlobalFunction("$INCLUDE", new Action<string>(name => engine.Execute(includes[name])));
            engine.SetGlobalFunction("$FAIL", new Action<string>(msg => Assert.Fail(msg)));
            engine.SetGlobalFunction("$ERROR", new Action<string>(msg => Assert.Fail(msg)));
            engine.SetGlobalFunction("$PRINT", new Action<string>(msg => Console.WriteLine(msg)));

            return null;
        }

        private static string StripHeader(string source)
        {
            while (source.StartsWith("//") == true)
                source = source.Substring(source.IndexOf('\n') + 1);
            return source;
        }

        private static Dictionary<string, string> BuildIncludes(string dir)
        {
            var includes = new Dictionary<string, string>();
            foreach (string filePath in Directory.EnumerateFiles(dir))
                includes.Add(Path.GetFileName(filePath), StripHeader(File.ReadAllText(filePath)));

            // Set up special include "environment.js" - contains time zone information.
            var environmentJS = new System.Text.StringBuilder();
            environmentJS.AppendFormat("$LocalTZ = {0};" + Environment.NewLine, TimeZoneInfo.Local.BaseUtcOffset.TotalHours);
            var rules = TimeZoneInfo.Local.GetAdjustmentRules();
            if (rules.Length > 0)
            {
                TimeZoneInfo.TransitionTime dstStart = rules[rules.Length - 1].DaylightTransitionStart;
                TimeZoneInfo.TransitionTime dstEnd = rules[rules.Length - 1].DaylightTransitionEnd;
                environmentJS.AppendFormat("$DST_start_month = {0};" + Environment.NewLine, dstStart.Month - 1);
                environmentJS.AppendFormat("$DST_start_sunday = {0};" + Environment.NewLine, CalculateSunday(dstStart));
                environmentJS.AppendFormat("$DST_start_hour = {0};" + Environment.NewLine, dstStart.TimeOfDay.AddSeconds(-1).Hour + 1);
                environmentJS.AppendFormat("$DST_start_minutes = {0};" + Environment.NewLine, (dstStart.TimeOfDay.AddSeconds(-1).Minute + 1) % 60);
                environmentJS.AppendFormat("$DST_end_month = {0};" + Environment.NewLine, dstEnd.Month - 1);
                environmentJS.AppendFormat("$DST_end_sunday = {0};" + Environment.NewLine, CalculateSunday(dstEnd));
                environmentJS.AppendFormat("$DST_end_hour = {0};" + Environment.NewLine, dstEnd.TimeOfDay.AddSeconds(-1).Hour + 1);
                environmentJS.AppendFormat("$DST_end_minutes = {0};" + Environment.NewLine, (dstEnd.TimeOfDay.AddSeconds(-1).Minute + 1) % 60);
            }
            else
            {
                // No daylight savings.
                environmentJS.Append("$DST_start_month = 0;" + Environment.NewLine);
                environmentJS.Append("$DST_start_sunday = 1;" + Environment.NewLine);
                environmentJS.Append("$DST_start_hour = 0;" + Environment.NewLine);
                environmentJS.Append("$DST_start_minutes = 0;" + Environment.NewLine);
                environmentJS.Append("$DST_end_month = 0;" + Environment.NewLine);
                environmentJS.Append("$DST_end_sunday = 1;" + Environment.NewLine);
                environmentJS.Append("$DST_end_hour = 0;" + Environment.NewLine);
                environmentJS.Append("$DST_end_minutes = 0;" + Environment.NewLine);
            }
            includes["environment.js"] = environmentJS.ToString();

            return includes;
        }

        // Returns the number of the sunday, or "last" if it is the last sunday of the month.
        private static string CalculateSunday(TimeZoneInfo.TransitionTime transition)
        {
            if (transition.IsFixedDateRule == true)
                throw new NotSupportedException("Harness does not like fixed DST date rules.");
            if (transition.DayOfWeek != DayOfWeek.Sunday)
                throw new NotSupportedException("Harness does not like non-sunday DST date rules.");
            if (transition.Week == 5)
                return "'last'";
            return (transition.Week - 1).ToString();
        }
    }
}
