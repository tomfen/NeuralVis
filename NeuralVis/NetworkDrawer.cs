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
        public static double margin = 50;
        public static double padding = 100;

        private class Node
        {
            private int i;
            private int j;
            public Ellipse Circle { get; private set; }
            public Neuron n { get; private set; }

            public static double size = 50;
            public bool IsInput { get { return n == null; } }

            public Node(int i, int j, Neuron n)
            {
                this.i = i;
                this.j = j;
                this.n = n;

                Circle = new Ellipse();
                Circle.Height = Node.size;
                Circle.Width = Node.size;
                Circle.Fill = Brushes.White;
                Circle.Stroke = Brushes.Black;
                Circle.StrokeThickness = 2;

            }

            public Point Position { get {
                return new Point(i * padding + margin, j * padding + margin);
            }}
            
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

                line.ToolTip = Weigth;
            }
            
        }

        Canvas canvas;
        ActivationNetwork network;

        Node[][] nodes;
        List<Connection> connections;

        public NetworkDrawer(Canvas canvas, ActivationNetwork network)
        {
            this.canvas = canvas;
            this.network = network;
            this.connections = new List<Connection>();

            nodes = new Node[1 + network.Layers.Length][];

            nodes[0] = new Node[network.InputsCount];

            for (int i = 0; i < network.InputsCount; i++)
            {
                nodes[0][i] = new Node(0, i, null);
            }

            for (int l = 1; l < network.Layers.Length+1; l++)
            {
                Layer layer = network.Layers[l-1];
                nodes[l] = new Node[layer.Neurons.Length];

                for (int n = 0; n < layer.Neurons.Length; n++)
                {
                    Neuron neuron = layer.Neurons[n];

                    nodes[l][n] = new Node(l, n, neuron);
                    for (int w = 0; w < neuron.Weights.Length; w++)
                    {
                        if (l != 0) //AKA input layer
                            if (l == 1 && w >= network.InputsCount) continue;
                            connections.Add(new Connection(nodes[l - 1][w], nodes[l][n], w));
                    }
                }
            }

            putConnections();
            putNodes();
        }

        private void putNodes()
        {
            for (int l = 0; l < nodes.Length; l++)
            {
                for(int n= 0; n< nodes[l].Length; n++)
                {
                    Node node = nodes[l][n];
                    Canvas.SetTop(node.Circle, node.Position.Y );
                    Canvas.SetLeft(node.Circle, node.Position.X );
                    canvas.Children.Add(node.Circle);
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
                con.line.StrokeThickness = con.Weigth;
            }
        }
    }
}
