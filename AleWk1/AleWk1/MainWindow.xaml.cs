﻿using System;
using System.Collections.Generic;
using System.Windows;

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
            tbPrefix.Text = "&(A, ~(B))";
            //"=( >(A,B), |( ~(A) ,B) )
            //&(A, ~(B))
        }

        private void btnParse_Click(object sender, RoutedEventArgs e)
        {
            var input = tbPrefix.Text
                .Replace(",", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(@" ", "")
                .Trim();

            var prefixInput = ConvertStringToList(input);
            var reversedPrefixInput = ReverseList(prefixInput);
            ParseInfix(reversedPrefixInput);
        }

        private List<string> ReverseList(List<string> prefixInput)
        {
            prefixInput.Reverse();
            return prefixInput;
        }

        private List<string> ConvertStringToList(string input)
        {
            List<string> chars = new List<string>();
            foreach (var c in input)
            {
                chars.Add(c.ToString());
            }
            return chars;
        }

        private void ParseInfix(List<string> reversedPrefixInput)
        {
            var flatList = GetTree(reversedPrefixInput);
            List<Node> infixs = new List<Node>();
            var output = "";

            foreach (var c in flatList)
            {
                infixs.Add(c);
            }

            foreach (var i in infixs)
            {
                output += i.Value;
            }
            tbInfix.Text = output;
        }

        private List<Node> GetTree(List<string> reversedPrefixInput)
        {
            List<Node> flatList = new List<Node>();
            var stack = new Stack<Node>();
            foreach (var c in reversedPrefixInput)
            {
                if (IsOperator(c))
                {
                    Node rightOperand = stack.Pop();
                    Node leftOperand = stack.Pop();
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
