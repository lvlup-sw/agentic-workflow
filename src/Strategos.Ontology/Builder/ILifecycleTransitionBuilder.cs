namespace Strategos.Ontology.Builder;

public interface ILifecycleTransitionBuilder
{
    ILifecycleTransitionBuilder TriggeredByAction(string actionName);

    ILifecycleTransitionBuilder TriggeredByEvent<T>();

    ILifecycleTransitionBuilder Description(string description);
}
