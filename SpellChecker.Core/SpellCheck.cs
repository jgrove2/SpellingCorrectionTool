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

      var possibleWords = _findAllPossibleWords(word);

      return possibleWords
         .OrderByDescending(tuple => tuple.Item2)
         .Take(8)
         .Select(tuple => tuple.Item1)
         .ToList();
   }

   private List<Tuple<string, long>> _findAllPossibleWords(string word)
   {
      var results = new List<Tuple<string, long>>();
      var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      // Remove one character at each position
      for (var i = 0; i < word.Length; i++)
      {
         var removed = word.Remove(i, 1);
         if (removed.Length > 0 && DoesWordExist(removed) && seen.Add(removed))
         {
            results.Add(Tuple.Create(removed, _getWordCount(removed)));
         }
      }

      // Add one character (a-z) at each position
      for (var i = 0; i <= word.Length; i++)
      {
         for (var c = 'a'; c <= 'z'; c++)
         {
            var added = word.Insert(i, c.ToString());
            if (DoesWordExist(added) && seen.Add(added))
            {
               results.Add(Tuple.Create(added, _getWordCount(added)));
            }
         }
      }
      // Replace each character with (a-z)
      for (var i = 0; i < word.Length; i++)
      {
         foreach (var c in Enumerable.Range('a', 26).Select(x => (char)x))
         {
            if (word[i] == c) continue; // skip if same letter
            var replaced = word[..i] + c + word[(i + 1)..];
            if (DoesWordExist(replaced) && seen.Add(replaced))
            {
               results.Add(Tuple.Create(replaced, _getWordCount(replaced)));
            }
         }
      }

      return results;
   }
}