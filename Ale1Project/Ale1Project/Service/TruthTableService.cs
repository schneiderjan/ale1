using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            for (var i = 1; i < expressionModel.DistinctVariables.Count; i++)
            {
                header = header + "\t" + expressionModel.DistinctVariables[i];
            }

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

            for (int i = 0; i < values.Count; i = i + expressionModel.DistinctVariables.Count + 1)
            {
                var row = "";
                row = values[i];

                for (int j = 1; j < expressionModel.DistinctVariables.Count + 1; j++)
                {
                    row = row + "\t" + values[i + j];
                    if (j == expressionModel.DistinctVariables.Count) expressionModel.TruthTable.Binary += values[i + j];
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
            char[] charArray = expressionModel.TruthTable.Binary.ToCharArray();
            Array.Reverse(charArray);
            expressionModel.TruthTable.Binary = new string(charArray);

            int rest = expressionModel.TruthTable.Binary.Length % 4;
            if (rest != 0)
            {
                expressionModel.TruthTable.Binary = new string('0', 4 - rest) + expressionModel.TruthTable.Binary;
            }

            string output = "";
            for (int i = 0; i <= expressionModel.TruthTable.Binary.Length - 4; i += 4)
            {
                output += $"{Convert.ToByte(expressionModel.TruthTable.Binary.Substring(i, 4), 2):X}";
            }
            expressionModel.TruthTable.Hexadecimal = output;
            return output;
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
            return table;
        }

        public List<string> SimplifyTruthTable(ExpressionModel expressionModel)
        {
            List<string> rows = new List<string>();
            List<string> truthRows = new List<string>();
            List<string> result = new List<string>();

            foreach (var str in expressionModel.TruthTable.Rows)
            {
                var x = Regex.Replace(str.ToString(), @"\s+", "");
                rows.Add(x);
            }
            //truthTable = rows;

            for (int i = 1; i < rows.Count; i++)
            {
                if (rows[i][expressionModel.DistinctVariables.Count] == '1') truthRows.Add(rows[i]);
            }

            if (truthRows.Count > 2)
            {
                for (int i = 0; i < expressionModel.DistinctVariables.Count; i++) //Columns i
                {
                    for (int j = 1; j < rows.Count; j++) //Rows j
                    {
                        int simplifiable = 0;

                        for (int k = 1; k < rows.Count; k++)
                        {
                            if (rows[j][expressionModel.DistinctVariables.Count] == rows[k][expressionModel.DistinctVariables.Count]
                                && rows[j][expressionModel.DistinctVariables.Count] == '1'
                                && rows[j][i] == rows[k][i]) simplifiable++;
                        }

                        if (simplifiable > 1)
                        {
                            string leftside = "", rightside = "";

                            for (int t = 0; t < i; t++) leftside += "*\t";
                            for (int t = i + 1; t < expressionModel.DistinctVariables.Count; t++) rightside += "*\t";

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
                            if (rows[j][expressionModel.DistinctVariables.Count] != '1')
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
            result.Insert(0, expressionModel.TruthTable.Rows[0]);
            expressionModel.TruthTable.RowsSimplified = result;
            return result;
        }

        public string GetDisjunctiveNormalForm(ExpressionModel expressionModel)
        {
            foreach (var truthTableAnswer in expressionModel.TruthTable.Answers)
            {
                if (truthTableAnswer)
                {

                }
                else
                {
                    
                }
            }

            return "";
        }
    }
}
