namespace Strategos.Ontology.Ingestion;

/// <summary>
/// Abstraction for an ingestion pipeline that processes texts into domain objects.
/// Can be resolved from DI with pre-wired dependencies.
/// </summary>
/// <typeparam name="T">The target domain object type.</typeparam>
public interface IIngestionPipeline<T>
    where T : class
{
    /// <summary>
    /// Executes the ingestion pipeline over the provided texts.
    /// </summary>
    /// <param name="texts">The input texts to process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An <see cref="IngestionResult"/> describing what was processed.</returns>
    Task<IngestionResult> ExecuteAsync(IEnumerable<string> texts, CancellationToken ct = default);

    /// <summary>
    /// Executes the ingestion pipeline over the provided async text stream.
    /// </summary>
    /// <param name="texts">The async enumerable of input texts to process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An <see cref="IngestionResult"/> describing what was processed.</returns>
    Task<IngestionResult> ExecuteAsync(IAsyncEnumerable<string> texts, CancellationToken ct = default);
}
