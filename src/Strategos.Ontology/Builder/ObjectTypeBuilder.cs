using System.Linq.Expressions;
using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Builder;

internal sealed class ObjectTypeBuilder<T>(string domainName) : IObjectTypeBuilder<T>
    where T : class
{
    private PropertyDescriptor? _keyProperty;
    private readonly List<PropertyBuilder> _propertyBuilders = [];
    private readonly List<LinkDescriptor> _links = [];
    private readonly List<ActionBuilder> _actionBuilders = [];
    private readonly List<EventDescriptor> _events = [];
    private readonly List<InterfaceDescriptor> _interfaces = [];

    public void Key(Expression<Func<T, object>> keySelector)
    {
        var memberName = ExpressionHelper.ExtractMemberName(keySelector);
        var memberType = ExpressionHelper.ExtractMemberType(keySelector);
        _keyProperty = new PropertyDescriptor(memberName, memberType);
    }

    public IPropertyBuilder Property(Expression<Func<T, object>> propertySelector)
    {
        var memberName = ExpressionHelper.ExtractMemberName(propertySelector);
        var memberType = ExpressionHelper.ExtractMemberType(propertySelector);
        var builder = new PropertyBuilder(memberName, memberType);
        _propertyBuilders.Add(builder);
        return builder;
    }

    public void HasOne<TLinked>(string linkName)
    {
        _links.Add(new LinkDescriptor(linkName, typeof(TLinked).Name, LinkCardinality.OneToOne));
    }

    public void HasMany<TLinked>(string linkName)
    {
        _links.Add(new LinkDescriptor(linkName, typeof(TLinked).Name, LinkCardinality.OneToMany));
    }

    public void ManyToMany<TLinked>(string linkName, Action<IEdgeBuilder>? edgeConfig)
    {
        IReadOnlyList<PropertyDescriptor> edgeProperties = [];
        if (edgeConfig is not null)
        {
            var edgeBuilder = new EdgeBuilder();
            edgeConfig(edgeBuilder);
            edgeProperties = edgeBuilder.Build();
        }

        _links.Add(new LinkDescriptor(linkName, typeof(TLinked).Name, LinkCardinality.ManyToMany)
        {
            EdgeProperties = edgeProperties,
        });
    }

    public IActionBuilder Action(string actionName)
    {
        var builder = new ActionBuilder(actionName);
        _actionBuilders.Add(builder);
        return builder;
    }

    public void Event<TEvent>(Action<IEventBuilder<TEvent>> configure)
    {
        var builder = new EventBuilder<TEvent>();
        configure(builder);
        _events.Add(builder.Build());
    }

    public void Implements<TInterface>(Action<IInterfaceMapping<T, TInterface>> configure)
    {
        var mapping = new InterfaceMapping<T, TInterface>();
        configure(mapping);
        _interfaces.Add(new InterfaceDescriptor(typeof(TInterface).Name, typeof(TInterface)));
    }

    public ObjectTypeDescriptor Build() =>
        new(typeof(T).Name, typeof(T), domainName)
        {
            KeyProperty = _keyProperty,
            Properties = _propertyBuilders.ConvertAll(b => b.Build()).AsReadOnly(),
            Links = _links.AsReadOnly(),
            Actions = _actionBuilders.ConvertAll(b => b.Build()).AsReadOnly(),
            Events = _events.AsReadOnly(),
            ImplementedInterfaces = _interfaces.AsReadOnly(),
        };
}
