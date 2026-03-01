namespace Strategos.Ontology.Descriptors;

public sealed record EventDescriptor(
    Type EventType,
    string Description)
{
    public EventSeverity Severity { get; init; } = EventSeverity.Info;

    public IReadOnlyList<string> MaterializedLinks { get; init; } = [];

    public IReadOnlyList<string> UpdatedProperties { get; init; } = [];
}
