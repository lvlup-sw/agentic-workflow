using System.Linq.Expressions;

namespace Strategos.Ontology.Builder;

public interface IInterfaceBuilder<TInterface>
{
    IInterfaceBuilder<TInterface> Property(Expression<Func<TInterface, object>> propertySelector);
}
