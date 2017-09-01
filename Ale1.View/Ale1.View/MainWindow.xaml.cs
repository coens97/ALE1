using Ale1.Parser;
using System;
using System.IO;
using System.Windows;

namespace Ale1.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonParse_Click(object sender, RoutedEventArgs e)
        {
            labelWrongInput.Content = string.Empty;
            try
            {
                GenerateGraph();
            }
            catch (ArgumentException ex) // Exceptions inside parser are ArgumentException
            {
                labelWrongInput.Content = ex.Message;
            }
        }

        private void GenerateGraph()
        {
            var tree = TextToTree.Parse(textBoxInput.Text);
            var dotText = TreeToDot.ToText(tree);
            File.WriteAllLines("dot\\generated.dot", dotText);
        }
    }
}
