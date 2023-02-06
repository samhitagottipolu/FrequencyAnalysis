
 # Project Title
Frequency Analysis

## Description
This is a C# application that generates k most frequent words from a user supplied text document. 
 - Allows the user to exclude any provided stop words from the analysis.
 - Allows the user to analyze only stem words by removing the common suffixes.
 - The original supplied text, k most frequent words, and stop/stem word settings get saved to a .txt file inside your documents in a new folder called Farmers_Fridge_Output.
 - This folder always maintains the top 10 most recent analysis and removes any older files from the folder.
 - When analyzing only stem words, a list of all actual words (words that are not root words but happen to end with a suffix) are printed to the console.
 - How does it work?
   - The application parses each word (ignores any non letter characters) and uses a Dictionary to store each word and it's frequency.
   - A min-heap of size k is used to keep track of k most frequent words. 
   - Once the heap has reached maximum capacity, the root node (node with minimum frequency) is swapped with a new word of higher frequency and is heapified.
   
## Getting Started

### Dependencies

* IDE: Visual Studio Community 2022 for Mac
* Language: C#
* Target Framework: .NET 6.0
* Browser: Google Chrome

### Installing

* Download VS Community Mac [here](https://visualstudio.microsoft.com/vs/mac/)

### Executing program

#### Setup
* Once the solution has been loaded to Visual Studio, right click on the project and select properties.
* Go to Run -> Configuration -> Default and check "Run in Terminal Window"
* <img width="1091" alt="image" src="https://user-images.githubusercontent.com/11053617/217100450-656a4489-8ba0-416a-b3b9-d45e0defb484.png">
* The build conguration should be set to Debug and select Chrome as the default browser.
* ![image](https://user-images.githubusercontent.com/11053617/217101689-849c9efc-58a7-49c5-b807-9bfbbef30be4.png)
#### Input
* Once the application starts, it asks to provide the .txt file path for analyzing. Make sure the file exists and provide the full path.
![image](https://user-images.githubusercontent.com/11053617/217103219-110fb194-5ca0-45a6-b59a-c513afa4cbe6.png)
* To exclude any stop words, add them to a txt file and provide the path in the terminal when asked.
#### Output
* The output folder should save the output txt files under the name 'FrequencyAnalysis'
![image](https://user-images.githubusercontent.com/11053617/217104126-b8890107-5774-4a1b-b1ce-c17dc4d83bc7.png)
<img width="482" alt="image" src="https://user-images.githubusercontent.com/11053617/217104519-d46bad7e-a211-4622-951e-72c96fcc8f7e.png">



## Author
Samhita Gottipolu


 

