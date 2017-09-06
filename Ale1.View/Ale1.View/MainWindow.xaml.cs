using Ale1.Functional;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
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
            int n;
            if (int.TryParse(textBoxRandom.Text, out n))
            {
                textBoxInput.Text = RandomTree.Make(n);
                ButtonParse_Click(sender, null);
            }
            else
            {
                MessageBox.Show("Insert number");
            }
        }
    }
}
