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
        public ChartValues<ObservablePoint> ValuesA { get; set; }
        public ChartValues<ObservablePoint> ValuesB { get; set; }
        public LineSeries errorsSeries = new LineSeries
                {
                    PointGeometry = null,
                    Values = new ChartValues<double> {}
                };
        public SeriesCollection errorsCollection = new SeriesCollection ();

        private double  learningRate = 0.1;
        private double  alphaValue = 2.0;
        private int     maxIterations = 0;
        private bool    IterationsLimited { get { return maxIterations != 0; } }
        private Point[] P;
        private double[] C;
        private NetworkDrawer drawer;

        private Thread workerThread = null;
        private volatile bool stopWorkerThread = false;
        private int[] layerNodes = new int[] {3,3,2,2};


        private void reportProgress(int iteration, double error)
        {
            iterationTextblock.Text = iteration.ToString();
            errorTextblock.Text = error.ToString("0.00000000");

            drawer.update();

            if (errorsSeries.Values.Count > 500)
                errorsSeries.Values.RemoveAt(0);
            errorsSeries.Values.Add(error);
        }

        public MainWindow()
        {
            InitializeComponent();

            errorChart.Series = errorsCollection;
            errorsCollection.Add(errorsSeries);
        }

        private void loadDataButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "CSV (Comma delimited) (*.csv)|*.csv|All files (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    String[] lines = File.ReadAllLines(ofd.FileName);

                    P = new Point[lines.Length];
                    C = new double[lines.Length];

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var vals = lines[i].Split(new char[3] { '\t', ',', ';' });
                        double x = double.Parse(vals[0], CultureInfo.InvariantCulture);
                        double y = double.Parse(vals[1], CultureInfo.InvariantCulture);
                        P[i] = new Point(x, y);
                        C[i] = double.Parse(vals[2], CultureInfo.InvariantCulture);
                    }

                    ValuesA = new ChartValues<ObservablePoint>();
                    ValuesB = new ChartValues<ObservablePoint>();

                    for (int i = 0; i < P.Length; i++)
                    {
                        if (C[i] == 0)
                            ValuesA.Add(new ObservablePoint(P[i].X, P[i].Y));
                        else if (C[i] == 1)
                            ValuesB.Add(new ObservablePoint(P[i].X, P[i].Y));
                    }

                    DataContext = null;
                    DataContext = this;
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
                learningRate = 0.2;

            if (!double.TryParse(alphaValueTextbox.Text, out alphaValue))
                alphaValue = 2.0;

            if (!int.TryParse(maxIterationsTextbox.Text, out maxIterations))
                maxIterations = 0;
        }

        void updateGuiValues()
        {
            learningRateTextbox.Text = learningRate.ToString();
            alphaValueTextbox.Text = alphaValue.ToString();
            maxIterationsTextbox.Text = maxIterations.ToString();
        }

        
		void enableControls(bool enable) {
            loadButton.IsEnabled = enable;
            startButton.IsEnabled = enable;
            stopButton.IsEnabled = !enable;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            updateGuiValues();
            enableControls(true);
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            enableControls(false);
            readGuiValues();
            updateGuiValues();

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
            this.drawer = new NetworkDrawer(canvas, network);
        }

        private double[][] getVectors(Point[] p)
        {
            int samples = p.Length;
            double[][] vectors = new double[samples][];

            for (int i = 0; i < samples; i++)
            {
                vectors[i] = new double[2] { p[i].X, p[i].Y};
            }

            return vectors;
        }

        private void networkWork()
        {
            Dispatcher.Invoke(new Action(delegate()
            {
                errorsSeries.Values.Clear();
            }));

            int features = 2;
            int samples = P.Length;

            double[][] input = getVectors(P);
            double[][] output = new double[samples][];

            for (int i = 0; i < samples; i++)
            {
                output[i] = new double[2];

                if (C[i] == 0)
                    output[i][0] = 1;
                else
                    output[i][1] = 1;
            }

            ActivationNetwork network = new ActivationNetwork(
                new SigmoidFunction(alphaValue), features, layerNodes);
            ActivationLayer layer = network.Layers[0] as ActivationLayer;
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

                Dispatcher.Invoke(new Action(delegate() {
                    reportProgress(iteration, error);
                }));

                iteration++;
                Thread.Sleep(1);
            }

            Dispatcher.Invoke(new Action(delegate()
            {
                enableControls(true);
            }));

            

            /*// show perceptron's weights
            ClearList(weightsList);
            for (int i = 0; i < neuronsCount; i++)
            {
                string neuronName = string.Format("Neuron {0}", i + 1);
                ListViewItem item = null;

                // add all weights
                for (int j = 0; j < variables; j++)
                {
                    item = AddListItem(weightsList, neuronName);
                    AddListSubitem(item, string.Format("Weight {0}", j + 1));
                    AddListSubitem(item, layer.Neurons[i].Weights[0].ToString("F6"));
                }
                // threshold
                item = AddListItem(weightsList, neuronName);
                AddListSubitem(item, "Threshold");
                AddListSubitem(item, ((ActivationNeuron)layer.Neurons[i]).Threshold.ToString("F6"));
            }*/

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            stopWorkerThread = true;
            if (workerThread != null)
                workerThread.Join(500);
        }


    } 
}
