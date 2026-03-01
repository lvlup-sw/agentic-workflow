namespace Strategos.Ontology.Descriptors;

public sealed record LifecycleTransitionDescriptor
{
    public required string FromState { get; init; }

    public required string ToState { get; init; }

    public string? TriggerActionName { get; init; }

    public string? TriggerEventTypeName { get; init; }

    public string? Description { get; init; }
}
