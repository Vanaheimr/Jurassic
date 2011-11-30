using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Jurassic;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ScriptTests
    {
        Process process;
        private readonly object plock = new object();
        private volatile int pCount = 0;

        public Process GetRunner()
        {
            int c;
            lock (plock)
                c = pCount;
            if (c >= 10)
                TearDown();

            lock (plock)
            {
                if (process == null)
                {
                    ProcessStartInfo psi = new ProcessStartInfo("TestAssembly.exe");
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.ErrorDialog = false;
                    psi.RedirectStandardError = true;
                    psi.RedirectStandardInput = true;
                    psi.RedirectStandardOutput = true;
                    //psi.StandardErrorEncoding = Encoding.UTF8;
                    //psi.StandardOutputEncoding = Encoding.UTF8;
                    process = new Process();
                    process.StartInfo = psi;
                    Console.WriteLine("Starting runner.");
                    process.Start();
                    pCount = 0;
                }
                else
                {
                    pCount++;
                }
            }
            return process;
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            if (process != null)
                lock (plock)
                {
                    if (process != null)
                    {
                        Console.WriteLine("Killing runner.");
                        try
                        {
                            process.Kill();
                        }
                        catch { }
                        process = null;
                    }
                }
        }

        [Test, TestCaseSource(typeof(Test262Suite), "GetTests")] // Don't let it run for longer than 2 minutes.
        public void Js265Test(string includes, string content, bool forceStrictMode, bool isNegative, string negativeReturnType)
        {
            var p = GetRunner();
            p.StandardInput.Write(includes);
            p.StandardInput.Write(Environment.NewLine);
            p.StandardInput.Write(content);
            p.StandardInput.Write(Environment.NewLine);
            p.StandardInput.Write("::end::");
            p.StandardInput.Write(Environment.NewLine);
            p.StandardInput.Flush();

            string result;
            if (WaitForResult(p, TimeSpan.FromMinutes(2), out result))
            {
                if (result != "::pass::")
                {
                    if (isNegative)
                    {
                        if (negativeReturnType == null)
                            return;

                        string firstLine = result.Split(new char[] { '\r', '\n' }, 2, StringSplitOptions.RemoveEmptyEntries)[0];
                        if (!firstLine.StartsWith("Jurassic.JavaScriptException: "))
                            Assert.Fail(result);

                        int startPos = "Jurassic.JavaScriptException: ".Length;
                        string errorType = firstLine.Substring(startPos, firstLine.IndexOf(':', startPos) - startPos);
                        if (!Regex.IsMatch(errorType, negativeReturnType))
                            Assert.Fail("Wrong exception:{2}  Expected: {0}{2}  Received: {1}", negativeReturnType, errorType, Environment.NewLine);
                    }
                }
                else
                {
                    if (isNegative)
                        Assert.Fail("Expected exception");
                }
            }
            else
            {
                TearDown();
                throw new TimeoutException("Opperation timed out.");
            }
            /*ScriptEngine engine = new ScriptEngine();
            engine.ForceStrictMode = forceStrictMode;
            engine.Execute(includes);
            try
            {
                Run(() => engine.Execute(content), engine);
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
                Assert.Fail("An exception was expected.");*/
        }

        private bool WaitForResult(Process p, TimeSpan timeout, out string result)
        {
            string res = "";
            DateTime exitTime = DateTime.Now + timeout;
            Thread t = new Thread((ThreadStart)delegate
            {
                StringBuilder sb = new StringBuilder();
                for (; ; )
                {
                    var l = p.StandardOutput.ReadLine();
                    Console.WriteLine(l);
                    if (l == "::fin::")
                        break;
                    sb.AppendLine(l);
                }
                sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length);
                res = sb.ToString();
            });
            t.Start();
            bool ret = t.Join(timeout);
            if (!ret)
                t.Interrupt();
            result = res;
            return ret;
        }

        private void Run(Action action, ScriptEngine engine)
        {
            Exception e = null;
            var t = new Thread((ThreadStart)delegate
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    if (ex is ThreadAbortException)
                        e = new TimeoutException("The method was aborted.");
                    else
                        e = ex;
                }
            });
            t.Start();
            if (!t.Join(TimeSpan.FromSeconds(10)))
            {
                Console.WriteLine("This is abort!");
                engine.ShutDown();
                t.Abort();
                Console.WriteLine("Aborted!");
                throw new TimeoutException("Operation timed out.");
            }
            if (e != null)
            {
                throw e;
            }
        }
    }
}
