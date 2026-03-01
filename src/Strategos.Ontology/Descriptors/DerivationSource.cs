namespace Strategos.Ontology.Descriptors;

public sealed record DerivationSource
{
    public required DerivationSourceKind Kind { get; init; }

    public string? PropertyName { get; init; }

    public string? ExternalDomain { get; init; }

    public string? ExternalObjectType { get; init; }

    public string? ExternalPropertyName { get; init; }
}
