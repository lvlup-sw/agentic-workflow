using System.Linq.Expressions;
using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Builder;

public interface IEventBuilder<TEvent>
{
    IEventBuilder<TEvent> Description(string description);

    IEventBuilder<TEvent> MaterializesLink<TOwner>(
        string linkName,
        Expression<Func<TEvent, object>> keySelector);

    IEventBuilder<TEvent> UpdatesProperty<TOwner>(
        Expression<Func<TOwner, object>> property,
        Expression<Func<TEvent, object>> valueSelector);

    IEventBuilder<TEvent> Severity(EventSeverity severity);
}
