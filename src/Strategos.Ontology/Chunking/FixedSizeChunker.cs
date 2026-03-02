using static Strategos.Ontology.Chunking.ChunkingUtilities;

namespace Strategos.Ontology.Chunking;

/// <summary>
/// Splits text into fixed-size chunks based on approximate token count, splitting at word boundaries.
/// Token estimation uses the heuristic: tokens = wordCount * 0.75.
/// </summary>
public sealed class FixedSizeChunker : ITextChunker
{
    /// <inheritdoc />
    public IReadOnlyList<TextChunk> Chunk(string text, ChunkOptions? options = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        options ??= new ChunkOptions();

        var maxWords = (int)(options.MaxTokens / TokensPerWord);
        var overlapWords = (int)(options.OverlapTokens / TokensPerWord);

        var words = SplitWordSpans(text);

        if (words.Count <= maxWords)
        {
            return [new TextChunk(text, 0, 0, text.Length)];
        }

        var chunks = new List<TextChunk>();
        var wordIndex = 0;
        var chunkIndex = 0;

        while (wordIndex < words.Count)
        {
            var endWordIndex = Math.Min(wordIndex + maxWords, words.Count);

            var startOffset = words[wordIndex].Offset;
            var lastWord = words[endWordIndex - 1];
            var endOffset = lastWord.Offset + lastWord.Length;

            var content = text[startOffset..endOffset];
            chunks.Add(new TextChunk(content, chunkIndex, startOffset, endOffset));

            chunkIndex++;

            var advance = maxWords - overlapWords;
            if (advance <= 0)
            {
                advance = 1;
            }

            wordIndex += advance;
        }

        return chunks;
    }
}
