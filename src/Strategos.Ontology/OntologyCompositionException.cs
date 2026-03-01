namespace Strategos.Ontology;

public sealed class OntologyCompositionException : Exception
{
    public OntologyCompositionException(string message)
        : base(message)
    {
    }

    public OntologyCompositionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
