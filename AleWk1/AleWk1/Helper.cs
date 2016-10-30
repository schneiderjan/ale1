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
                "%"
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

            string nandexpression = "";
            var left = "";
            var leftnandexpression = "";
            var rightnandexpression = "";
            var exp = "";
            var leftexp = "";

            Stack<Node> stack = new Stack<Node>();

            if (input.Contains("%")) return input;

            if (input.Length < 100)

            {
                foreach (var t in listWithAllTheNodes)
                {
                    if (IsOperator(t.Value) == false)
                    {
                        stack.Push(t);
                    }
                    else
                    {
                        switch (t.Value)
                        {
                            case "|":
                                if (stack.Count != 0)
                                    leftnandexpression = stack.Pop().Value;

                                if (left == "")
                                {
                                    if (stack.Count > 0) rightnandexpression = stack.Pop().Value;
                                    nandexpression = nandexpression + "%(%(" + leftnandexpression + "," +
                                                     leftnandexpression + "),%(" + rightnandexpression + "," +
                                                     rightnandexpression + "))";
                                    left = nandexpression;
                                    leftexp = "%(%(" + leftnandexpression + "," + leftnandexpression + "),%(" +
                                              rightnandexpression + "," + rightnandexpression + "))";
                                }
                                else
                                {
                                    exp = "%(%(" + leftexp + "," + leftexp + "),%(" + leftnandexpression + "," +
                                          leftnandexpression + "))";
                                    nandexpression = exp;
                                    leftexp = nandexpression;
                                }
                                break;
                            case "=": //bu implication
                                if (stack.Count != 0)
                                    leftnandexpression = stack.Pop().Value;

                                if (left == "" && leftexp == "")
                                {
                                    if (stack.Count > 0)
                                        rightnandexpression = stack.Pop().Value;
                                    nandexpression = nandexpression + "%(%(%(" + leftnandexpression + "," +
                                                     leftnandexpression + "),%(" + rightnandexpression + "," +
                                                     rightnandexpression + ")),%(" + leftnandexpression + "," +
                                                     rightnandexpression + "))";
                                    left = nandexpression;
                                    leftexp = "%(%(%(" + leftnandexpression + "," + leftnandexpression + "),%(" +
                                              rightnandexpression + "," + rightnandexpression + ")),%(" +
                                              leftnandexpression + "," + rightnandexpression + "))";
                                }
                                else
                                {
                                    exp = "%(%(%(" + leftexp + "," + leftexp + "),%(" + leftnandexpression + "," +
                                          leftnandexpression + ")),%(" + leftexp + "," + leftnandexpression + "))";
                                    nandexpression = exp;
                                    leftexp = nandexpression;
                                }
                                break;
                            case ">": //implication
                                if (stack.Count != 0)
                                    leftnandexpression = stack.Pop().Value;

                                if (left == "" && leftexp == "")
                                {
                                    if (stack.Count > 0) rightnandexpression = stack.Pop().Value;
                                    nandexpression = nandexpression + "%(" + leftnandexpression + ",%(" +
                                                     rightnandexpression + "," + rightnandexpression + "))";
                                    left = nandexpression;
                                    leftexp = "%(" + leftnandexpression + ",%(" + rightnandexpression + "," +
                                              rightnandexpression + "))";
                                }
                                else
                                {
                                    exp = "%(" + leftexp + ",%(" + leftnandexpression + "," + leftnandexpression + "))";
                                    nandexpression = exp;
                                    leftexp = nandexpression;
                                }
                                break;
                            case "&": //and
                                if (stack.Count != 0)
                                    leftnandexpression = stack.Pop().Value;

                                if (left == "" && leftexp == "")
                                {
                                    if (stack.Count > 0) rightnandexpression = stack.Pop().Value;
                                    nandexpression = nandexpression + "%(%(" + leftnandexpression + "," +
                                                     rightnandexpression + "),%(" + leftnandexpression + "," +
                                                     rightnandexpression + "))";
                                    left = nandexpression;
                                    leftexp = "%(%(" + leftnandexpression + "," + rightnandexpression + "),%(" +
                                              leftnandexpression + "," + rightnandexpression + "))";
                                }
                                else
                                {
                                    try
                                    {
                                        exp = "%(%(" + leftexp + "," + leftnandexpression + "),%(" + leftexp + "," +
                                              leftnandexpression + "))";
                                        nandexpression = exp;
                                        leftexp = nandexpression;
                                    }
                                    catch (OutOfMemoryException)
                                    {
                                        Debug.WriteLine("Too loooooooooong NAND");
                                    }
                                }
                                break;
                            case "~": //not
                                if (stack.Count != 0)
                                    leftnandexpression = stack.Pop().Value;

                                if (left == "" && leftexp == "")
                                {
                                    nandexpression = nandexpression + "%(" + leftnandexpression + "," +
                                                     leftnandexpression + ")";
                                    left = nandexpression;
                                    leftexp = "%(" + leftnandexpression + "," + leftnandexpression + ")";
                                }
                                else
                                {
                                    try
                                    {
                                        exp = "%(" + leftexp + "," + leftexp + ")";
                                    }
                                    catch (OutOfMemoryException)
                                    {
                                        exp = "";
                                        Debug.WriteLine("Too looooooooong NAND");
                                    }
                                    finally
                                    {
                                        nandexpression = exp;
                                        leftexp = nandexpression;
                                    }
                                }


                                break;
                            case "%":
                                break;

                            default:
                                throw new Exception("Error: Invalid operation!!");
                        }
                    }
                }
            }

            var result = nandexpression == "" ? input : nandexpression;

            return result;
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
                            // per variable

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
                            // per variable
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
                        case "%"://nand
                            stack.Push(!(stack.Pop() & stack.Pop()));
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

