using System.Collections.Concurrent;

namespace SpellChecker.Core;

public class SpellCheck
{
    private readonly Dictionary<string, long> _wordDictionary;
    private readonly int _maxSuggestions;
    private readonly int _maxDistance = 2;
    private readonly QGramIndex _qGramIndex;
    
    public SpellCheck(Dictionary<string, long> wordDictionary, int maxSuggestions = 8)
    {
        _wordDictionary = wordDictionary;
        _maxSuggestions = maxSuggestions;
        _qGramIndex = new QGramIndex(q: 2);
        _qGramIndex.BuildIndex(wordDictionary);
    }
   public bool DoesWordExist(string word)
   {
      return  _wordDictionary.ContainsKey(word);
   }


   public List<string> FindSpellingSuggestion(string word)
   {
      if (DoesWordExist(word))
      {
         return [];
      }

      if (word.Length <= 0)
      {
         return [];
      }

      var possibleWords = _findAllPossibleWordsWithUkkonenAndParallel(word);

      return possibleWords
         .OrderByDescending(tuple => tuple.Item2)
         .Take(_maxSuggestions)
         .Select(tuple => tuple.Item1)
         .ToList();
   }
   public List<string> FindSpellingSuggestionWithWagnerFischer(string word)
   {
      if (DoesWordExist(word))
      {
         return [];
      }

      if (word.Length <= 0)
      {
         return [];
      }

      var possibleWords = _findAllPossibleWordsWithWagnerFischerAndParallel(word);

      return possibleWords
         .OrderByDescending(tuple => tuple.Item2)
         .Take(_maxSuggestions)
         .Select(tuple => tuple.Item1)
         .ToList();
   }

   public List<string> FindSpellingSuggestionWithQGram(string word)
   {
      if (DoesWordExist(word))
      {
         return [];
      }

      if (word.Length <= 0)
      {
         return [];
      }

      // Get top 100 candidates using Q-gram pre-filtering
      var candidates = _qGramIndex.GetCandidatesWithCommonGrams(word, minCommonGrams: 3);
      
      // Apply Ukkonen's algorithm only to the filtered candidates
      var results = new List<Tuple<string, long>>();
      
      foreach (var candidate in candidates)
      {
         var editDistance = CalculateLevenshteinDistanceUkkonen(word, candidate, _maxDistance);
         if (editDistance <= _maxDistance)
         {
            results.Add(Tuple.Create(candidate, _wordDictionary[candidate]));
         }
      }

      return results
         .OrderByDescending(tuple => tuple.Item2) // Frequency
         .Take(_maxSuggestions)
         .Select(tuple => tuple.Item1)
         .ToList();
   }

   private List<Tuple<string, long>> _findAllPossibleWordsWithUkkonenAndParallel(string word)
   {
      var results = new ConcurrentBag<Tuple<string, long>>();

      var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 8 };
      Parallel.ForEach(_wordDictionary.Keys, parallelOptions, possibleWord =>
      {
         if (CalculateLevenshteinDistanceUkkonen(word, possibleWord, maxDistance: _maxDistance) <= 2)
         {
            results.Add(Tuple.Create(possibleWord, _wordDictionary[possibleWord]));
         }
      });

      return results.ToList();
   }
   private List<Tuple<string, long>> _findAllPossibleWordsWithWagnerFischerAndParallel(string word)
   {
      var results = new ConcurrentBag<Tuple<string, long>>();

      var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 8 };
      Parallel.ForEach(_wordDictionary.Keys, parallelOptions, possibleWord =>
      {
         if (CalculateLevenshteinDistanceWagnerFischer(word, possibleWord) <= 2)
         {
            results.Add(Tuple.Create(possibleWord, _wordDictionary[possibleWord]));
         }
      });

      return results.ToList();
   }

   // Wagner-Fischer dynamic programming approach to Levenshtein distance
   public static int CalculateLevenshteinDistanceWagnerFischer(string source, string target)
   {
      if (string.IsNullOrEmpty(source)) return target.Length;
      if (string.IsNullOrEmpty(target)) return source.Length;

      var distance = new int[source.Length + 1, target.Length + 1];

      // Initialize first row and column
      for (var i = 0; i <= source.Length; i++)
         distance[i, 0] = i;
      for (var j = 0; j <= target.Length; j++)
         distance[0, j] = j;

      // Fill the distance matrix
      for (var i = 1; i <= source.Length; i++)
      {
         for (var j = 1; j <= target.Length; j++)
         {
            var cost = source[i - 1] == target[j - 1] ? 0 : 1;
            distance[i, j] = Math.Min(
               Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
               distance[i - 1, j - 1] + cost);
         }
      }

      return distance[source.Length, target.Length];
   }

   // Ukkonen's algorithm for Levenshtein distance with a default maxDistance of 2
   public static int CalculateLevenshteinDistanceUkkonen(string source, string target, int maxDistance = 2)
   {
      if (string.IsNullOrEmpty(source)) return target.Length;
      if (string.IsNullOrEmpty(target)) return source.Length;

      int m = source.Length;
      int n = target.Length;

      // If the length difference is greater than maxDistance, return a value greater than maxDistance
      if (Math.Abs(m - n) > maxDistance)
         return maxDistance + 1;

      // Only keep two rows in memory
      int[] prevRow = new int[n + 1];
      int[] currRow = new int[n + 1];

      for (int j = 0; j <= n; j++)
         prevRow[j] = j;

      for (int i = 1; i <= m; i++)
      {
         currRow[0] = i;

         // Calculate the band boundaries
         int from = Math.Max(1, i - maxDistance);
         int to = Math.Min(n, i + maxDistance);

         // If the band does not cover the first column, set it to maxDistance+1
         if (from > 1)
            currRow[from - 1] = maxDistance + 1;

         bool rowExceeded = true;

         for (int j = from; j <= to; j++)
         {
            int cost = source[i - 1] == target[j - 1] ? 0 : 1;
            currRow[j] = Math.Min(
               Math.Min(prevRow[j] + 1, currRow[j - 1] + 1),
               prevRow[j - 1] + cost
            );
            if (currRow[j] <= maxDistance)
            {
               rowExceeded = false;
            }
         }

         // If the band does not cover the last column, set it to maxDistance+1
         if (to < n)
            currRow[to + 1] = maxDistance + 1;

         // If all values in this row are greater than maxDistance, break early
         if (rowExceeded)
            return maxDistance + 1;

         // Swap rows
         var temp = prevRow;
         prevRow = currRow;
         currRow = temp;
      }

      return prevRow[n] <= maxDistance ? prevRow[n] : maxDistance + 1;
   }
}