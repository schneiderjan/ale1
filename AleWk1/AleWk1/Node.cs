using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AleWk1
{
    public class Node
    {
        public Node Parent { get; set; }
        public Node LeftChild { get; set; }
        public Node RightChild { get; set; }
        public string Value { get; set; }
    }
}
