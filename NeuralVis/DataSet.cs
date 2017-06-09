

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
namespace NeuralVis
{
    class DataSet
    {
        public double[] Input { get; private set; }
        public double[] Output { get; private set; }

        public DataSet(String[] urls)
        {
            WebClient client = new WebClient();

            using (Stream stream = client.OpenRead("http://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&explaintext=1&titles=stack%20overflow|query"))
            using (StreamReader reader = new StreamReader(stream))
            {
                JsonSerializer ser = new JsonSerializer();
                Result result = ser.Deserialize<Result>(new JsonTextReader(reader));

                foreach (Page page in result.query.pages.Values)
                {
                    Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                    var str = rgx.Replace(page.extract, "");
                    Console.WriteLine(str);
                }
            }
        }
    }

    public class Result
    {
        public Query query { get; set; }
    }

    public class Query
    {
        public Dictionary<string, Page> pages { get; set; }
    }

    public class Page
    {
        public string extract { get; set; }
    }
}
