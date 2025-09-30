namespace SpellChecker.Core.Test;

public class SpellCheckAlgorithmsTests
{
    [Test]
    public void WagnerFischer_ComputesExpectedDistances()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SpellCheck.CalculateLevenshteinDistanceWagnerFischer("kitten", "sitting"), Is.EqualTo(3));
            Assert.That(SpellCheck.CalculateLevenshteinDistanceWagnerFischer("flaw", "lawn"), Is.EqualTo(2));
            Assert.That(SpellCheck.CalculateLevenshteinDistanceWagnerFischer("intention", "execution"), Is.EqualTo(5));
            Assert.That(SpellCheck.CalculateLevenshteinDistanceWagnerFischer("", "abc"), Is.EqualTo(3));
            Assert.That(SpellCheck.CalculateLevenshteinDistanceWagnerFischer("abc", ""), Is.EqualTo(3));
            Assert.That(SpellCheck.CalculateLevenshteinDistanceWagnerFischer("same", "same"), Is.EqualTo(0));
        });
    }

    [Test]
    public void Ukkonen_ComputesDistancesWithinAndBeyondThreshold()
    {
        Assert.Multiple(() =>
        {
            // Within default maxDistance=2 should return actual distance <= 2
            Assert.That(SpellCheck.CalculateLevenshteinDistanceUkkonen("flaw", "lawn"), Is.EqualTo(2));
            Assert.That(SpellCheck.CalculateLevenshteinDistanceUkkonen("abc", "abd"), Is.EqualTo(1));

            // Beyond threshold should return maxDistance + 1 (3 by default)
            Assert.That(SpellCheck.CalculateLevenshteinDistanceUkkonen("kitten", "sitting"), Is.EqualTo(3));

            // Custom threshold
            Assert.That(SpellCheck.CalculateLevenshteinDistanceUkkonen("intention", "execution", maxDistance: 5), Is.EqualTo(5));
            Assert.That(SpellCheck.CalculateLevenshteinDistanceUkkonen("intention", "execution", maxDistance: 4), Is.EqualTo(5));
        });
    }

    [Test]
    public void DistanceAlgorithms_ReportGreaterThanTwoCorrectly()
    {
        // Expect >2 for these pairs
        var wf = SpellCheck.CalculateLevenshteinDistanceWagnerFischer("banana", "bandana");
        var uk = SpellCheck.CalculateLevenshteinDistanceUkkonen("banana", "bandana");
        Assert.Multiple(() =>
        {
            Assert.That(wf, Is.Not.GreaterThan(2));
            Assert.That(uk, Is.Not.GreaterThan(2));
        });
    }

    [Test]
    public void FindSpellingSuggestion_ReturnsEmptyIfWordExists_OrLengthZero()
    {
        var dict = new Dictionary<string, long>
        {
            {"apple", 100},
            {"apply", 80},
            {"apples", 50},
            {"banana", 70},
        };
        var sc = new SpellCheck(dict, maxSuggestions: 5);

        // Word exists -> no suggestions
        var existing = sc.FindSpellingSuggestion("apple");
        Assert.That(existing, Is.Empty);

        // Empty input -> no suggestions
        var empty = sc.FindSpellingSuggestion("");
        Assert.That(empty, Is.Empty);
    }
}