using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ale1Project.Model
{
    public class NodeModel
    {
        public int Id { get; set; }
        public NodeModel LeftChild { get; set; }
        public NodeModel RightChild { get; set; }
        public string Value { get; set; }
        public bool BoolValue { get; set; }

        public NodeModel(int id, string val, NodeModel leftChild, NodeModel rightChild)
        {
            Id = id;
            Value = val;
            LeftChild = leftChild;
            RightChild = rightChild;
        }

        public NodeModel(int id, string val)
        {
            Id = id;
            Value = val;
        }

        public NodeModel(int id, string val, NodeModel rightChild)
        {
            Id = id;
            Value = val;
            RightChild = rightChild;
        }
    }
}
