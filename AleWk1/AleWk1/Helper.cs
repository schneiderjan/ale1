using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;

namespace AleWk1
{
    public class Helper
    {


        internal static bool[,] tableVals;
        internal static bool[] answer;
        internal static List<string> simplifiedTable;
        internal static int variableCount;
        internal static List<Node> listWithAllTheNodes;
        internal static List<char> variables = new List<char>();
        internal static List<string> operators = new List<string>()
            {
                "&",
                "|",
                ">",
                "=",
                "%",
            };

        internal const string not = "~";
        internal static string infix;
        internal static List<string> truthTable;
        private static string dj;
        private static string djSimplified;

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
                    sw.WriteLine(line);
                }
                sw.Dispose();
            }
        }

        internal static string GetDistinctVariables(List<Node> flatList)
        {
            string vars = "";
            foreach (var n in flatList)
            {
                if (!IsOperator(n.Value) && !n.Value.Equals(not)) { vars += n.Value; }

            }
            var distinct = new string(vars.Distinct().ToArray());
            distinct = Alphabetize(distinct);

            variables.Clear();
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

        internal static List<string> GenerateTableSimplified(ListView lvTruthTable)
        {
            List<string> rows = new List<string>();
            List<string> truthRows = new List<string>();
            List<string> result = new List<string>();

            foreach (var str in lvTruthTable.Items)
            {
                var x = Regex.Replace(str.ToString(), @"\s+", "");
                rows.Add(x);
            }
            truthTable = rows;

            for (int i = 1; i < rows.Count; i++)
            {
                if (rows[i][variableCount] == '1') truthRows.Add(rows[i]);
            }

            if (truthRows.Count > 2)
            {
                for (int i = 0; i < variableCount; i++) //Columns i
                {
                    for (int j = 1; j < rows.Count; j++) //Rows j
                    {
                        int simplifiable = 0;

                        for (int k = 1; k < rows.Count; k++)
                        {
                            if (rows[j][variableCount] == rows[k][variableCount]
                                && rows[j][variableCount] == '1'
                                && rows[j][i] == rows[k][i]) simplifiable++;
                        }

                        if (simplifiable > 1)
                        {
                            string leftside = "", rightside = "";

                            for (int t = 0; t < i; t++) leftside += "*\t";
                            for (int t = i + 1; t < variableCount; t++) rightside += "*\t";

                            string tautology = "";
                            if (rows[j][i] == '1')
                            {
                                tautology = leftside + "0\t" + rightside + "1";
                                if (result.Contains(tautology)) result.Remove(tautology);
                            }
                            else tautology = leftside + "1\t" + rightside + "1";

                            result.Add(leftside + rows[j][i] + "\t" + rightside + "1");
                        }
                        else
                        {
                            if (rows[j][variableCount] != '1')
                            {
                                string simplified = "";
                                for (int t = 0; t < rows[j].Length; t++) simplified += rows[j][t] + "\t";
                                result.Add(simplified);
                            }
                        }
                    }
                }

            }
            else
            {
                for (int i = 1; i < rows.Count; i++)
                {
                    var simplifedrow = "";
                    for (int j = 0; j < rows[i].Length; j++)
                    {
                        simplifedrow = simplifedrow + rows[i][j] + "\t";
                    }
                    result.Add(simplifedrow);
                }
            }
            result = result.Distinct().ToList();
            result.Insert(0, lvTruthTable.Items[0].ToString());
            simplifiedTable = result;
            return result;
        }

        internal static string GetNand(string input)
        {
            string nand = "",
                leftSide = "",
                rightSide = "",
                leftNand = "",
                rightNand = "",
                expression = "",
                fullExpression = "";
            Stack<Node> stack = new Stack<Node>();

            if (input.Contains("%")) return input;

            foreach (var node in listWithAllTheNodes)
            {
                if (!IsOperator(node.Value) && node.Value != not)
                {
                    stack.Push(node);
                }
                else
                {
                    switch (node.Value)
                    {
                        case "|":
                            if (leftSide != "" && rightSide != "")
                            {
                                if (stack.Count != 1)
                                {
                                    expression = "%(%(" + leftSide + "," + leftSide + "),%(" + rightSide + "," + rightSide + "))";
                                    fullExpression = expression;
                                    rightSide = expression;
                                    leftSide = "";
                                }
                                else
                                {
                                    leftNand = stack.Pop().Value;
                                    leftSide = "%(%(" + leftNand + "," + leftNand + "),%(" + leftSide + "," + leftSide + "))";
                                }
                            }
                            else if (stack.Count == 1)
                            {
                                leftNand = stack.Pop().Value;
                                leftSide = leftNand;
                                expression = "%(%(" + leftSide + "," + leftSide + "),%(" + rightSide + "," + rightSide + "))";
                                rightSide = expression;
                                leftSide = "";
                            }
                            else if (rightSide == "")
                            {
                                if (stack.Count != 0) rightNand = stack.Pop().Value;
                                leftNand = stack.Count > 0 ? stack.Pop().Value : rightSide;
                                rightSide = "%(%(" + leftNand + "," + leftNand + "),%(" + rightNand + "," + rightNand + "))";
                                fullExpression = leftSide;
                            }
                            else if (leftSide == "")
                            {
                                if (stack.Count != 0) leftNand = stack.Pop().Value;
                                rightNand = stack.Count > 0 ? stack.Pop().Value : rightSide;
                                leftSide = "%(%(" + leftNand + "," + leftNand + "),%(" + rightNand + "," + rightNand + "))";
                                fullExpression = leftSide;
                            }
                            break;
                        case "=":
                            if (leftSide != "" && rightSide != "")
                            {
                                if (stack.Count != 1)
                                {
                                    expression = "%(%(%(" + leftSide + "," + leftSide + "),%(" + rightSide + "," + rightSide + ")),%(" + leftSide + "," + rightSide + "))";
                                    fullExpression = expression;
                                    rightSide = expression;
                                    leftSide = "";
                                }
                                else
                                {
                                    leftNand = stack.Pop().Value;
                                    leftSide = "%(%(%(" + leftNand + "," + leftNand + "),%(" + leftSide + "," + leftSide + ")),%(" + leftNand + "," + leftSide + "))";
                                }
                            }

                            else if (stack.Count == 1)
                            {
                                leftNand = stack.Pop().Value;
                                leftSide = leftNand;
                                expression = "%(%(%(" + leftSide + "," + leftSide + "),%(" + rightSide + "," + rightSide + ")),%(" + leftSide + "," + rightSide + "))";
                                rightSide = expression;
                                leftSide = "";
                            }
                            else if (rightSide == "")
                            {
                                if (stack.Count != 0) rightNand = stack.Pop().Value;
                                leftNand = stack.Count > 0 ? stack.Pop().Value : rightSide;
                                rightSide = "%(%(%(" + leftNand + "," + leftNand + "),%(" + rightNand + "," + rightNand + ")),%(" + leftNand + "," + rightNand + "))";
                                fullExpression = leftSide;
                            }
                            else if (leftSide == "")
                            {
                                if (stack.Count != 0) leftNand = stack.Pop().Value;
                                rightNand = stack.Count > 0 ? stack.Pop().Value : rightSide;
                                leftSide = "%(%(%(" + leftNand + "," + leftNand + "),%(" + rightNand + "," + rightNand + ")),%(" + leftNand + "," + rightNand + "))";
                                fullExpression = leftSide;
                            }
                            break;
                        case ">":
                            if (leftSide != "" && rightSide != "")
                            {
                                if (stack.Count != 1)
                                {
                                    expression = "%(" + leftSide + ",%(" + rightSide + "," + rightSide + "))";
                                    fullExpression = expression;
                                    rightSide = expression;
                                    leftSide = "";
                                }
                                else
                                {
                                    leftNand = stack.Pop().Value;
                                    leftSide = "%(" + leftNand + ",%(" + leftSide + "," + leftSide + "))";
                                }
                            }
                            else if (stack.Count == 1)
                            {
                                leftNand = stack.Pop().Value;
                                leftSide = leftNand;
                                expression = "%(" + leftSide + ",%(" + rightSide + "," + rightSide + "))";
                                rightSide = expression;
                                leftSide = "";
                            }
                            else if (rightSide == "")
                            {
                                if (stack.Count != 0) leftNand = stack.Pop().Value;
                                rightNand = stack.Count > 0 ? stack.Pop().Value : rightSide;
                                rightSide = "%(" + leftNand + ",%(" + rightNand + "," + rightNand + "))";
                                fullExpression = leftSide;
                            }
                            else if (leftSide == "")
                            {
                                if (stack.Count != 0) leftNand = stack.Pop().Value;
                                rightNand = stack.Count > 0 ? stack.Pop().Value : rightSide;
                                leftSide = "%(" + leftNand + ",%(" + rightNand + "," + rightNand + "))";
                                fullExpression = leftSide;
                            }
                            break;
                        case "&":
                            if (leftSide != "" && rightSide != "")
                            {
                                if (stack.Count != 1)
                                {
                                    expression = "%(%(" + leftSide + "," + rightSide + "),%(" + leftSide + "," + rightSide + "))";
                                    fullExpression = expression;
                                    rightSide = expression;
                                    leftSide = "";
                                }
                                else
                                {
                                    leftNand = stack.Pop().Value;
                                    leftSide = "%(%(" + leftNand + "," + leftSide + "),%(" + leftNand + "," + leftSide + "))";
                                }
                            }
                            else if (stack.Count == 1)
                            {
                                leftNand = stack.Pop().Value;
                                leftSide = leftNand;
                                expression = "%(%(" + leftSide + "," + rightSide + "),%(" + leftSide + "," + rightSide + "))";
                                rightSide = expression;
                                leftSide = "";
                            }
                            else if (rightSide == "")
                            {
                                if (stack.Count != 0) rightNand = stack.Pop().Value;
                                leftNand = stack.Count > 0 ? stack.Pop().Value : rightSide;
                                rightSide = "%(%(" + leftNand + "," + rightNand + "),%(" + leftNand + "," + rightNand + "))";
                                fullExpression = leftSide;
                            }
                            else if (leftSide == "")
                            {
                                if (stack.Count != 0) leftNand = stack.Pop().Value;
                                rightNand = stack.Count > 0 ? stack.Pop().Value : rightSide;
                                leftSide = "%(%(" + leftNand + "," + rightNand + "),%(" + leftNand + "," + rightNand + "))";
                                fullExpression = leftSide;
                            }
                            break;
                        case "~":
                            if (rightSide == "")
                            {
                                if (stack.Count == 2)
                                {
                                    leftNand = stack.Pop().Value;
                                    rightSide = stack.Pop().Value;
                                    fullExpression = "%(" + leftNand + "," + leftNand + ")";
                                    leftSide = fullExpression;
                                }
                                else if (stack.Count == 1)
                                {
                                    leftNand = stack.Pop().Value;
                                    fullExpression = "%(" + leftNand + "," + leftNand + ")";
                                    rightSide = fullExpression;
                                }
                                else if (stack.Count == 0)
                                {
                                    fullExpression = "%(" + rightSide + "," + rightSide + ")";
                                    rightSide = fullExpression;
                                }
                            }
                            else if (leftSide == "")
                            {
                                if (stack.Count == 2)
                                {
                                    leftNand = stack.Pop().Value;
                                    rightSide = stack.Pop().Value;
                                    fullExpression = "%(" + leftNand + "," + leftNand + ")";
                                    leftSide = fullExpression;
                                }
                                else if (stack.Count == 1)
                                {
                                    leftNand = stack.Pop().Value;
                                    fullExpression = "%(" + leftNand + "," + leftNand + ")";
                                    leftSide = fullExpression;
                                }
                                else if (stack.Count == 0)
                                {
                                    fullExpression = "%(" + rightSide + "," + rightSide + ")";
                                    rightSide = fullExpression;
                                }
                            }
                            else
                            {
                                try
                                {
                                    if (leftSide != "") leftSide = "%(" + leftSide + "," + leftSide + ")";
                                    else rightSide = "%(" + rightSide + "," + rightSide + ")";
                                }
                                catch (OutOfMemoryException)
                                {
                                    Debug.WriteLine("Expression too long");
                                }
                                finally
                                {
                                    nand = input;
                                }
                            }
                            break;
                        default: throw new Exception("Something went wrong");
                    }
                }
            }
            nand = fullExpression;
            return nand;
        }

        internal static string GetDisjunctiveNormalForm(ListView lvTruthTable)
        {
            List<string> truthRows = new List<string>();
            List<string> rows = new List<string>();
            List<string> result = new List<string>();
            string disjunctiveNormalForm = "";
            string leftAndOp = "";
            string rightAndOp = "";
            int index = 0;

            foreach (var str in lvTruthTable.Items)
            {
                var x = Regex.Replace(str.ToString(), @"\s+", "");
                rows.Add(x);
            }
            for (int i = 1; i < rows.Count; i++)
            {
                if (rows[i][variableCount] == '1') truthRows.Add(rows[i]);
            }

            string leftSide = "", rightSide = "";
            for (int i = 1; i < rows.Count; i++) //Columns i
            {
                if (rows[i][rows[i].Length - 1] == '1')
                {
                    index = i;
                    if (rows[i] != null && rows[i][0] == '0') leftSide = "~(" + variables[0] + ")";
                    else leftSide = variables[0].ToString();

                    for (int j = 1; j < variableCount; j++) //Rows j
                    {
                        if (rows[i][j] == '0') rightSide = "~(" + variables[j] + ")";
                        else rightSide = variables[j].ToString();

                        leftAndOp = "&(" + leftSide + "," + rightSide + ")";

                        if (variableCount > 2) leftSide = leftAndOp;
                    }
                    break;
                }
            }

            if (truthRows.Count > 2)
            {
                for (int i = index + 1; i < rows.Count; i++)
                {
                    if (rows[i][rows[i].Length - 1] == '1')
                    {
                        if (rows[i][0] == '0') leftSide = "~(" + variables[0] + ")";
                        else leftSide = variables[0].ToString();

                        for (int j = 1; j < variableCount; j++)
                        {
                            if (rows[i][j] == '0') rightSide = "~(" + variables[j] + ")";
                            else rightSide = variables[j].ToString();

                            rightAndOp = "&(" + leftSide + "," + rightSide + ")";

                            if (variables.Count > 0) leftSide = rightAndOp;
                        }
                        disjunctiveNormalForm = "|(" + leftAndOp + "," + rightAndOp + ")";
                        leftAndOp = disjunctiveNormalForm;
                    }
                }
            }
            else
            {
                disjunctiveNormalForm = leftAndOp;
            }

            dj = disjunctiveNormalForm;
            return disjunctiveNormalForm;
        }
        internal static string GetDisjunctiveNormalFormSimplified(ListView lvSimplifiedTruthTable)
        {
            List<string> truthRows = new List<string>();
            List<string> rows = new List<string>();
            List<string> result = new List<string>();
            string disjunctiveNormalFormSimplified = "";
            string leftAndOp = "";
            string rightAndOp = "";
            int index = 0;

            foreach (var str in lvSimplifiedTruthTable.Items)
            {
                var x = Regex.Replace(str.ToString(), @"\s+", "");
                rows.Add(x);
            }
            for (int i = 1; i < rows.Count; i++)
            {
                if (rows[i][variableCount] == '1') truthRows.Add(rows[i]);
            }

            string leftSide = "", rightSide = "";
            for (int i = 1; i < rows.Count; i++) //Columns i
            {
                if (rows[i][rows[i].Length - 1] == '1')
                {
                    index = i;
                    if (rows[i][0] != '*')
                    {
                        if (rows[i] != null && rows[i][0] == '0') leftSide = "~(" + variables[0] + ")";
                        else leftSide = variables[0].ToString();
                    }

                    for (int j = 1; j < variableCount; j++) //Rows j
                    {
                        if (rows[i][j] != '*')
                        {
                            if (rows[i][j] == '0') rightSide = "~(" + variables[j] + ")";
                            else rightSide = variables[j].ToString();

                            leftAndOp = "&(" + leftSide + "," + rightSide + ")";

                            if (variableCount > 0) leftSide = leftAndOp;
                        }
                        else leftAndOp = leftSide;
                    }
                    break;
                }
            }

            if (truthRows.Count > 0)
            {
                for (int i = index + 1; i < rows.Count; i++)
                {
                    if (rows[i][rows[i].Length - 1] == '1')
                    {
                        if (rows[i][0] != '*')
                        {
                            if (rows[i][0] == '0') leftSide = "~(" + variables[0] + ")";
                            else leftSide = variables[0].ToString();
                        }
                        else leftSide = "";

                        for (int j = 1; j < variableCount; j++)
                        {
                            if (rows[i][j] != '*')
                            {
                                if (rows[i][j] == '0') rightSide = "~(" + variables[j] + ")";
                                else rightSide = variables[j].ToString();

                                if (leftSide == "")
                                {
                                    leftSide = rightSide;
                                    rightAndOp = rightSide;
                                }
                                else rightAndOp = "&(" + leftSide + "," + rightSide + ")";

                                if (variables.Count > 0) leftSide = rightAndOp;
                            }
                            else rightAndOp = leftSide;
                        }
                        disjunctiveNormalFormSimplified = "|(" + leftAndOp + "," + rightAndOp + ")";
                        leftAndOp = disjunctiveNormalFormSimplified;
                    }
                }
            }
            else
            {
                disjunctiveNormalFormSimplified = leftAndOp;
            }

            djSimplified = disjunctiveNormalFormSimplified;
            return disjunctiveNormalFormSimplified;
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
                case "%":
                    op = "%";
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
            //var inputTable = GenerateTableInput(Count);GetDistinctVariables
            var inputTable = GenerateTableInput(variableCount);

            bool[] answer1 = new bool[inputTable.GetLength(0)];
            for (int i = 0; i < inputTable.GetLength(0); i++)
            {
                for (int j = 0; j < inputTable.GetLength(1); j++) SetValue(variables[j], inputTable[i, j]);
                answer1[i] = Solve();
            }
            answer = answer1;

            //if (!equal) TruthTableView.Columns.Add("#2");
            var Values = new List<string>();
            for (int i = 0; i < inputTable.GetLength(0); i++)
            {
                Values.Add(GetBoolRepresentation(inputTable[i, 0]));
                for (int j = 1; j < inputTable.GetLength(1); j++) Values.Add(GetBoolRepresentation(inputTable[i, j]));
                Values.Add(GetBoolRepresentation(answer1[i]));
            }
            return Values;
        }

        internal static string GetBoolRepresentation(bool b)
        {
            return b ? "1" : "0";
        }

        internal static bool Solve()
        {
            Stack<bool> stack = new Stack<bool>();

            foreach (var t in listWithAllTheNodes)
            {
                if (!IsOperator(t.Value) && !t.Value.Equals(not)) stack.Push(t.BoolValue);
                else
                {
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
                        case "%"://nand
                            stack.Push(!(stack.Pop() & stack.Pop()));
                            break;
                        default:
                            throw new Exception("NOT FOUND OPERATOR!");
                    }
                }
            }

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

        private static bool[,] GenerateTableInput(int columns)
        {
            var rows = (int)Math.Pow(2, columns);
            var table = new bool[rows, columns];
            var divider = rows;

            // columns
            for (var c = 0; c < columns; c++)
            {
                divider /= 2;
                var cell = false;
                // rows
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
            char[] charArray = bin.ToCharArray();
            Array.Reverse(charArray);
            bin = new string(charArray);

            int rest = bin.Length % 4;
            if (rest != 0) bin = new string('0', 4 - rest) + bin;

            string output = "";
            for (int i = 0; i <= bin.Length - 4; i += 4) output += string.Format("{0:X}", Convert.ToByte(bin.Substring(i, 4), 2));
            return output;
        }
    }
}

