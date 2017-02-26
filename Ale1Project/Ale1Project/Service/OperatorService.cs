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
            if (Operators.Contains(val)) return true;
            return false;
        }
    }
}
