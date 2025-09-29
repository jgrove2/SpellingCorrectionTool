namespace SpellChecker.Core;

public class SpellingService()
{
    private readonly ImportData? _importData = new();
    private SpellCheck _spellCheck = null!;

    public bool IsInitialized()
    {
        var importData = this._importData;
        return importData != null && importData.IsInitialized();
    }

    public async Task Initialize()
    {
        if (_importData != null)
        {
            await _importData.AsyncImportData();
            _spellCheck =  new SpellCheck(_importData.WordDictionary);
        }
    }

    public Dictionary<string, List<string>> CheckSpelling(string text)
    {
        if (!IsInitialized())
        {
            throw new Exception("Spelling service is not initialized.");
        }
        var spellCheckResults = new Dictionary<string, List<string>>();
        // Convert text to lowercase and split into words
        foreach (var rawWord in text.ToLowerInvariant().Split(' '))
        {
            // Remove special characters, keep only letters
            var word = new string(rawWord.Where(char.IsLetter).ToArray());
            if (string.IsNullOrEmpty(word) || _spellCheck.DoesWordExist(word) || spellCheckResults.ContainsKey(word))
                continue;
            // Only process words that contain at least one letter
            spellCheckResults.Add(word, _spellCheck.FindSpellingSuggestion(word));
        }
        return spellCheckResults;
    }

}