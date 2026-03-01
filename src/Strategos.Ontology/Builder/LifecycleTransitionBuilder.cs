using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Builder;

internal sealed class LifecycleTransitionBuilder(string fromState, string toState) : ILifecycleTransitionBuilder
{
    private string? _triggerActionName;
    private string? _triggerEventTypeName;
    private string? _description;

    public ILifecycleTransitionBuilder TriggeredByAction(string actionName)
    {
        _triggerActionName = actionName;
        return this;
    }

    public ILifecycleTransitionBuilder TriggeredByEvent<T>()
    {
        _triggerEventTypeName = typeof(T).Name;
        return this;
    }

    public ILifecycleTransitionBuilder Description(string description)
    {
        _description = description;
        return this;
    }

    public LifecycleTransitionDescriptor Build() =>
        new()
        {
            FromState = fromState,
            ToState = toState,
            TriggerActionName = _triggerActionName,
            TriggerEventTypeName = _triggerEventTypeName,
            Description = _description,
        };
}
