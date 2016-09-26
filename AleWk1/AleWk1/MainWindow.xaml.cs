using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
            //"&(=(A,B),|(C,D))";
            //"=( >(A,B), |( ~(A) ,B) ) 
            //&(A, ~(B)) 
            //&(=(A,B),|(C,D)) 
            btnParse.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void btnParse_Click(object sender, RoutedEventArgs e)
        {
            var input = tbPrefix.Text
                .Replace(",", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(@" ", "")
                .Trim();

            var prefixInput = Helper.ConvertStringToList(input);
            var reversedPrefixInput = Helper.ReverseList(prefixInput);

            ParseInfix(reversedPrefixInput);
            ShowTable();
        }

        private void ParseInfix(List<string> reversedPrefixInput)
        {
            var flatList = Helper.GetFlatList(reversedPrefixInput);
            tbInfix.Text = Helper.GetInfixString(flatList);

            tbValues.Text = Helper.GetDistinctVariables(flatList);

            Helper.WriteToFile(flatList);
            Helper.DisplayGraph();
        }

        private void ShowTable()
        {
            // Helper.Maketable(lvTruthTable)
            var header = tbValues.Text[0].ToString();
            for (var i = 1; i < tbValues.Text.Length; i++)
            {
                header = header + "     " + tbValues.Text[i];
            }

            header = header + "     " + tbInfix.Text;

            lvTruthTable.Items.Add(header);

            var hexadecimal = "";

            var tableValues = Helper.GenerateTable(tbPrefix.Text);

            for (int i = 0; i < tableValues.Count; i = i + tbValues.Text.Length + 1)
            {
                var row = "";

                row = tableValues[i];

                for (int j = 1; j < tbValues.Text.Length + 1; j++)
                {
                    row = row + "     " + tableValues[i + j];

                    if (j == tbValues.Text.Length)
                        hexadecimal = hexadecimal + tableValues[i + j];

                }

                lvTruthTable.Items.Add(row);
            }



            tbHash.Text = Helper.HexaDecimal(hexadecimal);
        }
    }
}
