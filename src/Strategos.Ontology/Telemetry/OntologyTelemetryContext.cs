using Strategos.Ontology.ObjectSets;

namespace Strategos.Ontology.Telemetry;

/// <summary>
/// Captures telemetry context for ontology operations.
/// </summary>
public sealed record OntologyTelemetryContext(
    string Domain,
    string ObjectType,
    string? ActionName = null,
    IReadOnlyList<string>? TraversedLinks = null,
    IReadOnlyList<string>? ProducedEvents = null,
    int QueryDepth = 0,
    ObjectSetInclusion? InclusionLevel = null)
{
    /// <summary>
    /// Links traversed during the operation. Defaults to empty.
    /// </summary>
    public IReadOnlyList<string> TraversedLinks { get; init; } = TraversedLinks ?? [];

    /// <summary>
    /// Events produced during the operation. Defaults to empty.
    /// </summary>
    public IReadOnlyList<string> ProducedEvents { get; init; } = ProducedEvents ?? [];
}
