namespace Strategos.Ontology.Builder;

public interface ILifecycleStateBuilder
{
    ILifecycleStateBuilder Description(string description);

    ILifecycleStateBuilder Initial();

    ILifecycleStateBuilder Terminal();
}
