using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using LiveCharts;
using System.Globalization;
using LiveCharts.Defaults;
using System.Threading;
using AForge.Neuro;
using AForge.Neuro.Learning;
using System.Collections;
using LiveCharts.Wpf;

namespace NeuralVis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SeriesCollection errorsCollection = new SeriesCollection ();
        private LinechartManager errorchartManager;

        private double      learningRate = 0.3;
        private double      alphaValue = 0.5;
        private int         maxIterations = 0;
        private bool        IterationsLimited { get { return maxIterations != 0; } }
        private DataSet     dataSet;
        private NetworkDrawer drawer;
        private ActivationNetwork network;

        private Thread workerThread = null;
        private volatile bool stopWorkerThread = false;
        private int[] hiddenNodes = new int[]{10};

        private void reportProgress(int iteration, double error)
        {
            iterationTextblock.Text = iteration.ToString();
            errorTextblock.Text = error.ToString("0.00000000");

            errorchartManager.add(error);
        }

        private void reportWorkFinish()
        {
            errorchartManager.pushBuffer();
            drawer.update();
            queryTextbox.IsEnabled = true;
            computeButton.IsEnabled = true;
            enableControls(true);
        }

        public MainWindow()
        {
            InitializeComponent();

            errorChart.Series = errorsCollection;
            errorchartManager = new LinechartManager(errorsCollection);
            errorChart.AxisX[0].LabelFormatter = val => (val * 1000).ToString();
        }

        private void loadDataButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDataWindow ldw = new LoadDataWindow();
            if (ldw.ShowDialog() == true)
            {
                try
                {
                    dataSet = ldw.dataSet;
                    startButton.IsEnabled = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("Error reading file");
                }
            }
        }

        void readGuiValues()
        {
            if (!double.TryParse(learningRateTextbox.Text, out learningRate))
                learningRate = 0.3;

            if (!double.TryParse(alphaValueTextbox.Text, out alphaValue))
                alphaValue = 0.5;

            if (!int.TryParse(maxIterationsTextbox.Text, out maxIterations))
                maxIterations = 0;

            try
            {
                if (layersTextbox.Text.Length == 0)
                {
                    hiddenNodes = new int[0];
                }
                else
                {
                    String[] layers = layersTextbox.Text.Split(new char[] { ',', ';', ' ' });
                    hiddenNodes = new int[layers.Length];
                    for (int i = 0; i < layers.Length; i++)
                    {
                        hiddenNodes[i] = Math.Max(1, int.Parse(layers[i]));
                    }
                }
            }
            catch
            {
                hiddenNodes = new int[]{2};
            }
        }

        void updateGuiValues()
        {
            learningRateTextbox.Text = learningRate.ToString();
            alphaValueTextbox.Text = alphaValue.ToString();
            maxIterationsTextbox.Text = maxIterations.ToString();

            layersTextbox.Text = String.Join(",", hiddenNodes);
        }

        
		void enableControls(bool enable) {
            loadButton.IsEnabled = enable;
            startButton.IsEnabled = enable;
            clearButton.IsEnabled = enable;
            stopButton.IsEnabled = !enable;
            layersTextbox.IsEnabled = enable;
            alphaValueTextbox.IsEnabled = enable;
            learningRateTextbox.IsEnabled = enable;
            maxIterationsTextbox.IsEnabled = enable;
            keepPreviousCheckbox.IsEnabled = enable;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            updateGuiValues();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            enableControls(false);
            readGuiValues();
            updateGuiValues();

            if (keepPreviousCheckbox.IsChecked == false)
                errorchartManager.clear();

            errorchartManager.newSeries();

            stopWorkerThread = false;
            workerThread = new Thread(new ThreadStart(networkWork));
            workerThread.Start();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stopWorkerThread = true;
            workerThread.Join(100);
            workerThread = null;
        }

        private void createDrawer(ActivationNetwork network)
        {
            this.drawer = new NetworkDrawer(canvas, network, dataSet);
        }

        private void networkWork()
        {
            int features = dataSet.Input[0].Length;
            int samples = dataSet.Input.Length;

            double[][] input = dataSet.Input;
            double[][] output = dataSet.Output;

            int[] layers = new int[hiddenNodes.Length + 1];
            Array.Copy(hiddenNodes, layers, hiddenNodes.Length);
            layers[layers.Length - 1] = 2; 

            network = new ActivationNetwork(
                new SigmoidFunction(alphaValue), features, layers);

            BackPropagationLearning teacher = new BackPropagationLearning(network);
            teacher.LearningRate = learningRate;

            // iterations
            int iteration = 1;


            Dispatcher.Invoke(new Action(delegate()
            {
                canvas.Children.Clear();
                createDrawer(network);
            }));

            while (!stopWorkerThread && (!IterationsLimited || iteration <= maxIterations))
            {
                double error = teacher.RunEpoch(input, output) / samples;

                if (iteration % (1000) == 0)
                Dispatcher.Invoke(new Action(delegate() {
                    reportProgress(iteration, error);
                }));

                iteration++;
            }

            Dispatcher.Invoke(new Action(delegate()
            {
                reportWorkFinish();
            }));

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            stopWorkerThread = true;
            if (workerThread != null)
                workerThread.Join(500);
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            errorchartManager.clear();
        }



        /////////////////////////////////////////////////////////////////////////////////////
        private void computeButton_Click(object sender, RoutedEventArgs e)
        {
            String[] s = WikiClient.getPageExtract(queryTextbox.Text);
            var doc = new Document(s, -1);
            var v = doc.toVector(dataSet.voc);
            drawer.showCompute(v);
        }


    } 
}
