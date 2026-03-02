namespace Strategos.Ontology.Chunking;

/// <summary>
/// Splits text into smaller chunks suitable for embedding.
/// </summary>
public interface ITextChunker
{
    /// <summary>
    /// Splits the given text into chunks.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    /// <param name="options">Optional chunking options. If null, implementation-specific defaults are used.</param>
    /// <returns>An ordered list of text chunks.</returns>
    IReadOnlyList<TextChunk> Chunk(string text, ChunkOptions? options = null);
}
