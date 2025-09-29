using System.Globalization;
using System.Reflection;

namespace SpellChecker.Core;

public class ImportData
{
    private readonly string _csvPath = "/home/jgrove/Projects/SpellingCorrectionTool/SpellChecker.Core/data/unigram_freq.csv";

    public readonly Dictionary<string, long> WordDictionary = new();

    private bool _isInitialized = false;
    
    public async Task AsyncImportData()
    {
        try
        {
            if (!File.Exists(this._csvPath))
            {
                Console.WriteLine($"Error: File '{this._csvPath}' not found.");
                Console.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");
                Console.WriteLine($"Assembly location: {Assembly.GetExecutingAssembly().Location}");
                return;
            }

            using var reader = new StreamReader(this._csvPath);

            // Skip header line
            await reader.ReadLineAsync();

            while (await reader.ReadLineAsync() is { } line)
            {
                var parts = line.Split(',');
                if (parts.Length < 2) continue;
                var word = parts[0].Trim().ToLowerInvariant();

                if (!long.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out var count)) continue;
                if(!this.WordDictionary.TryAdd(word, count)) continue;
            }

            reader.Close();
            this._isInitialized = true;
            Console.WriteLine($"Data imported successfully");
            Console.WriteLine($"Word dictionary size: {this.WordDictionary.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file '{this._csvPath}': {ex.Message}");
            Console.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"Assembly location: {Assembly.GetExecutingAssembly().Location}");
        }
    }

    public bool IsInitialized()
    {
        return this._isInitialized;
    }
}