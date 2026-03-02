namespace Strategos.Ontology.ObjectSets;

/// <summary>
/// Write abstraction for storing items into an object set backend.
/// </summary>
public interface IObjectSetWriter
{
    /// <summary>
    /// Stores a single item.
    /// </summary>
    /// <typeparam name="T">The domain object type.</typeparam>
    /// <param name="item">The item to store.</param>
    /// <param name="ct">Cancellation token.</param>
    Task StoreAsync<T>(T item, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Stores multiple items in a batch.
    /// </summary>
    /// <typeparam name="T">The domain object type.</typeparam>
    /// <param name="items">The items to store.</param>
    /// <param name="ct">Cancellation token.</param>
    Task StoreBatchAsync<T>(IReadOnlyList<T> items, CancellationToken ct = default) where T : class;
}
