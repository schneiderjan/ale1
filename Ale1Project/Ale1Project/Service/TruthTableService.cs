using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
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
            expressionModel.TruthTable.Binary = string.Empty;

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
                    if (j == expressionModel.DistinctVariables.Count)
                    {
                        expressionModel.TruthTable.Binary += values[i + j];
                    }

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

        //Note assignment 3 (06.09.17):
        //calculate the same hash for the simplified truth table
        //reverse simplification of table by looking at the stars
        //each star indicates 2^* lines need to be added.
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


        /// <summary>
        /// Simplification of truth table by Quine McCluskey algorithm
        /// </summary>
        /// <param name="expressionModel"></param>
        public List<string> SimplifyTruthTable(ExpressionModel expressionModel)
        {
            //check for contradiction
            //there are not always n + 1 groups
            int nrOfGroups = expressionModel.DistinctVariables.Count + 1; //nr of vars. + 1
            var rows = new List<string>();
            var truthRows = new List<string>(); //aka Epsilon m
            var implicants = new Dictionary<int, string>();

            foreach (var str in expressionModel.TruthTable.Rows)
            {
                var x = Regex.Replace(str, @"\s+", "");
                rows.Add(x);
            }

            for (int i = 1; i < rows.Count; i++)
            {
                if (rows[i][expressionModel.DistinctVariables.Count] == '1')
                {
                    truthRows.Add(rows[i]);
                }
            }

            //search for columns that have 0, 1, ..., n number of 1's in the row
            //maybe a dictionary? row | group
            //assign group numbers to truth rows
            for (var index = 0; index < truthRows.Count; index++)
            {
                int counter = 0;
                var truthRow = truthRows[index];
                for (var i = 0; i < expressionModel.DistinctVariables.Count; i++)
                {
                    if (truthRow[i].Equals('1'))
                    {
                        counter++;
                    }
                }

                //add counter and row to implicants
                string implicant = truthRow.Remove(truthRow.Length - 1);
                implicants.Add(counter, implicant);
                //replace by key value pair /./ the dict i mean

            }

            if (implicants.Count > 1)
            {
                //recursive stuff
                MinimizeImplicants(implicants, nrOfGroups, expressionModel);
            }
            //some un-simplifiable stuff. probs a tautology then jan must think of something here

            return new List<string>();
        }

        private List<string> MinimizeImplicants(Dictionary<int, string> implicants, int nrOfGroups, ExpressionModel expressionModel)
        {
            int counter = 0;
            Dictionary<int, string> nextOrderImplicants = new Dictionary<int, string>();

            for (int i = 0; i < nrOfGroups; i++)
            {
                //check if not last group
                if (i < nrOfGroups - 1)
                {
                    var currentGroups = implicants.Where(x => x.Key == i);
                    if (currentGroups.Count()==0)
                    {
                        continue;
                    }
                    IEnumerable<KeyValuePair<int, string>> nextGroups;

                    //find the next greater key in implicants. 
                    //binary search returns next key if found. if not it finds next greatest as negative complement.
                    int index = 50000;
                    List<int> keysList = new List<int>(implicants.Keys);
                    index = keysList.BinarySearch(i + 1);
                    if (index < 0)
                    {
                        index = index * -1;
                        nextGroups = implicants.Where(x => x.Key == index);
                    }
                    else
                    {
                        continue;
                    }



                    foreach (var currentGroup in currentGroups)
                    {
                        List<int> indicesToBeReplacedByAsterix = new List<int>();

                        foreach (var nextGroup in nextGroups)
                        {
                            //for every in the current group need to check every item in next group
                            //it needs to be check whether exactly one difference exists in row,
                            //e.g. current: 00; next 01. then new implicant 0*
                            //however only index can be change at a given time
                            for (int j = 0; j < expressionModel.DistinctVariables.Count; j++)
                            {
                                if (currentGroup.Value[j] == '1' && nextGroup.Value[j] == '0'
                                    || currentGroup.Value[j] == '0' && nextGroup.Value[j] == '1')
                                {
                                    indicesToBeReplacedByAsterix.Add(j);
                                }
                            }
                        }

                        string newImplicant = currentGroup.Value;
                        if (indicesToBeReplacedByAsterix.Count == 1)
                        {

                            //replace values with * and add to new implicants
                            for (int k = 0; k < newImplicant.Length; k++)
                            {
                                if (indicesToBeReplacedByAsterix.Contains(k))
                                {
                                    StringBuilder sb = new StringBuilder(newImplicant) {[k] = '*'};
                                    newImplicant = sb.ToString();
                                }
                            }
                        }
                     

                    nextOrderImplicants.Add(i, newImplicant);
                    }
                }
            }

            if (nextOrderImplicants.Count > 0)
            {
                MinimizeImplicants(nextOrderImplicants, nrOfGroups, expressionModel);
            }

            //build simplified truth table

            return new List<string>();
        }

        //public List<string> SimplifyTruthTable(ExpressionModel expressionModel)
        //{
        //    List<string> rows = new List<string>();
        //    List<string> truthRows = new List<string>();
        //    List<string> result = new List<string>();

        //    foreach (var str in expressionModel.TruthTable.Rows)
        //    {
        //        var x = Regex.Replace(str, @"\s+", "");
        //        rows.Add(x);
        //    }
        //    //truthTable = rows;

        //    for (int i = 1; i < rows.Count; i++)
        //    {
        //        if (rows[i][expressionModel.DistinctVariables.Count] == '1')
        //        {
        //            truthRows.Add(rows[i]);
        //        }
        //    }

        //    if (truthRows.Count >= 2)
        //    {
        //        for (int i = 0; i < expressionModel.DistinctVariables.Count; i++) //Columns i
        //        {
        //            for (int j = 1; j < rows.Count; j++) //Rows j
        //            {
        //                int simplifiable = 0;

        //                for (int k = 1; k < rows.Count; k++)
        //                {
        //                    if (rows[j][expressionModel.DistinctVariables.Count] == rows[k][expressionModel.DistinctVariables.Count]
        //                        && rows[j][expressionModel.DistinctVariables.Count] == '1'
        //                        && rows[j][i] == rows[k][i])
        //                    {
        //                        simplifiable++;
        //                    }
        //                }

        //                if (simplifiable > 1)
        //                {
        //                    string leftside = String.Empty, rightside = string.Empty;

        //                    for (int t = 0; t < i; t++)
        //                    {
        //                        leftside += "*\t";
        //                    }
        //                    for (int t = i + 1; t < expressionModel.DistinctVariables.Count; t++)
        //                    {
        //                        rightside += "*\t";
        //                    }

        //                    string tautology = String.Empty;
        //                    if (rows[j][i] == '1')
        //                    {
        //                        tautology = leftside + "0\t" + rightside + "1";
        //                        if (result.Contains(tautology))
        //                        {
        //                            result.Remove(tautology);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        tautology = leftside + "1\t" + rightside + "1";
        //                    }

        //                    result.Add(leftside + rows[j][i] + "\t" + rightside + "1");
        //                }
        //                else
        //                {
        //                    if (rows[j][expressionModel.DistinctVariables.Count] != '1')
        //                    {
        //                        string simplified = "";
        //                        for (int t = 0; t < rows[j].Length; t++)
        //                        {
        //                            simplified += rows[j][t] + "\t";
        //                        }
        //                        result.Add(simplified);
        //                    }
        //                }
        //            }
        //        }

        //    }
        //    else
        //    {
        //        for (int i = 1; i < rows.Count; i++)
        //        {
        //            var simplifedrow = "";
        //            for (int j = 0; j < rows[i].Length; j++)
        //            {
        //                simplifedrow = simplifedrow + rows[i][j] + "\t";
        //            }
        //            result.Add(simplifedrow);
        //        }
        //    }

        //    //add something for 0 rows
        //    result = result.Distinct().ToList();
        //    result.Insert(0, expressionModel.TruthTable.Rows[0]);
        //    expressionModel.TruthTable.RowsSimplified = result;
        //    return result;
        //}

        public string GetDisjunctiveNormalForm(ExpressionModel expressionModel)
        {
            //PART 1 Extract all formulas out truth table

            //makes sure that prefix AND for any row has proper syntax.
            //aka. if you have variables ABC then one row is |(&(&(A,B),C)) with OR
            int counter;
            var disjunctiveNormalForm = string.Empty;
            var formulas = new List<string>();
            //loop distinct variables
            //on each index check value.
            var tableRowsWithoutTabs = new List<string>();
            foreach (var truthTableRow in expressionModel.TruthTable.Rows)
            {
                tableRowsWithoutTabs.Add(Regex.Replace(truthTableRow, @"\t", ""));
            }

            for (var i = 1; i < tableRowsWithoutTabs.Count; i++)
            {
                var truthTableRow = tableRowsWithoutTabs[i];
                var formula = string.Empty;
                //makes sure that prefix AND for any row has proper syntax.
                //aka. if you have variables ABC then one row is |(&(&(A,B),C)) with OR
                //~ if variablecount > 2 need to additional AND
                counter = 0;

                var answer = truthTableRow[truthTableRow.Length - 1];
                if (answer.Equals('0')) continue;

                for (var index = 0; index < expressionModel.DistinctVariables.Count; index++)
                {
                    var variable = expressionModel.DistinctVariables[index];
                    var value = truthTableRow[index];


                    if (formula == string.Empty)
                    {
                        formula = $"&(";
                    }

                    formula = AddVariableToFormula(counter, value, formula, variable);
                    counter++;

                    if (index == expressionModel.DistinctVariables.Count - 1)
                    {
                        formulas.Add(formula);
                    }
                }
            }


            //PART 2 Add OR operator to all formulas and create disjunctive normal form
            counter = 0;
            foreach (var formula in formulas)
            {
                if (counter == 0)
                {
                    disjunctiveNormalForm = $"|({formula}";
                }
                else if (counter == 1)
                {
                    disjunctiveNormalForm += $"), {formula}";
                }
                //else if (counter == formulas.Count - 1)
                //{
                //    disjunctiveNormalForm += $"), {formula})";
                //}
                else
                {
                    disjunctiveNormalForm = disjunctiveNormalForm.Insert(0, "|(");
                    disjunctiveNormalForm += $"),{formula}";
                }
                counter++;
            }
            disjunctiveNormalForm += ")";

            expressionModel.DisjunctiveNormalForm = disjunctiveNormalForm;
            return disjunctiveNormalForm;
        }

        private string AddVariableToFormula(int counter, char value, string formula, char variable)
        {
            if (counter == 0)
            {
                if (value.Equals('0'))
                {
                    formula += $"~{variable},";
                }
                else
                {
                    formula += $"{variable},";
                }
            }
            else if (counter == 1)
            {
                if (value.Equals('0'))
                {
                    formula += $"~{variable})";
                }
                else
                {
                    formula += $"{variable})";
                }
            }
            else //counter 2
            {
                formula = formula.Insert(0, "&(");

                if (value.Equals('0'))
                {
                    formula += $",~{variable})";
                }
                else
                {
                    formula += $",{variable})";
                }
            }
            return formula;
        }


        public string GetSimplifiedDisjunctiveNormalForm(ExpressionModel expressionModel)
        {
            //makes sure that prefix AND for any row has proper syntax.
            //aka. if you have variables ABC then one row is |(&(&(A,B),C)) with OR
            int counter = 0;
            var simplifiedDisjunctiveNormalForm = string.Empty;
            var formulas = new List<string>();
            //loop distinct variables
            //on each index check value.
            var tableRowsWithoutTabs = new List<string>();
            foreach (var truthTableRow in expressionModel.TruthTable.RowsSimplified)
            {
                var row = Regex.Replace(truthTableRow, @"\t", "");
                if (row.Last() == '1')
                {
                    tableRowsWithoutTabs.Add(row);
                }
            }

            foreach (var truthTableRow in tableRowsWithoutTabs)
            {
                for (var innerIndex = 0; innerIndex < expressionModel.DistinctVariables.Count; innerIndex++)
                {
                    var value = truthTableRow[innerIndex];
                    var variable = expressionModel.DistinctVariables[innerIndex];

                    simplifiedDisjunctiveNormalForm = AddVariableToDisjunctiveNormalForm(value, ref counter, variable, simplifiedDisjunctiveNormalForm);
                }
            }
            return simplifiedDisjunctiveNormalForm;
        }

        private string AddVariableToDisjunctiveNormalForm(char value, ref int counter, char variable,
            string simplifiedDisjunctiveNormalForm)
        {
            if (value.Equals('1'))
            {
                if (counter.Equals(0))
                {
                    //add or and first half,
                    //|(A, ... 
                    simplifiedDisjunctiveNormalForm += $"|({variable},";
                }
                else if (counter.Equals(1))
                {
                    //add second half,
                    //|(A,B)
                    simplifiedDisjunctiveNormalForm += $"{variable})";
                }
                else //everything above 1
                {
                    //Add additional OR's
                    //|(| ... (A,B),C, ... )
                    simplifiedDisjunctiveNormalForm = simplifiedDisjunctiveNormalForm.Insert(0, "|(");
                    simplifiedDisjunctiveNormalForm += $",{variable})";
                }
                counter++;
            }
            else if (value.Equals('0'))
            {
                if (counter.Equals(0))
                {
                    //add or and first half,
                    //|(A, ... 
                    simplifiedDisjunctiveNormalForm += $"|(~{variable},";
                }
                else if (counter.Equals(1))
                {
                    //add second half,
                    //|(A,B)
                    simplifiedDisjunctiveNormalForm += $"~{variable})";
                }
                else //everything above 1
                {
                    //Add additional OR's
                    //|(| ... (A,B),C, ... )
                    simplifiedDisjunctiveNormalForm = simplifiedDisjunctiveNormalForm.Insert(0, "|(");
                    simplifiedDisjunctiveNormalForm += $",~{variable})";
                }
                counter++;
            }
            return simplifiedDisjunctiveNormalForm;
        }
    }
}
