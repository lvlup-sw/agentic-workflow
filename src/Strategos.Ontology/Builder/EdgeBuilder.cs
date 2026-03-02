using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Builder;

internal sealed class EdgeBuilder : IEdgeBuilder
{
    private readonly List<PropertyDescriptor> _properties = [];

    public IEdgeBuilder Property<TProp>(string name)
    {
        _properties.Add(new PropertyDescriptor(name, typeof(TProp)));
        return this;
    }

    public IReadOnlyList<PropertyDescriptor> Build() => _properties.AsReadOnly();
}
