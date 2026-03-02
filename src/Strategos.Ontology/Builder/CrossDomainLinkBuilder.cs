using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Builder;

internal sealed class CrossDomainLinkBuilder(string name) : ICrossDomainLinkBuilder
{
    private Type _sourceType = typeof(object);
    private string _targetDomain = string.Empty;
    private string _targetTypeName = string.Empty;
    private LinkCardinality _cardinality = LinkCardinality.OneToOne;
    private IReadOnlyList<PropertyDescriptor> _edgeProperties = [];

    public ICrossDomainLinkBuilder From<T>()
    {
        _sourceType = typeof(T);
        return this;
    }

    public ICrossDomainLinkBuilder ToExternal(string domain, string typeName)
    {
        _targetDomain = domain;
        _targetTypeName = typeName;
        return this;
    }

    public ICrossDomainLinkBuilder ManyToMany()
    {
        _cardinality = LinkCardinality.ManyToMany;
        return this;
    }

    public ICrossDomainLinkBuilder WithEdge(Action<IEdgeBuilder> configure)
    {
        var edgeBuilder = new EdgeBuilder();
        configure(edgeBuilder);
        _edgeProperties = edgeBuilder.Build();
        return this;
    }

    public CrossDomainLinkDescriptor Build() =>
        new(name, _sourceType, _targetDomain, _targetTypeName, _cardinality)
        {
            EdgeProperties = _edgeProperties,
        };
}
