using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace AleWk1
{
    public class Helper
    {
        internal static bool[,] tableVals;
        internal static bool[] answer;
        internal static int variableCount;
        internal static List<Node> listWithAllTheNodes;
        internal static List<char> variables;
        internal static List<string> operators = new List<string>()
            {
                "&",
                "|",
                ">",
                "=",
            };

        internal const string not = "~";
        internal static string infix;


        internal static void WriteToFile(List<Node> flatList)
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
                        flatList.IndexOf(node) + 1, flatList.IndexOf(node.LeftChild) + 1);
                    txtFileLines.Add(nodeConnection);
                }
                if (node.RightChild != null)
                {
                    var nodeConnection = string.Format("node{0} -- node{1}",
                        flatList.IndexOf(node) + 1, flatList.IndexOf(node.RightChild) + 1);
                    txtFileLines.Add(nodeConnection);
                }

            }
            txtFileLines.Add("}");

            using (StreamWriter sw = new StreamWriter("dot.dot", false))
            {
                foreach (var line in txtFileLines)
                {
                    sw.WriteLineAsync(line);
                }
            }
        }

        internal static string GetDistinctVariables(List<Node> flatList)
        {
            variables = new List<char>();
            string vars = "";
            foreach (var n in flatList)
            {
                if (!IsOperator(n.Value) && !n.Value.Equals(not)) { vars += n.Value; }

            }
            var distinct = new string(vars.Distinct().ToArray());
            distinct = Alphabetize(distinct);

            for (int i = 0; i < distinct.Length; i++)
            {
                variables.Add(distinct[i]);
            }

            variableCount = distinct.Length;
            return distinct;
        }
        internal static string Alphabetize(string s)
        {
            char[] a = s.ToCharArray();
            Array.Sort(a);
            return new string(a);
        }

        internal static List<string> GenerateTableSimplified()
        {

            List<int> indeces = new List<int>();
            for (int j = 0; j < tableVals.GetLength(0); j++)
            {
                if (answer[j] == true)
                {
                    indeces.Add(j);
                }
            }

            int nextTrueItem = indeces.First();

            if (indeces.Count > 2)
            {


                for (int i = 0; i < variableCount; i++) //Columns i
                {
                    Debug.WriteLine("i: " + i);
                    for (int j = 0; j < tableVals.GetLength(0); j++) //Rows j
                    {
                        for (int k = 0; k < tableVals.GetLength(0); k++)
                        {

                            Debug.WriteLine("j: " + j);
                            if (indeces.Contains(i) && nextTrueItem == i)
                            {
                                if (indeces.Contains(i + 1))
                                {
                                    nextTrueItem = indeces[i + 1];

                                }
                                Debug.WriteLine("IT IS IN");
                            }
                        }
                    }
                }
            }
            return new List<string>();
        }

        internal static string GetInfixString(List<Node> flatList)
        {
            infix = "";
            flatList.Reverse();
            listWithAllTheNodes = flatList;
            InOrder(flatList[0], null);
            return infix;
        }
        private static void InOrder(Node rootNode, Node previousNode)
        {
            if (rootNode == null) return;

            if (previousNode != null && rootNode.RightChild != null)
            {
                infix = infix + "( ";

                InOrder(rootNode.LeftChild, rootNode);

                if (IsOperator(rootNode.Value) || rootNode.Value.Equals(not)) infix += GetAsciiReprentation(rootNode.Value);
                else infix += rootNode.Value;

                InOrder(rootNode.RightChild, rootNode);

                infix = infix + " )";
            }
            else
            {
                InOrder(rootNode.LeftChild, rootNode);

                if (IsOperator(rootNode.Value) || rootNode.Value.Equals(not)) infix += GetAsciiReprentation(rootNode.Value);
                else infix += rootNode.Value;

                InOrder(rootNode.RightChild, rootNode);
            }
        }

        private static string GetAsciiReprentation(string value)
        {
            var op = "";

            switch (value)
            {
                case not:
                    op = "¬";
                    break;
                case "&":
                    op = "⋀";
                    break;
                case "|":
                    op = "⋁";
                    break;
                case ">":
                    op = "⇒";
                    break;
                case "=":
                    op = "⇔";
                    break;
            }
            return op;
        }

        internal static void DisplayGraph()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var dotPath = path + "\\dot.dot";
            var imgPath = path + "\\dot.png";

            using (var p = new Process())
            {

                p.StartInfo.Verb = "runas";
                p.StartInfo.FileName = @"C:\Program Files (x86)\Graphviz2.38\bin\dot.exe";
                p.StartInfo.Arguments = " -Tpng -odot.png " + dotPath;
                p.Start();
                p.WaitForExit();
            }

            using (var p = new Process())
            {
                p.StartInfo.FileName = @imgPath;
                p.Start();
            }
        }

        internal static List<string> ReverseList(List<string> prefixInput)
        {
            prefixInput.Reverse();
            return prefixInput;
        }

        internal static List<string> ConvertStringToList(string input)
        {
            List<string> chars = new List<string>();
            foreach (var c in input)
            {
                chars.Add(c.ToString());
            }

            return chars;
        }

        internal static List<char> ConvertStringToCharList(string input)
        {
            List<char> chars = new List<char>();
            foreach (var c in input)
            {
                chars.Add(c);
            }
            return chars;
        }

        internal static List<Node> GetFlatList(List<string> reversedPrefixInput)
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

        internal static bool IsOperator(string c)
        {
            if (operators.Contains(c)) return true;
            return false;
        }


        internal static List<string> GenerateTable(string input)
        {
            var Variables = ConvertStringToCharList(input);

            var listString = ConvertStringToList(input);

            // var _nodes = listwithallthenodes;
            listWithAllTheNodes.Reverse();
            //var inputTable = GenerateTableInput(Varibles.Count);GetDistinctVariables
            var inputTable = GenerateTableInput(variableCount);

            bool[] answer1 = new bool[inputTable.GetLength(0)];
            for (int i = 0; i < inputTable.GetLength(0); i++)
            {
                for (int j = 0; j < inputTable.GetLength(1); j++)
                {
                    SetValue(variables[j], inputTable[i, j]);
                }
                answer1[i] = Solve();
            }
            answer = answer1;

            //if (!equal) TruthTableView.Columns.Add("#2");
            var Values = new List<string>();
            for (int i = 0; i < inputTable.GetLength(0); i++)
            {
                Values.Add(getBoolStr(inputTable[i, 0]));
                for (int j = 1; j < inputTable.GetLength(1); j++)
                {
                    Values.Add(getBoolStr(inputTable[i, j]));
                }
                Values.Add(getBoolStr(answer1[i]));
            }

            return Values;
        }

        // Get stirng from bool value
        internal static string getBoolStr(bool b)
        {
            return b ? "1" : "0";
        }

        internal static bool Solve()
        {
            Stack<bool> stack = new Stack<bool>();

            foreach (var t in listWithAllTheNodes)
            {
                if (!IsOperator(t.Value) && !t.Value.Equals(not))
                {
                    stack.Push(t.BoolValue);
                }
                else
                {
                    //if (t.ArgCount > stack.Count)
                    //{
                    //    throw new Exception("The user has not input sufficient values in the expression!");
                    //}

                    // evaluate the operator:
                    switch (t.Value)
                    {
                        case "|"://or
                            stack.Push(stack.Pop() | stack.Pop());
                            break;
                        case "="://xor
                            stack.Push(!(stack.Pop() ^ stack.Pop()));
                            break;
                        case ">"://implication
                            stack.Push(!stack.Pop() | stack.Pop());
                            break;
                        case "&"://and
                            stack.Push(stack.Pop() & stack.Pop());
                            break;
                        case "~"://not
                            stack.Push(!stack.Pop());
                            break;
                        default:
                            throw new Exception("Error: Invalid operation!!");
                    }
                }
            }

            // if (stack.Count > 1) throw new Exception("Error: The user input has too many values.");

            return stack.Pop();
        }

        // set variable value, returns true if successfully changed
        internal static bool SetValue(char c, bool val)
        {
            bool success = false;
            char ch = Char.ToUpper(c);
            foreach (var t in listWithAllTheNodes)
            {
                if (t.Value == ch.ToString())
                {
                    t.BoolValue = val;
                    success = true;
                }
            }

            return success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columns">The amount of varables indicate the amount of colomns</param>
        /// <returns></returns>
        private static bool[,] GenerateTableInput(int columns)
        {
            var rows = (int)Math.Pow(2, columns);

            var table = new bool[rows, columns];

            var divider = rows;

            // Iterate by column
            for (var c = 0; c < columns; c++)
            {
                divider /= 2;
                var cell = false;
                // Iterate every row by this column's index:
                for (var r = 0; r < rows; r++)
                {
                    table[r, c] = cell;
                    if ((divider == 1) || ((r + 1) % divider == 0))
                    {
                        cell = !cell;
                    }
                }
            }
            tableVals = table;
            return table;
        }


        internal static string HexaDecimal(string bin)
        {
            // //string bin = "10100010";
            //bin.Reverse();
            char[] charArray = bin.ToCharArray();
            Array.Reverse(charArray);
            bin = new string(charArray);



            int rest = bin.Length % 4;
            if (rest != 0)
                bin = new string('0', 4 - rest) + bin; //pad the length out to by divideable by 4

            string output = "";

            for (int i = 0; i <= bin.Length - 4; i += 4)
            {
                output += string.Format("{0:X}", Convert.ToByte(bin.Substring(i, 4), 2));
            }

            return output;
        }


    }
}

