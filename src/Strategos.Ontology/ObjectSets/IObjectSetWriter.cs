namespace Strategos.Ontology.ObjectSets;

/// <summary>
/// Provider abstraction for storing objects in an object set backend.
/// </summary>
public interface IObjectSetWriter
{
    /// <summary>
    /// Stores a single item in the backend.
    /// </summary>
    /// <typeparam name="T">The domain object type to store.</typeparam>
    /// <param name="item">The item to store.</param>
    /// <param name="ct">Cancellation token.</param>
    Task StoreAsync<T>(T item, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Stores a batch of items in the backend.
    /// </summary>
    /// <typeparam name="T">The domain object type to store.</typeparam>
    /// <param name="items">The items to store.</param>
    /// <param name="ct">Cancellation token.</param>
    Task StoreBatchAsync<T>(IReadOnlyList<T> items, CancellationToken ct = default) where T : class;
}
