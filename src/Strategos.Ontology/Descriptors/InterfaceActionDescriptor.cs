namespace Strategos.Ontology.Descriptors;

public sealed record InterfaceActionDescriptor
{
    public required string Name { get; init; }

    public string? Description { get; init; }

    public string? AcceptsTypeName { get; init; }

    public string? ReturnsTypeName { get; init; }
}
