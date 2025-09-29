namespace SpellChecker.Core;

public class SpellCheck(Dictionary<string, long> wordDictionary)
{
   public bool DoesWordExist(string word)
   {
      return  wordDictionary.ContainsKey(word);
   }

   private long _getWordCount(string word)
   {
      return DoesWordExist(word) ? wordDictionary[word] : throw new Exception("Word not found");
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

      var possibleWords = _findAllPossibleWordsWithOneOrTwoEdits(word);

      return possibleWords
         .OrderByDescending(tuple => tuple.Item2)
         .Take(8)
         .Select(tuple => tuple.Item1)
         .ToList();
   }
  
   private List<Tuple<string, long>> _findAllPossibleWordsWithOneOrTwoEdits(string word)
   {
      var results = new List<Tuple<string, long>>();
      
      foreach (var possibleWord in wordDictionary.Keys)
      {
         if (CalculateLevenshteinDistance(word, possibleWord) <= 2)
         {
            results.Add(Tuple.Create(possibleWord, wordDictionary[possibleWord]));
         }
      }

      return results;
   }

   private static int CalculateLevenshteinDistance(string source, string target)
   {
      if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
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
}