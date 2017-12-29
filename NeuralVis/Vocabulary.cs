using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralVis
{
    public class Vocabulary
    {
        public String[] words;
        public Dictionary<String, int> inverse;

        public Vocabulary(String[] words)
        {
            this.words = words;
            this.inverse = new Dictionary<String, int>();

            for (int i = 0; i < words.Length; i++)
            {
                inverse.Add(words[i], i);
            }
        }
    }
}
