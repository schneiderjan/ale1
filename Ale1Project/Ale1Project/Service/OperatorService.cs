using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ale1Project.Service
{
    public class OperatorService : IOperatorService
    {
        public string Not => "~";

        public List<string> Operators => new List<string>()
        {
            "&",
            "|",
            ">",
            "=",
            "%",
        };

        public bool IsOperator(string val)
        {
            return Operators.Contains(val);
        }
        public string ConvertAsciiReprentation(string rootNodeValue)
        {
            var output = string.Empty;

            switch (rootNodeValue)
            {
                case "~":
                    output = "¬";
                    break;
                case "&":
                    output = "⋀";
                    break;
                case "|":
                    output = "⋁";
                    break;
                case ">":
                    output = "⇒";
                    break;
                case "=":
                    output = "⇔";
                    break;
                case "%":
                    output = "⊼";
                    break;
            }
            return output;
        }
    }
}
