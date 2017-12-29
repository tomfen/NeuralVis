using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AForge.Neuro;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace NeuralVis
{
    class NetworkDrawer
    {
        private static double marginH = 10;
        private static double marginV = 40;
        private static double paddingH = 25;
        private static double paddingV = 100;

        private class Node
        {
            private int i;
            private int j;
            public Ellipse Circle { get; private set; }
            public Rectangle ThresholdIcon { get; private set; }
            public ActivationNeuron n { get; private set; }
            public String Label { get { return labelBlock.Text; } set { labelBlock.Text = value; } }

            internal TextBlock labelBlock;

            public static double size = 20;
            public bool IsInput { get { return n == null; } }

            public Node(int i, int j, ActivationNeuron n)
            {
                this.i = i;
                this.j = j;
                this.n = n;

                Circle = new Ellipse();
                Circle.Height = Node.size;
                Circle.Width = Node.size;
                Circle.Fill = Brushes.White;
                Circle.Stroke = Brushes.Black;
                Circle.StrokeThickness = 1;


                labelBlock = new TextBlock();
                labelBlock.TextAlignment = TextAlignment.Center;
                labelBlock.Width = Node.size;


                if (!IsInput)
                {
                    ThresholdIcon = new Rectangle();
                    ThresholdIcon.Width = 4;
                    ThresholdIcon.Height = 4;
                    updateThresholdIcon();
                }
            }

            public Point Position { get {
                return new Point(j * paddingH + marginH, i * paddingV + marginV);
            }}


            public void updateThresholdIcon()
            {
                if (!IsInput)
                {
                    ThresholdIcon.Fill = getBrush(n.Threshold);
                }
            }

            static private SigmoidFunction thresholdToColorMapper = new SigmoidFunction(2);
            static private Brush getBrush(double threshold)
            {
                byte r = (byte)(thresholdToColorMapper.Function(threshold) * 255);
                return new SolidColorBrush(Color.FromRgb(r, 0, 0));
            }
        }

        private class Connection
        {
            private Node node1;
            private Node node2;
            private int w;
            public Line line;

            public double Weigth { get{return node2.n.Weights[w];} }

            public Connection(Node node1, Node node2, int w)
            {
                this.node1 = node1;
                this.node2 = node2;
                this.w = w;

                line = new Line();
                line.X1 = node1.Position.X + Node.size / 2;
                line.Y1 = node1.Position.Y + Node.size / 2;
                line.X2 = node2.Position.X + Node.size / 2;
                line.Y2 = node2.Position.Y + Node.size / 2;
                line.Stroke = Brushes.Red;
                line.StrokeThickness = Weigth;

            }

            public void update()
            {
                line.StrokeThickness = this.Weigth * 0.2;
            }
        }

        Canvas canvas;
        ActivationNetwork network;

        Node[][] nodes;
        List<Connection> connections;

        public NetworkDrawer(Canvas canvas, ActivationNetwork network, DataSet dataSet)
        {
            this.canvas = canvas;
            this.network = network;
            this.dataSet = dataSet;
            this.connections = new List<Connection>();

            nodes = new Node[1 + network.Layers.Length][];

            nodes[0] = new Node[network.InputsCount];

            // Input nodes
            for (int i = 0; i < network.InputsCount; i++)
            {
                nodes[0][i] = new Node(0, i, null);
            }

            // Hidden and output
            for (int l = 1; l < network.Layers.Length+1; l++)
            {
                Layer layer = network.Layers[l-1];
                nodes[l] = new Node[layer.Neurons.Length];

                for (int n = 0; n < layer.Neurons.Length; n++)
                {
                    var neuron = (ActivationNeuron)layer.Neurons[n];

                    nodes[l][n] = new Node(l, n, neuron);
                    for (int w = 0; w < neuron.Weights.Length; w++)
                    {
                        if (l != 0) // not input layer
                            if (l == 1 && w >= network.InputsCount) continue;
                            connections.Add(new Connection(nodes[l - 1][w], nodes[l][n], w));
                    }
                }
            }

            putConnections();
            putNodes();
            updateCanvasSize();
            putLabels();
        }

        private void putLabels()
        {
            // input
            for (int i = 0; i < nodes[0].Length; i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = dataSet.InputLabels[i];

                double y = marginV-Node.size;
                double x = nodes[0][i].Position.X;

                tb.RenderTransform = new RotateTransform(-45, 5, 5);
                Canvas.SetTop(tb, y);
                Canvas.SetLeft(tb, x);
                canvas.Children.Add(tb);
            }

            // output
            for (int i = 0; i < dataSet.OutputLabels.Length; i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = dataSet.OutputLabels[i];

                int lastlayer = nodes.Length - 1;
                double y = canvas.Height - marginV + Node.size - 8;
                double x = nodes[lastlayer][i].Position.X + 8;

                tb.RenderTransform = new RotateTransform(45, 0, 10);
                Canvas.SetTop(tb, y);
                Canvas.SetLeft(tb, x);
                canvas.Children.Add(tb);
            }
        }

        private void updateCanvasSize()
        {
            Point maxPos = new Point();

            for (int i = 0; i < nodes.Length; i++)
            {
                for (int j = 0; j < nodes[i].Length; j++)
                {
                    maxPos.X = Math.Max(nodes[i][j].Position.X, maxPos.X);
                    maxPos.Y = Math.Max(nodes[i][j].Position.Y, maxPos.Y);
                }
            }

            maxPos.Offset(Node.size + marginH, Node.size + marginV);
            canvas.Width = maxPos.X;
            canvas.Height = maxPos.Y;
        }

        private void putNodes()
        {
            for (int l = 0; l < nodes.Length; l++)
            {
                for(int n= 0; n< nodes[l].Length; n++)
                {
                    Node node = nodes[l][n];
                    Canvas.SetTop(node.Circle, node.Position.Y );
                    Canvas.SetLeft(node.Circle, node.Position.X);
                    canvas.Children.Add(node.Circle);

                    if (!node.IsInput)
                    {
                        Canvas.SetTop(node.ThresholdIcon, node.Position.Y);
                        Canvas.SetLeft(node.ThresholdIcon, node.Position.X);
                        canvas.Children.Add(node.ThresholdIcon);
                    }

                    if (node.Label != null)
                    {
                        Canvas.SetTop(node.labelBlock, node.Position.Y+Node.size/10);
                        Canvas.SetLeft(node.labelBlock, node.Position.X);
                        canvas.Children.Add(node.labelBlock);
                    }
                }
            }
        }

        private void putConnections()
        {
            foreach (Connection con in connections)
            {
                canvas.Children.Add(con.line);
            }
        }

        public void update()
        {
            foreach (Connection con in connections)
            {
                con.update();
            }
            foreach (Node[] layer in nodes)
            {
                foreach (Node node in layer)
                {
                    node.updateThresholdIcon();
                }
            }
        }

        public void showCompute(double[] input)
        {
            for (int i = 0; i < nodes[0].Length; i++ )
            {
                nodes[0][i].Label = input[i].ToString("0.0");
            }

            network.Compute(input);

            for (int i = 1; i < nodes.Length; i++)
            {
                for (int j = 0; j < nodes[i].Length; j++)
                {
                    nodes[i][j].Label = nodes[i][j].n.Output.ToString("0.0");
                }
            }
        }

        public DataSet dataSet { get; set; }
    }
}
