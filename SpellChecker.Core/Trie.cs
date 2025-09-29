namespace SpellChecker.Core
{
    public class Trie
    {
        private readonly TrieNode _root;

        public Trie()
        {
            _root = new TrieNode();
        }

        /// <summary>
        /// Inserts a word into the trie.
        /// </summary>
        /// <param name="word">The word to insert</param>
        /// <param name="count">The count for the word</param>
        public void Insert(string word, long count) 
        {
            if (string.IsNullOrEmpty(word))
                return;

            TrieNode current = _root;
            foreach (char c in word.ToLowerInvariant()) 
            {
                if (!current.Children.ContainsKey(c)) 
                {
                    current.Children[c] = new TrieNode();
                }
                current = current.Children[c];
            }
            current.IsEndOfWord = true;
            current.Count = count;
        }
        
        /// <summary>
        /// Checks if a word exists in the trie.
        /// </summary>
        /// <param name="word">The word to check</param>
        /// <returns>-1 if the word does not exist, the count of the word if it exists</returns>
        public long ContainsWord(string word) 
        {
            if (string.IsNullOrEmpty(word))
                return -1;

            TrieNode current = _root;
            foreach (char c in word.ToLowerInvariant()) 
            {
                if (!current.Children.ContainsKey(c)) 
                {
                    return -1;
                }
                current = current.Children[c];
            }
            return current.Count;
        }
    }
}