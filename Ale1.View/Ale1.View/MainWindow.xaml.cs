using Ale1.Common.TreeNode;
using Ale1.Functional;
using Microsoft.FSharp.Core;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

            var dotText = TreeToDot.ToText(tree); // Generate dot file text
            File.WriteAllLines("generated.dot", dotText); // Write dot file to disk
            RunDotCommand(); // Run tool to generate image from dot file

            // Show image
            imageBox.Source = null;
            imageBox.Source = GetImageSource();

            // Show truthtable
            ShowTruthtable(tree);
        }

        private void RunDotCommand()
        {
            Process dot = new Process();
            dot.StartInfo.FileName = "dot\\dot.exe";
            dot.StartInfo.Arguments = "-Tpng -o generated.png generated.dot";
            dot.Start();
            dot.WaitForExit();
        }

        public static ImageSource GetImageSource() // Special tricks required to load file and not lock it in filesystem
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("generated.png", UriKind.Relative);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.EndInit();
            return bitmap;
        }

        private void ButtonRandom_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(textBoxRandom.Text, out int n))
            {
                textBoxInput.Text = RandomTree.Make(n);
                ButtonParse_Click(sender, null);
            }
            else
            {
                MessageBox.Show("Insert number");
            }
        }

        private void ShowTruthtable(ITreeNode tree)
        {
            var truthtable = TreeToTruthTable.CreateTruthTable(tree);

            dataGridTruth.Columns.Clear();
            dataGridTruth.Items.Clear();
            dataGridSimpleTruth.Columns.Clear();
            dataGridSimpleTruth.Items.Clear();
            // Show hex
            textBoxTruthTable.Text = BitarrayUtility.BitsToHex(truthtable.Values);
            textBoxInfix.Text = TreeToText.ToTextInfix(tree);
            // Add headers to datagrid
            var headerId = 0;
            foreach (var header in truthtable.Headers)
            {
                dataGridTruth.Columns.Add(
                    new DataGridTextColumn { Header = header, Binding = new Binding("[" + headerId + "]")
                });

                dataGridSimpleTruth.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = header, Binding = new Binding("[" + headerId + "]")
                    });
                headerId++;
            }

            dataGridTruth.Columns.Add(new DataGridTextColumn { Header = "Result", Binding = new Binding("[" + headerId + "]")});
            dataGridSimpleTruth.Columns.Add(new DataGridTextColumn { Header = "Result", Binding = new Binding("[" + headerId + "]") });
            
            // fill rows full truth table
            var rowCount = truthtable.Values.Length;
            var headerCount = truthtable.Headers.Length; // result column is not included
            for (var i = 0; i < rowCount; i++)
            {
                var variableBits = BitarrayUtility.IntToBits(headerCount, i);
                var rowValues = new string[headerCount + 1];
                int j = 0;
                foreach (var bit in BitarrayUtility.BitToSeq(variableBits))
                {
                    rowValues[j] = bit ? "1" : "0";
                    j++;
                }

                rowValues[j] = truthtable.Values.Get(i) ? "1" : "0";
                dataGridTruth.Items.Add(rowValues);
            }
            // simple truth table
            var simpleTruthtable = TruthTableSimplifier.toSimpleTruthTable(truthtable);

            foreach (var row in simpleTruthtable.Rows)
            {
                var rowValues = new string[headerCount + 1];
                int j = 0;
                foreach (var bit in row.Variables)
                {
                    // Not so pretty way to check if optional type has value
                    if (FSharpOption<bool>.get_IsSome(bit))
                    {
                        rowValues[j] = bit.Value ? "1" : "0";
                    }
                    else
                    {
                        rowValues[j] = "*";
                    }
                    j++;
                }
                rowValues[j] = row.Result ? "1" : "0";
                dataGridSimpleTruth.Items.Add(rowValues);
            }
        }
    }
}
