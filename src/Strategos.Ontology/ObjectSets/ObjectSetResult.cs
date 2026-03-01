namespace Strategos.Ontology.ObjectSets;

/// <summary>
/// Result of materializing an object set query.
/// </summary>
/// <typeparam name="T">The element type of the result set.</typeparam>
public sealed record ObjectSetResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    ObjectSetInclusion Inclusion);
