namespace Strategos.Ontology.Chunking;

/// <summary>
/// Represents a chunk of text extracted from a larger document.
/// </summary>
/// <param name="Content">The text content of the chunk.</param>
/// <param name="Index">The sequential index of this chunk (0-based).</param>
/// <param name="StartOffset">The starting character position in the source text.</param>
/// <param name="EndOffset">The ending character position (exclusive) in the source text.</param>
public readonly record struct TextChunk(string Content, int Index, int StartOffset, int EndOffset);
