namespace Strategos.Ontology.Events;

/// <summary>
/// Describes criteria for querying ontology events.
/// </summary>
public sealed record EventQuery(
    string Domain,
    string ObjectTypeName,
    string? ObjectId = null,
    DateTimeOffset? Since = null,
    IReadOnlyList<string>? EventTypes = null);
