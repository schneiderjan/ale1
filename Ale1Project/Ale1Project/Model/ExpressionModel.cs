using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ale1Project.Model
{
    public class ExpressionModel
    {
        public ExpressionModel()
        {
            DistinctVariables = new List<char>();
            TreeNodes = new List<NodeModel>();
        }

        public string Prefix { get; set; }
        public string Infix { get; set; }
        public List<char> DistinctVariables { get; set; }
        public List<NodeModel> TreeNodes { get; set; }


    }
}
