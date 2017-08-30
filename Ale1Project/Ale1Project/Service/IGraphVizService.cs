using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ale1Project.Model;
using Ale2Project.Model;

namespace Ale2Project.Service
{
   public interface IGraphVizService
   {
       void DisplayAutomaton();
       GraphVizFileModel ConvertExpressionModelToGraphVizFile(ExpressionModel expressionModel);
   }
}
