namespace Strategos.Ontology.Chunking;

/// <summary>
/// Configuration options for text chunking.
/// </summary>
public sealed record ChunkOptions
{
    /// <summary>
    /// Gets the maximum number of tokens per chunk. Defaults to 512.
    /// </summary>
    public int MaxTokens { get; init; } = 512;

    /// <summary>
    /// Gets the number of overlapping tokens between adjacent chunks. Defaults to 64.
    /// </summary>
    public int OverlapTokens { get; init; } = 64;
}
