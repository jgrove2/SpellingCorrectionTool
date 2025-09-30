namespace SpellChecker.Core;

public class QGramIndex(int q = 2)
{
    private readonly Dictionary<int, HashSet<int>> _index = new(); // Hash codes instead of strings
    private readonly Dictionary<string, long> _wordFrequencies = new();
    private readonly List<string> _words = new(); // Word list for index lookup
    private readonly Dictionary<string, int> _wordToIndex = new(); // Word to index mapping

    public void BuildIndex(Dictionary<string, long> wordDictionary)
    {
        // First pass: build word index
        foreach (var kvp in wordDictionary)
        {
            _wordFrequencies[kvp.Key] = kvp.Value;
            _wordToIndex[kvp.Key] = _words.Count;
            _words.Add(kvp.Key);
        }

        // Second pass: build q-gram index with hash codes
        for (int wordIndex = 0; wordIndex < _words.Count; wordIndex++)
        {
            var word = _words[wordIndex];
            var qGramHashes = GetQGramHashes(word);
            
            foreach (var qGramHash in qGramHashes)
            {
                if (!_index.ContainsKey(qGramHash))
                {
                    _index[qGramHash] = new HashSet<int>();
                }
                _index[qGramHash].Add(wordIndex);
            }
        }
    }

    public List<string> GetCandidatesWithCommonGrams(string word, double minCommonRatio, int maxDistance = 2)
    {
        var qGramHashes = GetQGramHashes(word);
        var candidateScores = new Dictionary<int, int>(); // Use word indices instead of strings
        var totalQGrams = qGramHashes.Count;
        var minCommonGrams = Math.Max(1, (totalQGrams * minCommonRatio));
        
        // Count q-gram overlaps using hash codes
        foreach (var qGramHash in qGramHashes)
        {
            if (!_index.TryGetValue(qGramHash, out var wordIndices)) continue;
            foreach (var wordIndex in wordIndices)
            {
                if(Math.Abs(_words[wordIndex].Length - word.Length) > maxDistance) continue;
                if(candidateScores.GetValueOrDefault(wordIndex, 0) >= minCommonGrams) break;
                candidateScores.TryAdd(wordIndex, 0);
                candidateScores[wordIndex]++;
            }
        }

        // Filter to only candidates with at least minCommonGrams common q-grams
        return candidateScores
            .Where(kvp => kvp.Value >= minCommonGrams)
            .Select(kvp => _words[kvp.Key])
            .ToList();
    }

    private List<int> GetQGramHashes(string word)
    {
        var qGramHashes = new List<int>();
        var paddedWord = $"#{word}#";
        
        for (var i = 0; i <= paddedWord.Length - q; i++)
        {
            // Use Span<char> to avoid string allocations
            var qGramSpan = paddedWord.AsSpan(i, q);
            qGramHashes.Add(string.GetHashCode(qGramSpan));
        }
        
        return qGramHashes;
    }

}