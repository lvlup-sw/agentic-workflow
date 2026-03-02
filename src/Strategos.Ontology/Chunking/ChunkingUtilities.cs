namespace Strategos.Ontology.Chunking;

/// <summary>
/// Shared utilities for text chunking operations.
/// </summary>
internal static class ChunkingUtilities
{
    /// <summary>
    /// Heuristic token-to-word ratio: 1 token ~ 0.75 words.
    /// </summary>
    internal const double TokensPerWord = 0.75;

    /// <summary>
    /// Converts MaxTokens to a max word count, validating that the value is positive.
    /// </summary>
    internal static int TokensToMaxWords(int maxTokens)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxTokens, 1);
        return (int)(maxTokens / TokensPerWord);
    }

    /// <summary>
    /// Counts the number of whitespace-delimited words in the given text.
    /// </summary>
    internal static int CountWords(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var count = 0;
        var inWord = false;

        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                inWord = false;
            }
            else if (!inWord)
            {
                inWord = true;
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Splits text into word spans (offset + length) at whitespace boundaries.
    /// </summary>
    internal static List<WordSpan> SplitWordSpans(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var words = new List<WordSpan>();
        var i = 0;

        while (i < text.Length)
        {
            while (i < text.Length && char.IsWhiteSpace(text[i]))
            {
                i++;
            }

            if (i >= text.Length)
            {
                break;
            }

            var start = i;

            while (i < text.Length && !char.IsWhiteSpace(text[i]))
            {
                i++;
            }

            words.Add(new WordSpan(start, i - start));
        }

        return words;
    }

    /// <summary>
    /// Represents a word's position within a text string.
    /// </summary>
    internal readonly record struct WordSpan(int Offset, int Length);
}
