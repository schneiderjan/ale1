using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AleWk1
{
    public class Node
    {
        public Node LeftChild { get; set; }
        public Node RightChild { get; set; }
        public string Value { get; set; }

        public Node(string _val, Node _leftChild, Node _rightChild)
        {
            Value = _val;
            LeftChild = _leftChild;
            RightChild = _rightChild;
        }

        public Node(string _val)
        {
            Value = _val;
        }

        public Node(string _val, Node _rightChild)
        {
            Value = _val;
            RightChild = _rightChild;
        }
    }
}
