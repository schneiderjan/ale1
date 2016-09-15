using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AleWk1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> operators;
        private const string not = "~";


        public MainWindow()
        {
            InitializeComponent();
            operators = new List<string>()
            {
                "&",
                "|",
                ">",
                "=",
            };
            tbPrefix.Text = "&(=(A,B),|(~C,D))";
            //"=( >(A,B), |( ~(A) ,B) )
            //&(A, ~(B))
            //&(=(A,B),|(C,D))
            btnParse.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        }

        private void btnParse_Click(object sender, RoutedEventArgs e)
        {
            var input = tbPrefix.Text
                .Replace(",", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(@" ", "")
                .Trim();

            var prefixInput = Helper.ConvertStringToList(input);
            var reversedPrefixInput = Helper.ReverseList(prefixInput);
            ParseInfix(reversedPrefixInput);
        }      

        private void ParseInfix(List<string> reversedPrefixInput)
        {
            var flatList = GetTree(reversedPrefixInput);
            List<Node> infixs = new List<Node>();
            var output = "";

            GenerateGraph(flatList);

            foreach (var i in infixs)
            {
                output += i.Value;
            }
            tbInfix.Text = output;
        }

        private void GenerateGraph(List<Node> flatList)
        {
            List<string> txtFileLines = new List<string>();

            txtFileLines.Add("graph logic {");
            txtFileLines.Add("node[fontname = \"Arial\"]");

            foreach (var node in flatList)
            {
                var label = string.Format("node{0} [ label = \" {1} \" ]", flatList.IndexOf(node) + 1, node.Value);
                txtFileLines.Add(label);
            }

            foreach (var node in flatList)
            {
                if (node.LeftChild != null)
                {
                    var nodeConnection = string.Format("node{0} -- node{1}",
                        flatList.IndexOf(node) + 1, flatList.IndexOf(node.LeftChild)+1);
                    txtFileLines.Add(nodeConnection);
                }
                if (node.RightChild != null)
                {
                    var nodeConnection = string.Format("node{0} -- node{1}",
                        flatList.IndexOf(node) + 1, flatList.IndexOf(node.RightChild)+1);
                    txtFileLines.Add(nodeConnection);
                }
            }

            txtFileLines.Add("}");
            Helper.WriteToFile(txtFileLines);
            Helper.DisplayGraph();

        }

        private List<Node> GetTree(List<string> reversedPrefixInput)
        {
            List<Node> flatList = new List<Node>();
            var stack = new Stack<Node>();
            foreach (var c in reversedPrefixInput)
            {
                if (IsOperator(c))
                {
                    Node leftOperand = stack.Pop();
                    Node rightOperand = stack.Pop();
                    var node = new Node(c, leftOperand, rightOperand);
                    flatList.Add(node);
                    stack.Push(node);
                }
                else if (c.Equals(not))
                {
                    Node rightOperand = stack.Pop();
                    var node = new Node(c, rightOperand);
                    flatList.Add(node);
                    stack.Push(node);
                }
                else
                {
                    var node = new Node(c);
                    flatList.Add(node);
                    stack.Push(node);
                }
            }
            return flatList;
        }

        private bool IsOperator(string c)
        {
            if (operators.Contains(c)) return true;
            return false;
        }
    }
}

////n is a child with right/left with a not as parent.
//if (n.LeftChild == null 
//    && n.RightChild == null
//    && flatList[flatList.IndexOf(n) +1].LeftChild == null
//    && flatList[flatList.IndexOf(n) + 1].RightChild == null)
//{
//    n.Parent = flatList[flatList.IndexOf(n) + 3];
//}
//else if (n.LeftChild == null
//    && n.RightChild == null
//    )
//{
//    n.Parent = flatList[flatList.IndexOf(n) + 1];
//}
////n must be a NOT operator if either one operand is null
////and first condition is already true.
//else if (n.LeftChild == null
//    || n.RightChild == null
//    && n.Value.Equals(not))
//{
//    n.Parent = flatList[flatList.IndexOf(n) + 1];
//}
//else if (operators.Contains(flatList[flatList.IndexOf(n) + 1].Value))
//{
//    n.Parent = flatList[flatList.IndexOf(n) + 1];
//}
//else if (operators.Contains(flatList[flatList.IndexOf(n)].Value))
//{
//    n.Parent = flatList[flatList.IndexOf(n) + 1];
//}