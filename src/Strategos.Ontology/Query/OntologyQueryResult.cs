namespace Strategos.Ontology.Query;

/// <summary>
/// Result of querying ontology metadata, holding discovered object types.
/// </summary>
public sealed record OntologyQueryResult(IReadOnlyList<string> ObjectTypes);
