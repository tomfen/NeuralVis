using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeuralVis
{
    /// <summary>
    /// Interaction logic for loadDataWindow.xaml
    /// </summary>
    public partial class LoadDataWindow : Window
    {
        public DataSet dataSet;

        public LoadDataWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var items1 = items1Textbox.Text.Split(new[] {"\r\n","\r","\n"}, StringSplitOptions.RemoveEmptyEntries);
            var items2 = items2Textbox.Text.Split(new[] {"\r\n","\r","\n"}, StringSplitOptions.RemoveEmptyEntries);

            int features;
            if (!int.TryParse(featuresTextbox.Text, out features))
            {
                features = 30;
                featuresTextbox.Text = features.ToString(); 
            }

            dataSet = new DataSet(items1, items2, label1Textbox.Text, label2Textbox.Text, features);
            closeButton.IsEnabled = true;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
