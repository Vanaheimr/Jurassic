using System;
using System.Text;
using Jurassic;

namespace TestAssembly
{
    class Program
    {
        static void Main(string[] args)
        {
            for (; ; )
            {
                ScriptEngine sr = new ScriptEngine();
                StringBuilder sb = new StringBuilder();
                for (; ; )
                {
                    string txt = Console.ReadLine();
                    if (txt == "::exit::")
                        return;
                    if (txt == "::end::")
                        break;
                    sb.AppendLine(txt);
                }
                try
                {
                    sr.Execute(sb.ToString());
                    Console.WriteLine("::pass::");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                Console.WriteLine("::fin::");
                Console.Out.Flush();
            }
        }
    }
}
