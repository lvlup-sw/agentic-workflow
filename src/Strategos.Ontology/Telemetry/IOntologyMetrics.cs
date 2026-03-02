namespace Strategos.Ontology.Telemetry;

/// <summary>
/// Interface for recording per-action and per-type ontology metrics.
/// </summary>
public interface IOntologyMetrics
{
    /// <summary>
    /// Records metrics for an ontology query operation.
    /// </summary>
    void RecordQuery(OntologyTelemetryContext context);

    /// <summary>
    /// Records metrics for an ontology action execution.
    /// </summary>
    void RecordAction(OntologyTelemetryContext context, TimeSpan duration);
}
