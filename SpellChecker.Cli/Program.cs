using SpellChecker.Core;

namespace SpellChecker.Cli;

public static class Program
{
    public static void Main(string[] args)
    {
        var spellingService = new SpellingService();
        spellingService.Initialize().Wait();
        if (!spellingService.IsInitialized())
        {
            Console.WriteLine("Spelling service is not initialized.");
            return;
        }
        var text = "I went to the libllary to borow a interesting buk.";
        
        Console.WriteLine("=== Spelling Suggestions (1 or 2 edits away) ===");
        var spellingResults = spellingService.CheckSpelling(text);
        foreach (var result in spellingResults)
        {
            Console.WriteLine($"{result.Key}: {string.Join(", ", result.Value)}");
        }
    }
}