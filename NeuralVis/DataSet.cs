using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Linq;
namespace NeuralVis
{
    public class DataSet
    {
        static HashSet<String> stopwords = Stopwords.English;

        public double[][] Input { get; private set; }
        public double[][] Output { get; private set; }

        public String[] InputLabels { get; set; }
        public String[] OutputLabels { get; set; }

        public Vocabulary voc;

        public List<Document> docs1 = new List<Document>();
        public List<Document> docs2 = new List<Document>();

        public DataSet(String[] items1, String[] items2, String label1, String label2, int features)
        {
            foreach (String item in items1)
            {
                try
                {
                    var doc = new Document(WikiClient.getPageExtract(item), 0);
                    docs1.Add(doc);
                }
                catch { };
            }

            foreach (String item in items2)
            {
                try
                {
                    var doc = new Document(WikiClient.getPageExtract(item), 1);
                    docs2.Add(doc);
                }
                catch { };
            }


            List<String> allWords = new List<String>();
            foreach (Document doc in docs1)
            {
                allWords.AddRange(doc.tokens);
            }
            foreach (Document doc in docs2)
            {
                allWords.AddRange(doc.tokens);
            }

            var mostCommon = allWords
                .Where(s => !stopwords.Contains(s))
                .Where(s => s.Length >= 2)
                .GroupBy(x => x)
                .Select(x => new
                    {
                        KeyField = x.Key,
                        Count = x.Count()
                    })
                .OrderByDescending(x => x.Count)
                .Take(features)
                .Select(x => x.KeyField);

            voc = new Vocabulary(mostCommon.ToArray());

            List<double[]> inputVectors = new List<double[]>();
            List<double[]> outputVectors = new List<double[]>();

            foreach (Document doc in docs1)
            {
                inputVectors.Add(doc.toVector(voc));
                outputVectors.Add(new[] { 1.0, 0.0 });
            }
            foreach (Document doc in docs2)
            {
                inputVectors.Add(doc.toVector(voc));
                outputVectors.Add(new[] { 0.0, 1.0 });
            }

            this.Input = inputVectors.ToArray();
            this.Output = outputVectors.ToArray();
            this.InputLabels = voc.words;
            this.OutputLabels = new[] { label1, label2 };
        }
    }

    
}
