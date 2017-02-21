using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ale1Project.Model;

namespace Ale1Project.Service
{
    public interface IFixConversionService
    {
        string ParsePrefix(ExpressionModel expressionModel);
        void GetDistinctVariables(ExpressionModel expressionModel);
    }
}
