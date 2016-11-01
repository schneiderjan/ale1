using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AleWk1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            tbPrefix.Text = "&((|(A,~(B)),C)";
            //&(|(A,B),>(C,~(&(|(D,E),>(&(|(F,G),>(H,~(&(|(I,J),>(K,~(L)))))),~(M))))))
            //"&(=(A,B),|(C,D))"
            //"&((|(A,~(B)),C)"
            //"=( >(A,B), |( ~(A) ,B) ) 
            //&(A, ~(B))
            //&(=(A,B),|(C,D))
            //~(&(~(&(A,C)),~(&(~(B),C))))
            btnParse.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void btnParse_Click(object sender, RoutedEventArgs e)
        {
            var input = tbPrefix.Text
                .Replace(",", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(@" ", "")
                .Trim().ToUpper();

            var prefixInput = Helper.ConvertStringToList(input);
            var reversedPrefixInput = Helper.ReverseList(prefixInput);

            ParseInfix(reversedPrefixInput);
            ShowTable();
            ShowSimplifiedTruthTable();
            ShowDisjunctiveNormalForm();
            ShowDisjunctiveNormalFormSimp();
            ShowNand();
            ShowHashes();
        }


        private void ParseInfix(List<string> reversedPrefixInput)
        {
            var flatList = Helper.GetFlatList(reversedPrefixInput);
            tbInfix.Text = Helper.GetInfixString(flatList);

            tbValues.Text = Helper.GetDistinctVariables(flatList);

            //Helper.WriteToFile(flatList);
            //Helper.DisplayGraph();
        }

        private void ShowTable()
        {
            lvTruthTable.Items.Clear();
            // Helper.Maketable(lvTruthTable)
            var header = tbValues.Text[0].ToString();
            for (var i = 1; i < tbValues.Text.Length; i++) header = header + "\t" + tbValues.Text[i];

            header = header + "\t" + tbInfix.Text;
            lvTruthTable.Items.Add(header);

            var hexadecimal = "";
            var tableValues = Helper.GenerateTable(tbPrefix.Text);

            for (int i = 0; i < tableValues.Count; i = i + tbValues.Text.Length + 1)
            {
                var row = "";
                row = tableValues[i];

                for (int j = 1; j < tbValues.Text.Length + 1; j++)
                {
                    row = row + "\t" + tableValues[i + j];
                    if (j == tbValues.Text.Length) hexadecimal = hexadecimal + tableValues[i + j];
                }
                lvTruthTable.Items.Add(row);
            }

            tbHash.Text = Helper.HexaDecimal(hexadecimal);

        }
        private void ShowSimplifiedTruthTable()
        {
            var tableValues = Helper.GenerateTableSimplified(lvTruthTable);
            lvSimplifiedTruthTable.Items.Clear();
            if (tableValues.Count == 1)
            {
                foreach (var x in lvTruthTable.Items) lvSimplifiedTruthTable.Items.Add(x);
            }
            else
            {
                foreach (var x in tableValues) lvSimplifiedTruthTable.Items.Add(x);
            }
        }
        private void ShowDisjunctiveNormalForm()
        {
            string dj = Helper.GetDisjunctiveNormalForm(lvTruthTable);
            tbDisjunctiveNormalForm.Text = dj;
        }

        private string GetHexString(List<string> tableValues)
        {
            string hexadecimal = "";
            for (int i = 0; i < tableValues.Count; i = i + tbValues.Text.Length + 1)
            {
                for (int j = 1; j < tbValues.Text.Length + 1; j++)
                {
                    if (j == tbValues.Text.Length) hexadecimal = hexadecimal + tableValues[i + j];
                }
            }
            return hexadecimal;
        }

        private void ShowDisjunctiveNormalFormSimp()
        {
            string dj = Helper.GetDisjunctiveNormalFormSimplified(lvSimplifiedTruthTable);
            if (String.IsNullOrEmpty(dj)) tbDisjunctiveNormalFormSimplified.Text = tbDisjunctiveNormalForm.Text;
            else tbDisjunctiveNormalFormSimplified.Text = dj;
        }
        private void ShowNand()
        {
            string nand = Helper.GetNand(tbPrefix.Text);
            tbNand.Text = nand;
        }

        private void ShowHashes()
        {
            //DisjunctiveNormal
            var dj = tbDisjunctiveNormalForm.Text.Replace(",", "")
                 .Replace("(", "")
                 .Replace(")", "")
                 .Replace(@" ", "")
                 .Trim().ToUpper();
            var stringList = Helper.ConvertStringToList(dj);
            var flatList = Helper.GetFlatList(Helper.ReverseList(stringList));
            Helper.GetInfixString(flatList);
            var tableValues = Helper.GenerateTable(dj);
            tbHashDisj.Text = Helper.HexaDecimal(GetHexString(tableValues));

            //DisjunctiveNormalSimplified
            var djSimp = tbDisjunctiveNormalFormSimplified.Text.Replace(",", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(@" ", "")
                .Trim().ToUpper();
            var djSimpList = Helper.ConvertStringToList(djSimp);
            var djSimpListFlat = Helper.GetFlatList(Helper.ReverseList(djSimpList));
            Helper.GetInfixString(djSimpListFlat);
            var tableValuesDisjSimp = Helper.GenerateTable(djSimp);
            tbHashDisjSimp.Text = Helper.HexaDecimal(GetHexString(tableValuesDisjSimp));

            //Nand Hash
            var nand = tbDisjunctiveNormalFormSimplified.Text.Replace(",", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(@" ", "")
                .Trim().ToUpper();
            var nandSimpList = Helper.ConvertStringToList(nand);
            var nandSimpListFlat = Helper.GetFlatList(Helper.ReverseList(nandSimpList));
            Helper.GetInfixString(nandSimpListFlat);
            var tableValuesNand = Helper.GenerateTable(nand);
            tbHashNand.Text = Helper.HexaDecimal(GetHexString(tableValuesNand));
        }
    }
}
