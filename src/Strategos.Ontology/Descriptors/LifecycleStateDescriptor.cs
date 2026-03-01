namespace Strategos.Ontology.Descriptors;

public sealed record LifecycleStateDescriptor
{
    public required string Name { get; init; }

    public string? Description { get; init; }

    public bool IsInitial { get; init; }

    public bool IsTerminal { get; init; }
}
