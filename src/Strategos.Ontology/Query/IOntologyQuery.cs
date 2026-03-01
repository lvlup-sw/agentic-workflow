namespace Strategos.Ontology.Query;

/// <summary>
/// Schema-level query interface for agents exploring the ontology graph.
/// Provides discovery of object types, links, actions, and events across domains.
/// </summary>
public interface IOntologyQuery
{
    /// <summary>
    /// Queries the ontology schema and returns metadata about available object types.
    /// </summary>
    Task<OntologyQueryResult> QueryAsync(string domain, CancellationToken ct = default);
}
