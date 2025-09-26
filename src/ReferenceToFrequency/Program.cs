using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ReferenceToFrequency
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ReferenceToFrequency <input_file_path>");
                Console.WriteLine("Example: ReferenceToFrequency large_text_file.txt");
                return;
            }

            string inputFilePath = args[0];
            
            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine($"Error: File '{inputFilePath}' not found.");
                return;
            }

            Console.WriteLine($"Processing file: {inputFilePath}");
            Console.WriteLine("Reading file and counting word frequencies...");

            var wordFrequencies = await ProcessFileAsync(inputFilePath);
            
            Console.WriteLine($"Found {wordFrequencies.Count} unique words");
            Console.WriteLine("Saving frequency table to JSON...");

            await SaveFrequencyTableAsync(wordFrequencies);
            
            Console.WriteLine("Frequency table saved to src/data/word_frequencies.json");
        }

        static async Task<Dictionary<string, int>> ProcessFileAsync(string filePath)
        {
            var wordFrequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var buffer = new char[8192]; // 8KB buffer for streaming
            var wordBuilder = new StringBuilder();
            
            // Regex to match word characters (letters, digits, apostrophes)
            var wordRegex = new Regex(@"\b[a-zA-Z']+\b", RegexOptions.Compiled);

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 65536);
            using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 65536);

            string? line;
            long lineNumber = 0;
            
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;
                
                // Process line in chunks to handle very long lines
                var matches = wordRegex.Matches(line);
                
                foreach (Match match in matches)
                {
                    string word = match.Value.ToLowerInvariant();
                    
                    // Skip very short words (1-2 characters) and very long words (likely errors)
                    if (word.Length >= 3 && word.Length <= 50)
                    {
                        // Remove leading/trailing apostrophes
                        word = word.Trim('\'');
                        
                        if (!string.IsNullOrEmpty(word))
                        {
                            wordFrequencies.TryGetValue(word, out int count);
                            wordFrequencies[word] = count + 1;
                        }
                    }
                }

                // Progress indicator for large files
                if (lineNumber % 10000 == 0)
                {
                    Console.WriteLine($"Processed {lineNumber:N0} lines, found {wordFrequencies.Count:N0} unique words so far...");
                }
            }

            Console.WriteLine($"Completed processing {lineNumber:N0} lines");
            return wordFrequencies;
        }

        static async Task SaveFrequencyTableAsync(Dictionary<string, int> wordFrequencies)
        {
            // Create data directory if it doesn't exist
            var dataDir = Path.Combine("src", "data");
            Directory.CreateDirectory(dataDir);

            var outputPath = Path.Combine(dataDir, "word_frequencies.json");
            
            // Sort by frequency (descending) then by word (ascending)
            var sortedFrequencies = wordFrequencies
                .OrderByDescending(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Calculate total word count
            int totalWords = wordFrequencies.Values.Sum();

            // Create the output object with total and frequency data
            var outputData = new
            {
                total = totalWords,
                frequency = sortedFrequencies
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(outputData, jsonOptions);
            await File.WriteAllTextAsync(outputPath, json, Encoding.UTF8);
        }
    }
}
