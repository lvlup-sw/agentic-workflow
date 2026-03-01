namespace Strategos.Ontology.Descriptors;

public sealed record LifecycleDescriptor
{
    public required string PropertyName { get; init; }

    public required string StateEnumTypeName { get; init; }

    public required IReadOnlyList<LifecycleStateDescriptor> States { get; init; }

    public required IReadOnlyList<LifecycleTransitionDescriptor> Transitions { get; init; }
}
