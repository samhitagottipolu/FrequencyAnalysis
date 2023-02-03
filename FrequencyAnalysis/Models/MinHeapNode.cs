using System;
namespace FrequencyAnalysis.Models
{
    public class MinHeapNode
    {
        public string word;
        public int frequency;

        public MinHeapNode(string word, int frequency)
        {
            this.word = word;
            this.frequency = frequency;
        }
    }
}

