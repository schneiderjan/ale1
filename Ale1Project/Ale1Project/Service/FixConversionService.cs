﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ale1Project.Model;
using Ale1Project.ViewModel;

namespace Ale1Project.Service
{
    public class FixConversionService : IFixConversionService
    {
        private string infix;
        private IOperatorService _operatorService;

        public FixConversionService(IOperatorService operatorService)
        {
            _operatorService = operatorService;
        }

        public string ParsePrefix(ExpressionModel expressionModel)
        {
            expressionModel.Prefix = expressionModel.Prefix
                .Replace(",", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(@" ", "")
                .Trim().ToUpper();

            var prefixInput = ConvertStringToList(expressionModel.Prefix);
            prefixInput.Reverse();

            var treeNodes = GetTree(prefixInput);
            expressionModel.TreeNodes = treeNodes;
            foreach (var nodeModel in treeNodes)
            {
                expressionModel.TreeNodesReversed.Add(nodeModel);
            }

            expressionModel.Infix = ConvertNodesToInfix(expressionModel.TreeNodes);

            return expressionModel.Infix;
        }

        private string ConvertNodesToInfix(List<NodeModel> flatNodeTree)
        {
            infix = "";
            flatNodeTree.Reverse();
            
            InOrder(flatNodeTree[0], null);
            return infix;
        }

        private void InOrder(NodeModel rootNode, NodeModel previousNode)
        {
            if (rootNode == null) return;

            if (previousNode != null && rootNode.RightChild != null)
            {
                infix = infix + "( ";

                InOrder(rootNode.LeftChild, rootNode);

                if (_operatorService.IsOperator(rootNode.Value) || rootNode.Value.Equals(_operatorService.Not)) infix += ConvertAsciiReprentation(rootNode.Value);
                else infix += rootNode.Value;

                InOrder(rootNode.RightChild, rootNode);

                infix = infix + " )";
            }
            else
            {
                InOrder(rootNode.LeftChild, rootNode);

                if (_operatorService.IsOperator(rootNode.Value) || rootNode.Value.Equals(_operatorService.Not)) infix += ConvertAsciiReprentation(rootNode.Value);
                else infix += rootNode.Value;

                InOrder(rootNode.RightChild, rootNode);
            }
        }

        private string ConvertAsciiReprentation(string rootNodeValue)
        {
            var op = "";

            switch (rootNodeValue)
            {
                case "~":
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

        private List<NodeModel> GetTree(List<string> prefixInput)
        {
            var flatList = new List<NodeModel>();
            var stack = new Stack<NodeModel>();

            foreach (var c in prefixInput)
            {
                if (_operatorService.IsOperator(c))
                {
                    NodeModel leftOperand = stack.Pop();
                    NodeModel rightOperand = stack.Pop();
                    var node = new NodeModel(c, leftOperand, rightOperand);
                    flatList.Add(node);
                    stack.Push(node);
                }
                else if (c.Equals(_operatorService.Not))
                {
                    NodeModel rightOperand = stack.Pop();
                    var node = new NodeModel(c, rightOperand);
                    flatList.Add(node);
                    stack.Push(node);
                }
                else
                {
                    var node = new NodeModel(c);
                    flatList.Add(node);
                    stack.Push(node);
                }
            }
            return flatList;
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

        public void GetDistinctVariables(ExpressionModel expressionModel)
        {
            string vars = "";
            foreach (var n in expressionModel.TreeNodes)
            {
                if (!_operatorService.IsOperator(n.Value) && !n.Value.Equals(_operatorService.Not)) { vars += n.Value; }

            }
            var distinct = new string(vars.Distinct().ToArray());
            distinct = Alphabetize(distinct);

            expressionModel.DistinctVariables.Clear();
            for (int i = 0; i < distinct.Length; i++)
            {
                expressionModel.DistinctVariables.Add(distinct[i]);
            }
        }

        private string Alphabetize(string s)
        {
            char[] a = s.ToCharArray();
            Array.Sort(a);
            return new string(a);
        }
    }

   
}