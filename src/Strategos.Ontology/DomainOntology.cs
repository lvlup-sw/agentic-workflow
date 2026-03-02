using Strategos.Ontology.Builder;

namespace Strategos.Ontology;

public abstract class DomainOntology
{
    public abstract string DomainName { get; }

    protected abstract void Define(IOntologyBuilder builder);

    internal void Build(IOntologyBuilder builder) => Define(builder);
}
