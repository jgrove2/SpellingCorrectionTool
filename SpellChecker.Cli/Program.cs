using SpellChecker.Core;

namespace SpellChecker.Cli;

public static class Program
{
    public static void Main(string[] args)
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
        var text = "I went to the libllary to borow a interesting buk.";
        
        Console.WriteLine(text);
        var spellingResults = spellingService.CheckSpelling(text);
        foreach (var result in spellingResults)
        {
            Console.WriteLine($"{result.Key}: {string.Join(", ", result.Value)}");
        }
        var autocompleteResults = spellingService.GetAutocomplete("lib");
        Console.WriteLine($"Autocomplete results for 'lib': {string.Join(", ", autocompleteResults)}");
    }
}