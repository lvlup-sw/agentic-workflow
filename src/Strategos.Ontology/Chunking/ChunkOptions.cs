namespace Strategos.Ontology.Chunking;

/// <summary>
/// Options controlling how text is split into chunks.
/// </summary>
public sealed record ChunkOptions
{
    /// <summary>
    /// Gets the maximum number of tokens per chunk. Default is 512.
    /// </summary>
    public int MaxTokens { get; init; } = 512;

    /// <summary>
    /// Gets the number of overlap tokens between consecutive chunks. Default is 64.
    /// </summary>
    public int OverlapTokens { get; init; } = 64;
}
