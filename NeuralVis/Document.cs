using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralVis
{
    public class Document
    {
        public String[] tokens;
        public int category;

        public Document(String[] text, int category)
        {
            this.category = category;

            List<String> filtered = new List<string>();

            foreach (String token in text)
            {
                if (!String.IsNullOrEmpty(token))
                {
                    filtered.Add(token.ToLower());
                }
            }
            this.tokens = filtered.ToArray();
        }

        public double[] toVector(Vocabulary v)
        {
            double[] ret = new double[v.words.Length];
            foreach (String word in tokens)
            {
                if (v.inverse.ContainsKey(word))
                    ret[v.inverse[word]] = 1.0;
            }
            return ret;
        }
    }
}
