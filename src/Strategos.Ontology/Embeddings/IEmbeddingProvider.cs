namespace Strategos.Ontology.Embeddings;

/// <summary>
/// Provider abstraction for generating embedding vectors from text.
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// Gets the dimensionality of the embedding vectors produced by this provider.
    /// </summary>
    int Dimensions { get; }

    /// <summary>
    /// Generates an embedding vector for the given text.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A float array representing the embedding vector.</returns>
    Task<float[]> EmbedAsync(string text, CancellationToken ct = default);

    /// <summary>
    /// Generates embedding vectors for multiple texts in a single batch call.
    /// </summary>
    /// <param name="texts">The texts to embed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of float arrays, one per input text, in the same order.</returns>
    Task<IReadOnlyList<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default);
}
