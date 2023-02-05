using System;
namespace FrequencyAnalysis.Models
{
    public class TokenFrequencyModel
    {
        public int frequency;
        public int minHeapIndex;
        public int suffixFrequency;
        public HashSet<string> suffixes;

        public TokenFrequencyModel(int frequency)
        {
            this.frequency = frequency;
            this.minHeapIndex = -1;
            suffixFrequency = 0;
        }
    }
}

