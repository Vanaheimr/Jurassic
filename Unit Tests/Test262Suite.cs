using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace UnitTests
{
    public static class Test262Suite
    {
        public static IEnumerable<TestCaseData> GenerateTests()
        {
            var file = Directory.GetFiles(@"..\..\..\Unit Tests\Script-Tests", "*.zip").First();
            var zipfile = new ZipFile(file);

            var includeBuilder = new StringBuilder();
            includeBuilder.AppendLine(File.ReadAllText(Path.Combine(@"..\..\..\Unit Tests\Script-Tests", @"harness\cth.js")));
            includeBuilder.AppendLine(File.ReadAllText(Path.Combine(@"..\..\..\Unit Tests\Script-Tests", @"harness\sta.js")));
            includeBuilder.AppendLine(File.ReadAllText(Path.Combine(@"..\..\..\Unit Tests\Script-Tests", @"harness\ed.js")));
            var includes = includeBuilder.ToString();

            var skippedTests = XDocument.Load(Path.Combine(@"..\..\..\Unit Tests\Script-Tests", @"config\excludelist.xml"))
                .Element("excludeList")
                .Elements("test")
                .Select(t => new { Name = t.Attribute("id").Value, Reason = t.Value }).ToList();

            Regex r = new Regex("^ \\* @([a-z]+)(.*?)$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            for (int i = 0; i < zipfile.Count; i++)
            {
                var zf = zipfile[i];
                if (zf.IsFile && zf.Name.EndsWith(".js"))
                {
                    string content;
                    using (var sr = new StreamReader(zipfile.GetInputStream(zf)))
                        content = sr.ReadToEnd();

                    var isNegative = content.Contains("@negative");
                    string negativeType = null;
                    if (isNegative)
                    {
                        var negativeStart = content.IndexOf("@negative ") + "@negative ".Length;
                        if (negativeStart != -1 + "@negative ".Length)
                        {
                            negativeType = content.Substring(negativeStart, content.IndexOfAny(new char[] { '\r', '\n' }, negativeStart) - negativeStart).Trim();
                            if (string.IsNullOrWhiteSpace(negativeType))
                                negativeType = null;
                        }
                    }
                    var forceStrictMode = content.Contains("@onlyStrict");

                    var fn = Path.GetFileName(zf.Name);
                    string name = fn.Substring(0, fn.Length - 3);
                    var tcd = new TestCaseData(includes, content, forceStrictMode, isNegative, negativeType).SetName(name);
                    if (skippedTests.Any(t => t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        tcd = tcd.Ignore(skippedTests.First(t => t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Reason);
                    }
                    else
                    {
                        tcd = tcd.SetProperty("_NOTIGNORED", "TRUE");
                    }

                    tcd = tcd.SetProperty("@_name", zf.Name.Substring(0, zf.Name.Length - 3)).SetProperty("@_negativeType", negativeType);

                    foreach (Match match in r.Matches(content))
                    {
                        if (match.Groups[2].Length > 0)
                            tcd = tcd.SetProperty("_" + match.Groups[1].Value.ToUpper(), match.Groups[2].Value.Trim());
                        else
                            tcd = tcd.SetProperty("_" + match.Groups[1].Value.ToUpper(), "TRUE");
                    }

                    yield return tcd;
                }
            }
        }

        public static IEnumerable<TestCaseData> GetTests()
        {
            return GenerateTests()/*.OrderBy(t => t.Properties["@_name"])*/;
        }
    }
}
