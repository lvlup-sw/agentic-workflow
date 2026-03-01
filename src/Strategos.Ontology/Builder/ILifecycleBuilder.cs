namespace Strategos.Ontology.Builder;

public interface ILifecycleBuilder<TEnum>
    where TEnum : struct, Enum
{
    ILifecycleStateBuilder State(TEnum state);

    ILifecycleTransitionBuilder Transition(TEnum from, TEnum to);
}
