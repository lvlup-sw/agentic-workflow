using System.Linq.Expressions;
using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Builder;

internal sealed class InterfaceBuilder<T>(string name) : IInterfaceBuilder<T>
{
    private readonly List<PropertyDescriptor> _properties = [];

    public IInterfaceBuilder<T> Property(Expression<Func<T, object>> propertySelector)
    {
        var memberName = ExpressionHelper.ExtractMemberName(propertySelector);
        var memberType = ExpressionHelper.ExtractMemberType(propertySelector);
        _properties.Add(new PropertyDescriptor(memberName, memberType));
        return this;
    }

    public InterfaceDescriptor Build() =>
        new(name, typeof(T))
        {
            Properties = _properties.AsReadOnly(),
        };
}
