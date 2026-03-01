namespace Strategos.Ontology.Extensions;

/// <summary>
/// Collects workflow metadata (consumed/produced types) for ontology graph integration.
/// </summary>
public sealed class WorkflowMetadataBuilder
{
    public string WorkflowName { get; }

    public string? ConsumedTypeName { get; private set; }

    public string? ProducedTypeName { get; private set; }

    public WorkflowMetadataBuilder(string workflowName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowName);
        WorkflowName = workflowName;
    }

    public WorkflowMetadataBuilder Consumes<T>()
        where T : class
    {
        ConsumedTypeName = typeof(T).Name;
        return this;
    }

    public WorkflowMetadataBuilder Produces<T>()
        where T : class
    {
        ProducedTypeName = typeof(T).Name;
        return this;
    }
}
