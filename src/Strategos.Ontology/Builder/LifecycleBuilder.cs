using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Builder;

internal sealed class LifecycleBuilder<TEnum> : ILifecycleBuilder<TEnum>
    where TEnum : struct, Enum
{
    private readonly List<LifecycleStateBuilder> _stateBuilders = [];
    private readonly List<LifecycleTransitionBuilder> _transitionBuilders = [];

    public ILifecycleStateBuilder State(TEnum state)
    {
        var builder = new LifecycleStateBuilder(state.ToString());
        _stateBuilders.Add(builder);
        return builder;
    }

    public ILifecycleTransitionBuilder Transition(TEnum from, TEnum to)
    {
        var builder = new LifecycleTransitionBuilder(from.ToString(), to.ToString());
        _transitionBuilders.Add(builder);
        return builder;
    }

    public LifecycleDescriptor Build() =>
        new()
        {
            PropertyName = string.Empty, // Set by ObjectTypeBuilder
            StateEnumTypeName = typeof(TEnum).Name,
            States = _stateBuilders.ConvertAll(b => b.Build()).AsReadOnly(),
            Transitions = _transitionBuilders.ConvertAll(b => b.Build()).AsReadOnly(),
        };
}
