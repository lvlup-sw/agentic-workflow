namespace Strategos.Ontology.ObjectSets;

/// <summary>
/// Result of a similarity search, including relevance scores for each item.
/// </summary>
/// <typeparam name="T">The element type of the result set.</typeparam>
public sealed record ScoredObjectSetResult<T>(
    IReadOnlyList<T> Items,
    IReadOnlyList<double> Scores,
    int TotalCount,
    ObjectSetInclusion Inclusion);
