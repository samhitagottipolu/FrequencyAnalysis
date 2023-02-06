using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FrequencyAnalysis.Models;
using System.Text;
using System.IO;
using System.Runtime;
using System;

namespace FrequencyAnalysis.Controllers;

public class HomeController : Controller
{
    private Dictionary<string, string> suffixes;
    private HashSet<string> stopWords;

    public IActionResult Index()
    {
        GetTopKFreqWords();
        return View();
    }

    #region Processing User Input and reading from .txt file
    /// <summary>
    /// Returns the K most frequent words from the txt file provided by user
    /// </summary>
    /// <param name="k"></param>
    public void GetTopKFreqWords()
    {
        ProcessUserInput();
    }

    public void ProcessUserInput()
    {
        try
        {
            string swFilePath = string.Empty;
            bool useStopWords = false;
            bool onlyStemWords = false;

            Console.WriteLine("Please provide the file path");
            string? userFilePath = Console.ReadLine();
 
            if (String.IsNullOrEmpty(userFilePath) || (!System.IO.File.Exists(userFilePath)))
                throw new Exception("The provided file path is not valid. Please provide a valid path");                

            Console.WriteLine("Would you like to only analyze stem words? Y/N");
            string? stemWords = Console.ReadLine() ;
            if (!String.IsNullOrEmpty(stemWords) && stemWords.ToLower() == "y")
                onlyStemWords = true;

            Console.WriteLine("Would you like to include any stop words to remove them from the analysis? Y/N");
            string? stopwords = Console.ReadLine();
            if (!String.IsNullOrEmpty(stopwords) && stopwords.ToLower() == "y")
            {
                Console.WriteLine("Please provide the stop words txt file path");
                swFilePath = Console.ReadLine();
                if(String.IsNullOrEmpty(swFilePath) || !System.IO.File.Exists(swFilePath))
                   throw new Exception("The provided file path is not valid. Please provide a valid path");

                useStopWords = true;
            }

            //Number of most frequent words
            int k = 25;
            ReadTextFile(k, useStopWords, onlyStemWords, userFilePath, swFilePath);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    /// <summary>
    /// Parses the text file uploaded by the user.
    /// </summary>
    /// <param name="k">Total count of most frequent words to analyze</param>
    /// <param name="useStopWords">Whether or not to exclude stop words</param>
    /// <param name="onlyStemWords">Whether or not to include only stem words</param>
    public void ReadTextFile(int k, bool useStopWords, bool onlyStemWords, string userFilePath, string swFilePath)
    {
        if (useStopWords)
            AddStopWords(swFilePath);

        if (onlyStemWords)
            LoadSuffixes();

        //Regular Expression to ignore any non-letter characters
        Regex rgx = new Regex("[^a-zA-Z]+");

        //Initialize dictionary and min heap
        Dictionary<string, TokenFrequencyModel> tokenMap = new Dictionary<string, TokenFrequencyModel>();
        MinHeap minHeap = new MinHeap(k, tokenMap);

        // Create an instance of StreamReader to read from a file.
        // The using statement also closes the StreamReader.
        using (StreamReader sr = new StreamReader(userFilePath))
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
        }
        
        WriteAnalysisToFile(k, minHeap, userFilePath, useStopWords, onlyStemWords);
        PrintActualWords(tokenMap);
    }

    /// <summary>
    /// If user selected to remove stop words from the analysis, load the stop words and add them to the hashset
    /// </summary>
    public void AddStopWords(string swFilePath)
    {
        stopWords = new HashSet<string>();
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
        }
    }

    /// <summary>
    /// Loads the most common suffixes into the dictionary along with their replacements, if any
    /// </summary>
    public void LoadSuffixes()
    {
        suffixes = new Dictionary<string, string>
        {
            ["L"] = "",
            ["LZ"] ="",
            ["ZL"] = "A",
            ["ZQ"] = "",
            ["EVM"] = "",
            ["EZL"] = "R",
            ["PZL"] = "AZ"
        };
    }
    #endregion

