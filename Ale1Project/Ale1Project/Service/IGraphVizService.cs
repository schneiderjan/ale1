using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ale1Project.Model;

namespace Ale1Project.Service
{
   public interface IGraphVizService
   {
       void DisplayAutomaton();
       GraphVizFileModel ConvertExpressionModelToGraphVizFile(ExpressionModel expressionModel);
   }
}
