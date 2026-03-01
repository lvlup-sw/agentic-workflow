namespace Strategos.Ontology.Descriptors;

public sealed record PropertyDescriptor(
    string Name,
    Type PropertyType,
    bool IsRequired = false,
    bool IsComputed = false,
    string? ExpressionPath = null)
{
    public IReadOnlyList<DerivationSource> DerivedFrom { get; init; } = [];

    public IReadOnlyList<DerivationSource> TransitiveDerivedFrom { get; init; } = [];
}
