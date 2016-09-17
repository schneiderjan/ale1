﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace AleWk1
{
    public static class Helper
    {
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
            string vars = "";
            foreach (var n in flatList)
            {
                if (!IsOperator(n.Value)) vars += n.Value;
            }
            return new string(vars.Distinct().ToArray());
        }

        internal static string GetInfixString(List<Node> flatList)
        {
            infix = "";
            flatList.Reverse();
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

                if (IsOperator(rootNode.Value)) infix += GetAsciiReprentation(rootNode.Value);
                else infix += rootNode.Value;

                InOrder(rootNode.RightChild, rootNode);

                infix = infix + " )";
            }
            else
            {
                InOrder(rootNode.LeftChild, rootNode);

                if (IsOperator(rootNode.Value)) infix += GetAsciiReprentation(rootNode.Value);
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
    }
}
