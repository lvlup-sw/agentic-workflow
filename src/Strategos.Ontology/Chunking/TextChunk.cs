namespace Strategos.Ontology.Chunking;

/// <summary>
/// Represents a single chunk of text extracted by an <see cref="ITextChunker"/>.
/// </summary>
/// <param name="Content">The text content of the chunk.</param>
/// <param name="Index">The zero-based index of this chunk within the source text.</param>
/// <param name="StartOffset">The character offset in the source text where this chunk begins.</param>
/// <param name="EndOffset">The character offset in the source text where this chunk ends (exclusive).</param>
public readonly record struct TextChunk(string Content, int Index, int StartOffset, int EndOffset);
