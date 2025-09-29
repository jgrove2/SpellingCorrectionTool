namespace SpellChecker.Core
{
    public class Trie
    {
        private TrieNode _root = new(){IsEndOfWord = false};

        public TrieNode GetRootNode()
        {
            return _root;
        }

        public void AddString(string word, long count)
        {
            var currentNode = _root;
            foreach(var c in word)
            {
                if(!currentNode.Children.TryGetValue(c, out var child))
                {
                    child = new TrieNode(){IsEndOfWord = false};
                    currentNode.Children.Add(c, child);
                }
                currentNode = child;
            }
            currentNode.IsEndOfWord = true;
            currentNode.Count = count;
        }

        public List<Tuple<string, long>> GetWordsWithPrefix(string prefix)
        {
            var currentNode = _root;
            foreach(var c in prefix) {
                if(!currentNode.Children.TryGetValue(c, out var child)) return [];
                currentNode = child;
            }
            var words = new List<Tuple<string, long>>();

            void Dfs(TrieNode node, string currentWord)
            {
                if (node.IsEndOfWord) words.Add(new Tuple<string, long>(currentWord, node.Count));
                foreach(var child in node.Children)
                {
                    Dfs(child.Value, currentWord + child.Key);
                }
            }
            Dfs(currentNode, prefix);
            return words;
        }
    }
}