namespace SpellChecker.Core;

public class QGramIndex
{
    private readonly int _q;
    private readonly Dictionary<string, HashSet<string>> _index;
    private readonly Dictionary<string, long> _wordFrequencies;

    public QGramIndex(int q = 2)
    {
        _q = q;
        _index = new Dictionary<string, HashSet<string>>();
        _wordFrequencies = new Dictionary<string, long>();
    }

    public void BuildIndex(Dictionary<string, long> wordDictionary)
    {
        foreach (var kvp in wordDictionary)
        {
            _wordFrequencies[kvp.Key] = kvp.Value;
            var qGrams = GetQGrams(kvp.Key);
            
            foreach (var qGram in qGrams)
            {
                if (!_index.ContainsKey(qGram))
                {
                    _index[qGram] = new HashSet<string>();
                }
                _index[qGram].Add(kvp.Key);
            }
        }
    }

    public List<string> GetCandidatesWithCommonGrams(string word, int minCommonGrams = 3)
    {
        var qGrams = GetQGrams(word);
        var candidateScores = new Dictionary<string, int>();
        
        // Count q-gram overlaps
        foreach (var qGram in qGrams)
        {
            if (!_index.TryGetValue(qGram, out var candidates)) continue;
            foreach (var candidate in candidates)
            {
                candidateScores.TryAdd(candidate, 0);
                candidateScores[candidate]++;
            }
        }

        // Filter to only candidates with at least 3 common q-grams
        return candidateScores
            .Where(kvp => kvp.Value >= minCommonGrams)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    private List<string> GetQGrams(string word)
    {
        var qGrams = new List<string>();
        var paddedWord = $"#{word}#";
        
        for (int i = 0; i <= paddedWord.Length - _q; i++)
        {
            qGrams.Add(paddedWord.Substring(i, _q));
        }
        
        return qGrams;
    }
}