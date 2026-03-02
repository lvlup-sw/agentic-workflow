using System.Linq.Expressions;

namespace Strategos.Ontology.Builder;

public interface IInterfaceMapping<TObject, TInterface>
{
    IInterfaceMapping<TObject, TInterface> Via(
        Expression<Func<TObject, object>> source,
        Expression<Func<TInterface, object>> target);

    IInterfaceMapping<TObject, TInterface> ActionVia(
        string interfaceActionName,
        string concreteActionName);

    IInterfaceMapping<TObject, TInterface> ActionDefault(
        string interfaceActionName,
        Action<IActionBuilder> configure);
}
