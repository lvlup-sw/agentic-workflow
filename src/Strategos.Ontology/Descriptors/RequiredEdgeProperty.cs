namespace Strategos.Ontology.Descriptors;

public sealed record RequiredEdgeProperty
{
    public required string Name { get; init; }

    public required string TypeName { get; init; }
}
