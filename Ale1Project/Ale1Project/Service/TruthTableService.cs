using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ale1Project.Model;

namespace Ale1Project.Service
{
    public class TruthTableService : ITruthTableService
    {
        private readonly IOperatorService _operatorService;

        public TruthTableService(IOperatorService operatorService)
        {
            _operatorService = operatorService;
        }

        public List<string> GetTruthTable(ExpressionModel expressionModel)
        {
            expressionModel.TruthTable.Rows.Clear();
            var header = expressionModel.DistinctVariables[0].ToString();
            for (var i = 1; i < expressionModel.DistinctVariables.Count; i++) header = header + "\t" + expressionModel.DistinctVariables[0];

            header = header + "\t" + expressionModel.Infix;
            expressionModel.TruthTable.Rows.Add(header);

            expressionModel.TruthTable.TableValues = GenerateTableInput(expressionModel.DistinctVariables.Count);

            bool[] answer1 = new bool[expressionModel.TruthTable.TableValues.GetLength(0)];
            for (int i = 0; i < expressionModel.TruthTable.TableValues.GetLength(0); i++)
            {
                for (int j = 0; j < expressionModel.TruthTable.TableValues.GetLength(1); j++)
                {
                    SetValue(expressionModel.DistinctVariables[j], expressionModel.TruthTable.TableValues[i, j], expressionModel);
                }
                answer1[i] = Solve(expressionModel);
            }
            expressionModel.TruthTable.Answers = answer1;

            var values = new List<string>();
            for (int i = 0; i < expressionModel.TruthTable.TableValues.GetLength(0); i++)
            {
                values.Add(GetBoolRepresentation(expressionModel.TruthTable.TableValues[i, 0]));
                for (int j = 1; j < expressionModel.TruthTable.TableValues.GetLength(1); j++)
                {
                    values.Add(GetBoolRepresentation(expressionModel.TruthTable.TableValues[i, j]));
                }
                values.Add(GetBoolRepresentation(answer1[i]));
            }

            for (int i = 0; i < values.Count; i = i + values.Count + 1)
            {
                var row = "";
                row = values[i];

                for (int j = 1; j < values[i].Length + 1; j++)
                {
                    row = row + "\t" + values[i + j];
                    if (j == values.Count) expressionModel.TruthTable.Binary += values[i + j];
                }
                expressionModel.TruthTable.Rows.Add(row);
            }

            return expressionModel.TruthTable.Rows;
        }

        private string GetBoolRepresentation(bool p0)
        {
            return p0 ? "1" : "0";
        }

        private bool Solve(ExpressionModel expressionModel)
        {
            Stack<bool> stack = new Stack<bool>();

            foreach (var t in expressionModel.TreeNodesReversed)
            {
                if (!_operatorService.IsOperator(t.Value)
                   && !t.Value.Equals(_operatorService.Not))
                {
                    stack.Push(t.BoolValue);
                }
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

        private void SetValue(char c, bool value, ExpressionModel expressionModel)
        {
            char ch = char.ToUpper(c);
            foreach (var node in expressionModel.TreeNodesReversed)
            {
                if (node.Value == ch.ToString())
                {
                    node.BoolValue = value;
                }
            }
        }

        public string CalculateHash(ExpressionModel expressionModel)
        {

            return "";
        }

        private bool[,] GenerateTableInput(int nrOfColumns)
        {
            var rows = (int)Math.Pow(2, nrOfColumns);
            var table = new bool[rows, nrOfColumns];
            var divider = rows;

            // columns
            for (var c = 0; c < nrOfColumns; c++)
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
            //tableVals = table;
            return table;
        }


        //var hexadecimal = "";
        //var tableValues = Helper.GenerateTable(tbPrefix.Text);

        //for (int i = 0; i < tableValues.Count; i = i + tbValues.Text.Length + 1)
        //{
        //    var row = "";
        //    row = tableValues[i];

        //    for (int j = 1; j < tbValues.Text.Length + 1; j++)
        //    {
        //        row = row + "\t" + tableValues[i + j];
        //        if (j == tbValues.Text.Length) hexadecimal = hexadecimal + tableValues[i + j];
        //    }
        //    lvTruthTable.Items.Add(row);
        //}

        //tbHash.Text = Helper.HexaDecimal(hexadecimal);
    }
}
