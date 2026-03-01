using System.Linq.Expressions;

namespace Strategos.Ontology.Builder;

public interface IInterfaceMapping<TObject, TInterface>
{
    IInterfaceMapping<TObject, TInterface> Via(
        Expression<Func<TObject, object>> source,
        Expression<Func<TInterface, object>> target);
}
