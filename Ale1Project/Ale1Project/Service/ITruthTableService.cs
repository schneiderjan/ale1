using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ale1Project.Model;

namespace Ale1Project.Service
{
    public interface ITruthTableService
    {
        List<string> GetTruthTable(ExpressionModel expressionModel);
        string CalculateHash(ExpressionModel expressionModel);
        List<string> SimplifyTruthTable(ExpressionModel expressionModel);
        string GetDisjunctiveNormalForm(ExpressionModel expressionModel);
        string GetDisjunctiveNormalFormSimplified(ExpressionModel expressionModel);
    }
}
