using System;
using System.Collections;
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
        private List<string> _simplifiedTruthTable = new List<string>();
        //think you dont need to know the table values 
        //just important what the end value in the row was
        private List<string> _reversedTruthTable = new List<string>();
        //for debugging
        int nrOfRecursions = 0;


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
            expressionModel.TruthTable.Binary = new string(charArray);
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
            List<ImplicantModel> implicants = new List<ImplicantModel>();
            _simplifiedTruthTable.Clear();


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
                implicants.Add(new ImplicantModel(counter, implicant, counter));

            }

            if (implicants.Count > 1)
            {
                //recursive stuff
                _simplifiedTruthTable = MinimizeImplicants(implicants, nrOfGroups, expressionModel);
            }
            //some un-simplifiable stuff. probs a tautology then jan must think of something here
            if (_simplifiedTruthTable.Any())
            {
                expressionModel.TruthTable.RowsSimplified = _simplifiedTruthTable;
                return _simplifiedTruthTable;
            }

            expressionModel.TruthTable.RowsSimplified = expressionModel.TruthTable.Rows;
            return expressionModel.TruthTable.Rows;
        }

        public List<string> RevertTruthTableSimplification(ExpressionModel DnfExpressionModel, string originalBinary)
        {
            if (DnfExpressionModel.TruthTable.RowsSimplified.Any(x => x.Contains('*')))
            {
                var revertedTruthTable = new List<string>();
                var revertedTruthTableWithOutTabs = new List<string>();

                var binary = string.Empty;

                revertedTruthTable.Add(DnfExpressionModel.TruthTable.Rows.FirstOrDefault());

                for (var index = 1; index < DnfExpressionModel.TruthTable.Rows.Count; index++)
                {
                    var row = DnfExpressionModel.TruthTable.Rows[index];

                    if (row[row.Length - 1].Equals('0'))
                    {
                        var zeroRow = DnfExpressionModel.TruthTable.RowsSimplified.First(x => x.EndsWith("0\t"));
                        revertedTruthTable.Add(zeroRow);
                    }
                    else if (row[row.Length - 1].Equals('1'))
                    {
                        var oneRow = DnfExpressionModel.TruthTable.RowsSimplified.First(x => x.EndsWith("\t1"));
                        revertedTruthTable.Add(oneRow);
                    }
                }

                var temp = new List<string>(revertedTruthTable);
                foreach (var row in temp)
                {
                    var x = Regex.Replace(row, @"\s+", "");
                    revertedTruthTableWithOutTabs.Add(x);
                }
                for (var index = 1; index < revertedTruthTableWithOutTabs.Count; index++)
                {
                    var row = revertedTruthTableWithOutTabs[index];
                    var value = row[DnfExpressionModel.DistinctVariables.Count];
                    binary += value;
                }

                DnfExpressionModel.TruthTable.Binary = binary;
                return revertedTruthTable;
            }
            else
            {
                DnfExpressionModel.TruthTable.Binary = originalBinary;
                return DnfExpressionModel.TruthTable.Rows;
            }
        }


        private List<string> MinimizeImplicants(List<ImplicantModel> implicants, int nrOfGroups, ExpressionModel expressionModel)
        {

            Debug.WriteLine($"iteration: {nrOfRecursions++}");

            List<ImplicantModel> nextOrderImplicants = new List<ImplicantModel>();

            for (int i = 0; i < nrOfGroups; i++)
            {
                //check if not last group
                if (i < nrOfGroups - 1)
                {

                    var currentGroupsImplicants = implicants.Where(x => x.Group == i).ToList();
                    if (!currentGroupsImplicants.Any())
                    {
                        continue;
                    }

                    //find the next greater key in implicants. 
                    //binary search returns index (i+1) if found. if not it finds next greatest as negative complement.
                    //if nothing is found count is returned
                    List<int> possibleNextGroups =
                        (from implicant in implicants select implicant.Group).Distinct().ToList();
                    possibleNextGroups.Sort();
                    var index = possibleNextGroups.BinarySearch(i + 1);
                    index = Math.Abs(index);
                    if (index >= possibleNextGroups.Count) //see binarySearch docu
                    {
                        continue;
                    }
                    //if i get the error here then i somehow need to keep the old group
                    var groupNumber = possibleNextGroups[index];
                    var nextGroupsImplicants = implicants.Where(x => x.Group == groupNumber).ToList();

                    foreach (var currentGroupImplicant in currentGroupsImplicants)
                    {
                        List<int> indicesToBeReplacedByAsterix = new List<int>();

                        foreach (var nextGroupImplicant in nextGroupsImplicants)
                        {
                            indicesToBeReplacedByAsterix.Clear();
                            //for every in the current group need to check every item in next group
                            //it needs to be check whether exactly one difference exists in row,
                            //e.g. current: 00; next 01. then new implicant 0*
                            //however only index can be change at a given time
                            for (int j = 0; j < expressionModel.DistinctVariables.Count; j++)
                            {
                                if (currentGroupImplicant.Implicant[j] == '1' &&
                                    nextGroupImplicant.Implicant[j] == '0'
                                    || currentGroupImplicant.Implicant[j] == '0' &&
                                    nextGroupImplicant.Implicant[j] == '1')
                                {
                                    indicesToBeReplacedByAsterix.Add(j);
                                }
                            }

                            string newImplicantCurrentGroup = currentGroupImplicant.Implicant;
                            string newImplicantNextGroup = nextGroupImplicant.Implicant;
                            //add also from next group
                            if (indicesToBeReplacedByAsterix.Count == 1)
                            {
                                //replace values with * and add to new implicants
                                //for current and next group implicant
                                var k = indicesToBeReplacedByAsterix.FirstOrDefault();

                                //currentgroup
                                StringBuilder sb1 = new StringBuilder(newImplicantCurrentGroup) { [k] = '*' };
                                newImplicantCurrentGroup = sb1.ToString();
                                //var newGroupNumber1 = newImplicantCurrentGroup.Count(x => x.Equals('1'));
                                if (nextOrderImplicants.All(x => x.Implicant != newImplicantCurrentGroup))
                                {
                                    nextOrderImplicants.Add(new ImplicantModel(currentGroupImplicant.OriginalNrOfOnes,
                                         newImplicantCurrentGroup, currentGroupImplicant.OriginalNrOfOnes));
                                }
                                //nextgroup
                                StringBuilder sb2 = new StringBuilder(newImplicantNextGroup) { [k] = '*' };
                                newImplicantNextGroup = sb2.ToString();
                                var newGroupNumber2 = newImplicantNextGroup.Count(x => x.Equals('1'));
                                //if (!newImplicantNextGroup.Equals(newImplicantCurrentGroup))
                                if (nextOrderImplicants.All(x => x.Implicant != newImplicantNextGroup))
                                {
                                    nextOrderImplicants.Add(new ImplicantModel(nextGroupImplicant.OriginalNrOfOnes, newImplicantNextGroup, nextGroupImplicant.OriginalNrOfOnes));
                                }

                            }
                        }


                    }
                }
                //we compare to the last group but we also need to keep them for next order
                //else
                //{
                //    var lastGroupsImplicants = implicants.Where(x => x.Group == i).ToList();
                //    if (!lastGroupsImplicants.Any())
                //    {
                //        continue;
                //    }
                //    nextOrderImplicants.AddRange(lastGroupsImplicants);
                //}
            }

            //if for one nextOrderImplicants row length - 1 = number of *
            //then simplification is complete
            //thats however not always true. lets distinctVariables plus 2 and * exist then i end recursion
            //

            //if nextorderimplicants == 0 then check if there are stars in old order implicants
            //if they contain stars return them in combination with old truth table
            //if (nextOrderImplicants.Count == 0)
            //{

            //when continuationFlag is true then the table can be simplified further
            //if for one nextOrderImplicants row length - 1 = number of *
            //then simplification is complete
            bool continuationFlag = false;
            if (nextOrderImplicants.Count > 0)
            {
                foreach (var nextOrderImplicant in nextOrderImplicants)
                {
                    if (continuationFlag)
                    {
                        break;
                    }

                    int dontCareCounter = 0;
                    foreach (var c in nextOrderImplicant.Implicant)
                    {
                        if (c.Equals('*'))
                        {
                            dontCareCounter++;
                        }
                    }

                    var newNrOfGroups = nextOrderImplicants.Max(x => x.Group) + 1;
                    if (AreImplicantsMinimizable(expressionModel, nextOrderImplicants, newNrOfGroups))
                    {
                        //break;
                        continuationFlag = true;
                    }
                    //if (nextOrderImplicant.Implicant.Length - 1 != dontCareCounter)
                    //{
                    //    continuationFlag = true;
                    //}
                }

                if (continuationFlag)
                {
                    var newNrOfGroups = nextOrderImplicants.Max(x => x.Group) + 1; //see n+1 groups
                    MinimizeImplicants(nextOrderImplicants, newNrOfGroups, expressionModel);
                }
                else
                {
                    _simplifiedTruthTable = CreateSimplifiedTruthTable(expressionModel, nextOrderImplicants);
                }
            }

            return _simplifiedTruthTable;
        }

        private bool AreImplicantsMinimizable(ExpressionModel expressionModel, List<ImplicantModel> implicants, int nrOfGroups)
        {
            List<int> replacableIndicesCounter = new List<int>();
            for (int i = 0; i < nrOfGroups; i++)
            {
                if (i < nrOfGroups - 1)
                {
                    var currentGroupsImplicants = implicants.Where(x => x.Group == i).ToList();
                    if (!currentGroupsImplicants.Any())
                    {
                        continue;
                    }

                    //find the next greater key in implicants. 
                    //binary search returns index (i+1) if found. if not it finds next greatest as negative complement.
                    //if nothing is found count is returned
                    List<int> possibleNextGroups =
                        (from implicant in implicants select implicant.Group).Distinct().ToList();
                    possibleNextGroups.Sort();
                    var index = possibleNextGroups.BinarySearch(i + 1);
                    index = Math.Abs(index);
                    if (index == possibleNextGroups.Count) //see binarySearch docu
                    {
                        continue;
                    }
                    //if i get the error here then i somehow need to keep the old group
                    var groupNumber = possibleNextGroups[index];
                    var nextGroupsImplicants = implicants.Where(x => x.Group == groupNumber).ToList();

                    foreach (var currentGroupImplicant in currentGroupsImplicants)
                    {
                        List<int> indicesToBeReplacedByAsterix = new List<int>();

                        foreach (var nextGroupImplicant in nextGroupsImplicants)
                        {
                            indicesToBeReplacedByAsterix.Clear();
                            //for every in the current group need to check every item in next group
                            //it needs to be check whether exactly one difference exists in row,
                            //e.g. current: 00; next 01. then new implicant 0*
                            //however only index can be change at a given time
                            for (int j = 0; j < expressionModel.DistinctVariables.Count; j++)
                            {
                                if (currentGroupImplicant.Implicant[j] == '1' &&
                                    nextGroupImplicant.Implicant[j] == '0'
                                    || currentGroupImplicant.Implicant[j] == '0' &&
                                    nextGroupImplicant.Implicant[j] == '1')
                                {
                                    indicesToBeReplacedByAsterix.Add(j);
                                }
                            }
                            replacableIndicesCounter.Add(indicesToBeReplacedByAsterix.Count);
                        }
                    }
                }
            }

            if (!replacableIndicesCounter.Any())
            {
                return false;
            }
            else if (replacableIndicesCounter.Any(x => x == 1))
            {
                return true;
            }
            return false;
        }

        private List<string> CreateSimplifiedTruthTable(ExpressionModel expressionModel, List<ImplicantModel> nextOrderImplicants)
        {
            List<string> simplifiedTruthTable = new List<string>();

            //add header
            simplifiedTruthTable.Add(expressionModel.TruthTable.Rows[0]);

            //add rows with 0 as result
            List<string> zeroRows = new List<string>();
            foreach (var str in expressionModel.TruthTable.Rows)
            {
                var x = Regex.Replace(str, @"\s+", "");
                zeroRows.Add(x);
            }
            for (int i = 1; i < zeroRows.Count; i++)
            {
                if (zeroRows[i][expressionModel.DistinctVariables.Count] == '0')
                {
                    var row = Regex.Replace(zeroRows[i], ".{1}", "$0\t");
                    simplifiedTruthTable.Add(row);
                }
            }

            //add simplified rows
            foreach (var nextOrderImplicant in nextOrderImplicants)
            {
                var row = Regex.Replace(nextOrderImplicant.Implicant, ".{1}", "$0\t");
                row += "1";
                simplifiedTruthTable.Add(row);
            }

            return simplifiedTruthTable;
        }

        public string GetDisjunctiveNormalForm(ExpressionModel expressionModel)
        {
            //is contradiction
            if (expressionModel.TruthTable.Hexadecimal.Equals("0"))
            {
                expressionModel.DisjunctiveNormalForm = expressionModel.Prefix;
                return expressionModel.Prefix;
            }
            //is tautology but only with one variable
            else if(expressionModel.DistinctVariables.Count == 1 && 
                expressionModel.TruthTable.Binary.Distinct().Count() == 1 && 
                expressionModel.TruthTable.Binary.Contains('1'))
            {
                expressionModel.DisjunctiveNormalForm = expressionModel.Prefix;
                return expressionModel.Prefix;
            }
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
                if (answer.Equals('0'))
                {
                    continue;
                }

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
            //is contradiction or tautology
            if (expressionModel.TruthTable.Hexadecimal.Equals("0") ||
                (expressionModel.TruthTable.Binary.Distinct().Count() == 1 && expressionModel.TruthTable.Binary.Contains('1')))
            {
                expressionModel.DisjunctiveNormalForm = expressionModel.Prefix;
                return expressionModel.Prefix;
            }
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

            //something when there is only one row?

            foreach (var truthTableRow in tableRowsWithoutTabs)
            {
                for (var innerIndex = 0; innerIndex < expressionModel.DistinctVariables.Count; innerIndex++)
                {
                    var value = truthTableRow[innerIndex];
                    var variable = expressionModel.DistinctVariables[innerIndex];
                    if (!value.Equals('*'))
                    {
                        simplifiedDisjunctiveNormalForm = AddVariableToDisjunctiveNormalForm(value, ref counter, variable, simplifiedDisjunctiveNormalForm);
                    }
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
