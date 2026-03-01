using System.Linq.Expressions;

namespace Strategos.Ontology.Builder;

public interface IObjectTypeBuilder<T>
    where T : class
{
    void Key(Expression<Func<T, object>> keySelector);

    IPropertyBuilder Property(Expression<Func<T, object>> propertySelector);

    void HasOne<TLinked>(string linkName);

    void HasMany<TLinked>(string linkName);

    void ManyToMany<TLinked>(string linkName, Action<IEdgeBuilder>? edgeConfig);

    IActionBuilder Action(string actionName);

    void Event<TEvent>(Action<IEventBuilder<TEvent>> configure);

    void Implements<TInterface>(Action<IInterfaceMapping<T, TInterface>> configure);
}
