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
        List<string> RevertTruthTableSimplification(ExpressionModel DnfExpressionModel, string originalBinary);
        string GetDisjunctiveNormalForm(ExpressionModel expressionModel);
        string GetSimplifiedDisjunctiveNormalForm(ExpressionModel expressionModel);
    }
}
