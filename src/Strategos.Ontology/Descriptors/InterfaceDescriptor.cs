namespace Strategos.Ontology.Descriptors;

public sealed record InterfaceDescriptor(
    string Name,
    Type InterfaceType,
    IReadOnlyList<PropertyDescriptor>? Properties = null)
{
    public IReadOnlyList<PropertyDescriptor> Properties { get; init; } =
        Properties ?? [];
}
