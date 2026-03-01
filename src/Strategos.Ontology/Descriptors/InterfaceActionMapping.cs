namespace Strategos.Ontology.Descriptors;

public sealed record InterfaceActionMapping
{
    public required string InterfaceActionName { get; init; }

    public required string ConcreteActionName { get; init; }
}
