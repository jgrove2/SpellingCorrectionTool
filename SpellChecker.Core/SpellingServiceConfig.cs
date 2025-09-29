namespace SpellChecker.Core;

public class SpellingServiceConfig
{
    public int MaxSpellingSuggestions { get; set; } = 8;
    public int MaxAutocompleteSuggestions { get; set; } = 10;
    
    public SpellingServiceConfig()
    {
    }
    
    public SpellingServiceConfig(int maxSpellingSuggestions, int maxAutocompleteSuggestions)
    {
        MaxSpellingSuggestions = maxSpellingSuggestions;
        MaxAutocompleteSuggestions = maxAutocompleteSuggestions;
    }
}
