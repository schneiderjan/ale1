using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ale1Project.Model
{
    public class NodeModel
    {
        public NodeModel LeftChild { get; set; }
        public NodeModel RightChild { get; set; }
        public string Value { get; set; }
        public bool BoolValue { get; set; }

        public NodeModel(string val, NodeModel leftChild, NodeModel rightChild)
        {
            Value = val;
            LeftChild = leftChild;
            RightChild = rightChild;
        }

        public NodeModel(string val)
        {
            Value = val;
        }

        public NodeModel(string val, NodeModel rightChild)
        {
            Value = val;
            RightChild = rightChild;
        }
    }
}
