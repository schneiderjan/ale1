using System;
using System.Collections.Generic;
using System.Windows;

namespace AleWk1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> operators;
                public MainWindow()
        {
            InitializeComponent();
            operators = new List<string>()
            {
                "~",
                "&",
                "|",
                ">",
                "=",
            };
        }

        private void btnParse_Click(object sender, RoutedEventArgs e)
        {
            ParseInfix(tbPrefix.Text.Trim());
        }

        private void ParseInfix(string text)
        {
            GetTree(text);
        }

        private void GetTree(string text)
        {
            
        }
    }
}
