namespace SpellChecker.Core;

public class Program
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
        var text = "I went to the libary to borow a interesting buk.";
        var spellingResults = spellingService.CheckSpelling(text);
        foreach (var result in spellingResults)
        {
            Console.WriteLine($"{result.Key}: {string.Join(", ", result.Value)}");
        }
    }
}