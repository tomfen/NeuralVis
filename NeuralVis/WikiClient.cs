using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NeuralVis
{
    class WikiClient
    {
        static public String[] getPageExtract(String articleName)
        {
            WebClient client = new WebClient();

            List<String> tokens = new List<string>();

            String url = "http://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&explaintext=1&titles=" + articleName;

            using (Stream stream = client.OpenRead(url))
            using (StreamReader reader = new StreamReader(stream))
            {
                JsonSerializer ser = new JsonSerializer();
                Result result = ser.Deserialize<Result>(new JsonTextReader(reader));

                foreach (Page page in result.query.pages.Values)
                {
                    Regex rgx = new Regex("[^'a-zA-Z -]");
                    var str = rgx.Replace(page.extract, " ");
                    tokens.AddRange(str.Split());
                }
            }

            return tokens.Count > 0? tokens.ToArray() : null;
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
