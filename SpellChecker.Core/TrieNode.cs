namespace SpellChecker.Core
{
    public class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; } = new ();
        public bool IsEndOfWord { get; set; } = false;
        public long Count { get; set; } = 0;
    }
}