using System.Linq.Expressions;
using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Builder;

internal sealed class EventBuilder<TEvent> : IEventBuilder<TEvent>
{
    private string _description = string.Empty;
    private EventSeverity _severity = EventSeverity.Info;
    private readonly List<string> _materializedLinks = [];
    private readonly List<string> _updatedProperties = [];

    public IEventBuilder<TEvent> Description(string description)
    {
        _description = description;
        return this;
    }

    public IEventBuilder<TEvent> MaterializesLink<TOwner>(
        string linkName,
        Expression<Func<TEvent, object>> keySelector)
    {
        _materializedLinks.Add(linkName);
        return this;
    }

    public IEventBuilder<TEvent> UpdatesProperty<TOwner>(
        Expression<Func<TOwner, object>> property,
        Expression<Func<TEvent, object>> valueSelector)
    {
        var propertyName = ExpressionHelper.ExtractMemberName(property);
        _updatedProperties.Add(propertyName);
        return this;
    }

    public IEventBuilder<TEvent> Severity(EventSeverity severity)
    {
        _severity = severity;
        return this;
    }

    public EventDescriptor Build() =>
        new(typeof(TEvent), _description)
        {
            Severity = _severity,
            MaterializedLinks = _materializedLinks.AsReadOnly(),
            UpdatedProperties = _updatedProperties.AsReadOnly(),
        };
}
