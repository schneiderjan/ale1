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
            
            
            tbPrefix.Text = "=( >(A,B), |( ~(A) ,B) )";
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

            Helper.WriteToFile(flatList);
            Helper.DisplayGraph(imgGraph);
            //imgGraph.Source = new BitmapImage(new Uri("dot.png", UriKind.Relative));
            //D:\Code\GitHub\4_fourth_year_block1\ale1\AleWk1\AleWk1\bin\Debug


            //List<Node> infixs = new List<Node>();
            //var output = "";
            //foreach (var i in infixs)
            //{
            //    output += i.Value;
            //}
            //tbInfix.Text = output;
        }

        

       
    }
}

////n is a child with right/left with a not as parent.
//if (n.LeftChild == null 
//    && n.RightChild == null
//    && flatList[flatList.IndexOf(n) +1].LeftChild == null
//    && flatList[flatList.IndexOf(n) + 1].RightChild == null)
//{
//    n.Parent = flatList[flatList.IndexOf(n) + 3];
//}
//else if (n.LeftChild == null
//    && n.RightChild == null
//    )
//{
//    n.Parent = flatList[flatList.IndexOf(n) + 1];
//}
////n must be a NOT operator if either one operand is null
////and first condition is already true.
//else if (n.LeftChild == null
//    || n.RightChild == null
//    && n.Value.Equals(not))
//{
//    n.Parent = flatList[flatList.IndexOf(n) + 1];
//}
//else if (operators.Contains(flatList[flatList.IndexOf(n) + 1].Value))
//{
//    n.Parent = flatList[flatList.IndexOf(n) + 1];
//}
//else if (operators.Contains(flatList[flatList.IndexOf(n)].Value))
//{
//    n.Parent = flatList[flatList.IndexOf(n) + 1];
//}