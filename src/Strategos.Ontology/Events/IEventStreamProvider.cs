namespace Strategos.Ontology.Events;

/// <summary>
/// Provider for querying ontology event streams.
/// </summary>
public interface IEventStreamProvider
{
    /// <summary>
    /// Queries events matching the specified criteria.
    /// </summary>
    IAsyncEnumerable<OntologyEvent> QueryEventsAsync(EventQuery query, CancellationToken ct = default);
}
