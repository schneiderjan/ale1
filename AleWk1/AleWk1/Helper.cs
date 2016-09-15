using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AleWk1
{
    public static class Helper
    {
        internal static void WriteToFile(List<string> txtFileLines)
        {
            using (StreamWriter sw = new StreamWriter(@"C:\Program Files (x86)\Graphviz2.38\bin\dot.dot", false))
            {
                foreach (var line in txtFileLines)
                {
                    sw.WriteLineAsync(line);
                }
            }
        }

        internal static void DisplayGraph()
        {
            using (var p = new Process())
            {
                p.StartInfo.Verb = "runas";
                p.StartInfo.FileName = @"C:\Program Files (x86)\Graphviz2.38\bin\dot.exe";
                p.StartInfo.Arguments = " -Tpng -odot.png dot.dot";
                p.Start();
                p.WaitForExit();
            }
        }

        internal static List<string> ReverseList(List<string> prefixInput)
        {
            prefixInput.Reverse();
            return prefixInput;
        }

        internal static List<string> ConvertStringToList(string input)
        {
            List<string> chars = new List<string>();
            foreach (var c in input)
            {
                chars.Add(c.ToString());
            }
            return chars;
        }
    }
}