    #region Token insertion and frequency updates
    /// <summary>
    /// Inserts the token into the dictionary and updates it's frequency
    /// </summary>
    /// <param name="minHeap">Heap that holds the most frequent words</param>
    /// <param name="tokenMap">Dictionary to store all tokens</param>
    /// <param name="tokens">Array of tokens read from the supplied txt file</param>
    public void InsertIntoMap(int k, MinHeap minHeap, Dictionary<string, TokenFrequencyModel> tokenMap, string[] tokens, bool useStopWords = false, bool onlyStemWords = false)
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i].ToUpper();
            if (token.Length == 0 || (useStopWords && stopWords.Contains(token)))
                continue;

            string suffix = string.Empty;
            //Remove common suffixes from the token
            if(onlyStemWords)
            {
                token = ReturnStemWord(token, ref suffix);
            }

            if (!tokenMap.ContainsKey(token))
                tokenMap[token] = new TokenFrequencyModel(1);
            else
                tokenMap[token].frequency++;

            if(suffix != string.Empty)
            {
                tokenMap[token].suffixFrequency++;
                if (tokenMap[token].suffixes == null)
                    tokenMap[token].suffixes = new HashSet<string>();

                tokenMap[token].suffixes.Add(suffix);
            }

            InsertIntoMinHeap(token, tokenMap, minHeap, k);
        }
    }

    /// <summary>
    /// Inserts the token into min heap based on the frequency of the token
    /// If the min-heap has a token with frequency lesser than current token frequency, it removes the root token and replaces it with current token
    /// </summary>
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
    /// Returns the stem word after removing the suffix from the token.
    /// Checks in the order of largest suffix to smaller suffix for a given token
    /// </summary>
    /// <returns></returns>
    public string ReturnStemWord(string token, ref string suffix)
    {
        int maxSuffixLen = 3;
        int startIndex = token.Length - maxSuffixLen;
        while (maxSuffixLen > 0 && startIndex >= 0)
        {
            if (suffixes.ContainsKey(token.Substring(startIndex)))
            {
                suffix = token.Substring(startIndex);
                return token.Substring(0, startIndex) + suffixes[token.Substring(startIndex)];
            }

            maxSuffixLen--;
            startIndex = token.Length - maxSuffixLen;
        }

        return token;
    }
    #endregion

    #region Output analysis and writing to txt file
    /// <summary>
    /// Writes the analysis to a txt file.
    /// Maintains 10 most recent analysis txt files in the folder
    /// </summary>
    /// <param name="fileToCopy">User supplied txt file</param>
    public void WriteAnalysisToFile(int k, MinHeap minHeap, string fileToCopy, bool useStopWords, bool onlyStemWords)
    {
        try
        {
            string outputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            outputDirectory = Path.Combine(outputDirectory, "Documents/Farmers_Fridge_Output"); //@"/Users/(username)/Documents/Farmers_Fridge_Output";

            //Creates a new folder 'Farmers_Fridge_Output' if it doesn't already exist
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(outputDirectory);

            //store the total text files count
            int fileCount = dirInfo.GetFiles("*.txt").Length;

            //output txt file name
            string outputFileName = "FrequencyAnalysis";
            int count = 1;

            int maxFileCount = 10;

            if (fileCount > 0)
            {
                //sorts all the files by the datetime they were last written to
                FileSystemInfo[] filesInfo = dirInfo.GetFileSystemInfos(outputFileName + "*").OrderBy(x => x.LastWriteTime).ToArray();

                //If we've reach max file count, delete the oldest file to maintain the top 10 recent file analysis
                if (fileCount == maxFileCount)
                {
                    System.IO.File.Delete(filesInfo[0].FullName);
                }

                //Figures out the file count to append to the new file name
                string latestfilename = Path.GetFileNameWithoutExtension(filesInfo[filesInfo.Length - 1].Name);
                int prevCount = Int32.Parse(latestfilename.Substring(outputFileName.Length + 1, latestfilename.Length - outputFileName.Length - 2));
                count += prevCount;

            }

            outputFileName = String.Format("{0}{1}{2}", outputFileName, "(" + count + ")", ".txt");
            string filePath = System.IO.Path.Combine(outputDirectory, outputFileName);

            //Copy the originally supplied text file to the output folder
            System.IO.File.Copy(fileToCopy, filePath, true);

            // Create an instance of StreamReader to write to a file.
            // The using statement also closes the StreamReader.
            using (StreamWriter outputFile = System.IO.File.AppendText(filePath))
            {
                outputFile.WriteLine(Environment.NewLine);
                outputFile.WriteLine("------------------------------------------------");
                outputFile.WriteLine("WORD FREQUENCY ANALYSIS: TOP " + k.ToString() + " WORDS");
                outputFile.WriteLine("Analyzed only stem words: " + onlyStemWords.ToString());
                outputFile.WriteLine("Excluded stop words: " + useStopWords.ToString());
                outputFile.WriteLine("------------------------------------------------");

                foreach (var node in minHeap.minHeapNodes)
                {
                    if (node == null)
                        continue;

                    string word = node.word;
                    string frequency = node.frequency.ToString();

                    outputFile.WriteLine(word + " : " + frequency);
                }
            }

            Console.WriteLine("Analysis successfully completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not complete analysis");
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Prints all words that are actual words and not root words that end with the given suffixes
    /// </summary>
    public void PrintActualWords(Dictionary<string,TokenFrequencyModel> tokenMap)
    {
        //Find all words that have the same frequency with or without the suffix making them an actual word.
        var actualWords = tokenMap.
            Where(x => x.Value.frequency == x.Value.suffixFrequency && x.Value.suffixes.Count() == 1).
            Select(x => x.Key).ToList();

        if (actualWords.Count() > 0)
        {
            //print them to the console
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Actual words that are not root words");
            foreach (var actualWord in actualWords)
            {
                string s = tokenMap[actualWord].suffixes.First();
                if (suffixes[s] != String.Empty)
                {
                    int toRemove = suffixes[s].Length;
                    Console.WriteLine(actualWord.Substring(0, actualWord.Length - toRemove) + s);
                }
                else
                    Console.WriteLine(actualWord + s);
            }
        }
    }
    #endregion
}

