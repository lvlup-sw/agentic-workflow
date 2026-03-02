namespace Strategos.Ontology.Chunking;

/// <summary>
/// Splits text into chunks suitable for embedding and vector search.
/// </summary>
public interface ITextChunker
{
    /// <summary>
    /// Splits the provided text into chunks according to the specified options.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    /// <param name="options">Optional chunking options. If null, defaults are used.</param>
    /// <returns>A read-only list of text chunks.</returns>
    IReadOnlyList<TextChunk> Chunk(string text, ChunkOptions? options = null);
}
