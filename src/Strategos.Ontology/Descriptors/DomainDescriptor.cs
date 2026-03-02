namespace Strategos.Ontology.Descriptors;

public sealed record DomainDescriptor(
    string DomainName)
{
    public IReadOnlyList<ObjectTypeDescriptor> ObjectTypes { get; init; } = [];
}
