using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ale1Project.Service
{
    public interface IOperatorService
    {
        string Not { get; }
        List<string> Operators { get; }
        bool IsOperator(string val);
        string ConvertAsciiReprentation(string rootNodeValue);


    }
}
