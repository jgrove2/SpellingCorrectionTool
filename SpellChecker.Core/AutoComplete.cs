namespace SpellChecker.Core;

public class AutoComplete
{
    private readonly Trie _possibleWords;
    private readonly int _maxSuggestions;
    
    public AutoComplete(Trie possibleWords, int maxSuggestions = 10)
    {
        _possibleWords = possibleWords;
        _maxSuggestions = maxSuggestions;
    }
    
    public List<string> GetAutoCompleteOptions(string prefix)
    {
        return _possibleWords.GetWordsWithPrefix(prefix)
            .OrderByDescending(tuple => tuple.Item2)
            .Take(_maxSuggestions)
            .Select(tuple => tuple.Item1).ToList();
    }
}