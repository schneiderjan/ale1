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
            TreeNodesReversed = new List<NodeModel>();
            TruthTable = new TruthTableModel();
        }

        public string Prefix { get; set; }
        public string Infix { get; set; }
        public string DisjunctiveNormalForm { get; set; }
        public List<char> DistinctVariables { get; set; }
        public List<NodeModel> TreeNodes { get; set; }
        public List<NodeModel> TreeNodesReversed { get; set; }
        public TruthTableModel TruthTable { get; set; }
        }
}
