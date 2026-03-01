namespace Strategos.Ontology.Events;

/// <summary>
/// Represents a domain event emitted by the ontology.
/// </summary>
public sealed record OntologyEvent(
    string Domain,
    string ObjectType,
    string ObjectId,
    string EventType,
    DateTimeOffset Timestamp,
    object? Payload);
