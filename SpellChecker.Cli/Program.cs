using SpellChecker.Core;
using System.Diagnostics;

namespace SpellChecker.Cli;

public static class Program
{
    public static void Main(string[] args)
    {
        // Check for benchmark mode
        if (args.Length > 0 && args[0].ToLower() == "--benchmark")
        {
            RunBenchmarks();
            return;
        }

        // Check for autocomplete mode
        if (args.Length > 0 && args[0].ToLower() == "--autocomplete")
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: --autocomplete <prefix>");
                return;
            }
            RunAutocompleteMode(args[1]);
            return;
        }

        // Normal spell checking mode
        RunNormalMode();
    }

    private static void RunNormalMode()
    {
        // Create configuration with custom settings
        var config = new SpellingServiceConfig(
            maxSpellingSuggestions: 5,    // Show only 5 spelling suggestions
            maxAutocompleteSuggestions: 8  // Show only 8 autocomplete suggestions
        );
        
        var spellingService = new SpellingService(config);
        spellingService.Initialize().Wait();
        if (!spellingService.IsInitialized())
        {
            Console.WriteLine("Spelling service is not initialized.");
            return;
        }

        // Check if input is being piped in
        string text;
        if (Console.IsInputRedirected)
        {
            // Read from piped input
            text = Console.In.ReadToEnd();
            if (string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine("No input provided via pipe.");
                return;
            }
        }
        else
        {
            // Use default text if no pipe
            text = "I went to the libllary to borow a interesting buk.";
        }
        
        Console.WriteLine($"Input text: {text}");
        var spellingResults = spellingService.CheckSpelling(text);
        
        if (spellingResults.Count == 0)
        {
            Console.WriteLine("No spelling errors found!");
        }
        else
        {
            Console.WriteLine("\nSpelling suggestions:");
            foreach (var result in spellingResults)
            {
                Console.WriteLine($"{result.Key}: {string.Join(", ", result.Value)}");
            }
        }
    }

    private static void RunAutocompleteMode(string prefix)
    {
        // Create configuration for autocomplete
        var config = new SpellingServiceConfig(
            maxSpellingSuggestions: 5,
            maxAutocompleteSuggestions: 10
        );
        
        var spellingService = new SpellingService(config);
        spellingService.Initialize().Wait();
        
        if (!spellingService.IsInitialized())
        {
            Console.WriteLine("Spelling service is not initialized.");
            return;
        }

        Console.WriteLine($"Autocomplete suggestions for '{prefix}':");
        var autocompleteResults = spellingService.GetAutocomplete(prefix);
        
        if (autocompleteResults.Count == 0)
        {
            Console.WriteLine("No autocomplete suggestions found.");
        }
        else
        {
            Console.WriteLine(string.Join(", ", autocompleteResults));
        }
    }

    private static void RunBenchmarks()
    {
        Console.WriteLine("Initializing spelling service for benchmarks...");
        
        // Create configuration for benchmarks
        var config = new SpellingServiceConfig(
            maxSpellingSuggestions: 10,
            maxAutocompleteSuggestions: 10
        );
        
        var spellingService = new SpellingService(config);
        spellingService.Initialize().Wait();
        
        if (!spellingService.IsInitialized())
        {
            Console.WriteLine("Spelling service is not initialized. Cannot run benchmarks.");
            return;
        }

        // Get the SpellCheck instance for direct algorithm testing
        var spellCheck = GetSpellCheckInstance(spellingService);
        if (spellCheck == null)
        {
            Console.WriteLine("Could not access SpellCheck instance for benchmarking.");
            return;
        }

        Console.WriteLine("Running benchmarks...\n");

        // Test data sets
        var smallSet = new[]
        {
            "recieve", "adress", "definately", "seperate", "acommodate", "enviroment",
            "occured", "publically", "goverment", "untill", "acquaintence", "refered",
            "wierd", "writting", "acheive", "collegue", "embarass", "greatful"
        };

        var largeSet = GenerateLargeTestSet();

        // Run benchmarks
        RunBenchmarkSet("Small Set", smallSet, spellCheck);
        RunBenchmarkSet("Large Set", largeSet, spellCheck);
    }

    private static void RunBenchmarkSet(string setName, string[] inputs, SpellCheck spellCheck)
    {
        Console.WriteLine($"=== {setName} ({inputs.Length} inputs) ===");

        // Warmup
        Console.WriteLine("Warming up...");
        foreach (var word in inputs)
        {
            _ = spellCheck.FindSpellingSuggestion(word);
            _ = spellCheck.FindSpellingSuggestionWithWagnerFischer(word);
        }

        var stopwatch = new Stopwatch();

        // Test Ukkonen algorithm
        stopwatch.Restart();
        foreach (var word in inputs)
        {
            _ = spellCheck.FindSpellingSuggestion(word);
        }
        stopwatch.Stop();
        var ukkonenMs = stopwatch.Elapsed.TotalMilliseconds;

        // Test Wagner-Fischer algorithm
        stopwatch.Restart();
        foreach (var word in inputs)
        {
            _ = spellCheck.FindSpellingSuggestionWithWagnerFischer(word);
        }
        stopwatch.Stop();
        var wagnerMs = stopwatch.Elapsed.TotalMilliseconds;

        // Test Q-Gram algorithm (hybrid)
        stopwatch.Restart();
        foreach (var word in inputs)
        {
            _ = spellCheck.FindSpellingSuggestionWithQGram(word);
        }
        stopwatch.Stop();
        var qgramMs = stopwatch.Elapsed.TotalMilliseconds;

        // Calculate speedups
        var ukkonenSpeedup = wagnerMs / ukkonenMs;
        var qgramSpeedup = wagnerMs / qgramMs;

        Console.WriteLine($"Ukkonen Algorithm:     {ukkonenMs:F2} ms");
        Console.WriteLine($"Wagner-Fischer:        {wagnerMs:F2} ms");
        Console.WriteLine($"Q-Gram (Hybrid):       {qgramMs:F2} ms");
        Console.WriteLine();
        Console.WriteLine($"Speedups vs Wagner-Fischer:");
        Console.WriteLine($"  Ukkonen:             {ukkonenSpeedup:F2}x faster");
        Console.WriteLine($"  Q-Gram (Hybrid):     {qgramSpeedup:F2}x faster");
        Console.WriteLine();
        Console.WriteLine($"Average per word:");
        Console.WriteLine($"  Ukkonen:             {ukkonenMs / inputs.Length:F3} ms");
        Console.WriteLine($"  Wagner-Fischer:      {wagnerMs / inputs.Length:F3} ms");
        Console.WriteLine($"  Q-Gram (Hybrid):     {qgramMs / inputs.Length:F3} ms");
        Console.WriteLine();
    }

    private static string[] GenerateLargeTestSet()
    {
        var seeds = new[]
        {
            "accommodation", "acknowledgment", "argument", "committed", "consensus",
            "dedicated", "discipline", "exception", "harassment", "indispensable",
            "independent", "maintenance", "millennium", "necessary", "occurrence",
            "possession", "privilege", "publicly", "recommend", "separate",
            "successful", "tendency", "tomorrow", "unfortunately", "until"
        };

        var inputs = new List<string>();
        foreach (var seed in seeds)
        {
            inputs.Add(seed);
            if (seed.Length > 2) inputs.Add(seed.Remove(1, 1)); // drop one character
            if (seed.Length > 4) inputs.Add(seed.Insert(2, "e")); // insert extra char
            if (seed.Length > 3) inputs.Add(seed[..2] + seed[3] + seed[2] + seed[4..]); // swap letters
        }

        return inputs.ToArray();
    }

    private static SpellCheck? GetSpellCheckInstance(SpellingService spellingService)
    {
        // Use reflection to access the private _spellCheck field
        var field = typeof(SpellingService).GetField("_spellCheck", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        return field?.GetValue(spellingService) as SpellCheck;
    }
}