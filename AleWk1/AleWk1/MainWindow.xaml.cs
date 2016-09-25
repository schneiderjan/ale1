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
            tbPrefix.Text = "&(=(A,B),|(C,D))";
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
        }

        private void ParseInfix(List<string> reversedPrefixInput)
        {
            var flatList = Helper.GetFlatList(reversedPrefixInput);
            tbInfix.Text = Helper.GetInfixString(flatList);
            
            tbValues.Text = Helper.GetDistinctVariables(flatList);

            Helper.WriteToFile(flatList);
            Helper.DisplayGraph();
        }
    }
}
