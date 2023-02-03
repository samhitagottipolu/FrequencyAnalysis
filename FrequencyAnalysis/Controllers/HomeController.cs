using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FrequencyAnalysis.Models;
using System.Text;
using System.IO;

namespace FrequencyAnalysis.Controllers;

public class HomeController : Controller
{
    private Dictionary<string, string> suffixes;
    private HashSet<string> stopWords;

    public HomeController()
    {
       
    }

    public IActionResult Index()
    {
        ReadTextFile(25, true, true);
        return View();
    }

    /// <summary>
    /// Returns the K most frequent words from the txt file provided by user
    /// </summary>
    /// <param name="k"></param>
    public void GetTopKFreqWords(int k)
    {

    }
    /// <summary>
    /// Parses the text file uploaded by the user.
    /// </summary>
    /// <param name="useStopWords"></param>
    /// <param name="onlyStemWords"></param>
    public void ReadTextFile(int k, bool useStopWords = false, bool onlyStemWords = false)
    {
        if (useStopWords)
            AddStopWords();

        if (onlyStemWords)
            LoadSuffixes();

        //Regular Expression to ignore any non-letter characters
        Regex rgx = new Regex("[^a-zA-Z]+");
        Dictionary<string, TokenFrequencyModel> tokenMap = new Dictionary<string, TokenFrequencyModel>();
        MinHeap minHeap = new MinHeap(k, tokenMap);

        // Create an instance of StreamReader to read from a file.
        // The using statement also closes the StreamReader.
        string filePath = @"/Users/Samhita/Documents/samplewords.txt";
        using (StreamReader sr = new StreamReader(filePath))
        {
            string line;
            // Read and display lines from the file until the end of
            // the file is reached.
            while ((line = sr.ReadLine()) != null)
            {
                string tokenizedString = rgx.Replace(line, " ");
                if (tokenizedString == String.Empty)
                    continue;
            
                string[] tokens = tokenizedString.Split(" ").Select(x => x.Trim()).ToArray();

                InsertIntoMap(k, minHeap, tokenMap, tokens, useStopWords, onlyStemWords);

            }
            sr.Close();
        }

        for (int i = 0; i < minHeap.minHeapNodes.Length; i++)
        {
            if (minHeap.minHeapNodes[i] == null)
                continue;

            string word = minHeap.minHeapNodes[i].word;
            int frequency = minHeap.minHeapNodes[i].frequency;
            Console.WriteLine(word + " " + frequency);            
        }

        //tokenMap = tokenMap.OrderByDescending(x => x.Value.frequency).ToDictionary(x => x.Key, x => x.Value);

    }
    /// <summary>
    /// Inserts the token into the hashmap 
    /// </summary>
    /// <param name="k"></param>
    /// <param name="minHeap"></param>
    /// <param name="tokenMap"></param>
    /// <param name="tokens"></param>
    /// <param name="useStopWords"></param>
    /// <param name="onlyStemWords"></param>
    public void InsertIntoMap(int k, MinHeap minHeap, Dictionary<string, TokenFrequencyModel> tokenMap, string[] tokens, bool useStopWords = false, bool onlyStemWords = false)
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];
            if (token.Length == 0 || (useStopWords && stopWords.Contains(token)))
                continue;

            //Remove common suffixes from the token
            if(onlyStemWords)
            {
                token = ReturnStemWord(token);
            }

            if (!tokenMap.ContainsKey(token))
                tokenMap[token] = new TokenFrequencyModel(1);
            else
                tokenMap[token].frequency++;

            InsertIntoMinHeap(token, tokenMap, minHeap, k);
        }
    }
    /// <summary>
    /// Inserts the token into min heap based on the frequency of the token
    /// </summary>
    /// <param name="token"></param>
    public void InsertIntoMinHeap(string token, Dictionary<string, TokenFrequencyModel> tokenMap, MinHeap minHeap, int k)
    {
        int minHeapIndex = tokenMap[token].minHeapIndex;
        int tokenFrequency = tokenMap[token].frequency;

        //If the token is already present in the heap, update the frequency and heapify
        if (minHeapIndex != -1)
        {
            minHeap.minHeapNodes[minHeapIndex].frequency = tokenFrequency;
            minHeap.FixHeapBelow(minHeapIndex, minHeap.size - 1); //the token above will still have frequencies that are lesser so just check the lower half

        }
        //If the token is not present in the heap and size is not full, add token to the heap and heapify
        else if (minHeapIndex == -1 && minHeap.size < k)
        {
            tokenMap[token].minHeapIndex = minHeap.size;
            MinHeapNode n = new MinHeapNode(token, tokenFrequency);
            minHeap.Insert(n);
        }
        //If the token is not present in the heap, but the size is full, replace the first (minimum) token with the current token if the min heap's root freq is less than curr token's frequency and heapify
        else if (minHeapIndex == -1 && minHeap.size >= k)
        {
            if (tokenFrequency > minHeap.minHeapNodes[0].frequency)
            {
                //Make sure the leafNode's minHeapIndex is reset since we're removing this token from the minHeap
                tokenMap[minHeap.minHeapNodes[0].word].minHeapIndex = -1;

                tokenMap[token].minHeapIndex = 0;
                minHeap.minHeapNodes[0] = new MinHeapNode(token, tokenFrequency);
                minHeap.FixHeapBelow(0, minHeap.size - 1);
            }
        }      

    }
    /// <summary>
    /// If user selected to remove stop words from the analysis, load the stop words and add them to the hashset
    /// </summary>
    public void AddStopWords()
    {
        stopWords = new HashSet<string>();
        string swFilePath = @"/Users/Samhita/Documents/Farmers Fridge/stopwords (1).txt";
        using (StreamReader sr = new StreamReader(swFilePath))
        {
            string line;
            // Read and display lines from the file until the end of
            // the file is reached.
            while ((line = sr.ReadLine()) != null)
            {
                if (line != String.Empty)
                    stopWords.Add(line);

            }
            sr.Close();
        }
    }

    /// <summary>
    /// Loads the most common suffixes into the hashmap along with their replacements, if any
    /// </summary>
    public void LoadSuffixes()
    {
        suffixes = new Dictionary<string, string>();
        string s = "L LZ ZL ZQ EVM EZL PZL";
        string[] suffixArr = s.Split(" ");

        for(int i=0;i<suffixArr.Length;i++)
        {
            string suffix = suffixArr[i];
            if (suffix == "ZL")
                suffixes[suffix] = "A";
            else if (suffix == "PZL")
                suffixes[suffix] = "AZ";
            else if (suffix == "EZL")
                suffixes[suffix] = "R";
            else
                suffixes[suffix] = string.Empty;
        }
    }
    /// <summary>
    /// Returns the stem word after removing the suffix from the token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public string ReturnStemWord(string token)
    {
        int maxSuffixLen = 3;
        int startIndex = token.Length - maxSuffixLen;
        while (maxSuffixLen > 0 && startIndex >= 0)
        {
            if (suffixes.ContainsKey(token.Substring(startIndex)))
            {
                return token.Substring(0, startIndex) + suffixes[token.Substring(startIndex)];
            }

            maxSuffixLen--;
            startIndex = token.Length - maxSuffixLen;
        }

        return token;
    }
}

