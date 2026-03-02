namespace Strategos.Ontology.ObjectSets;

/// <summary>
/// Result of materializing a similarity search query, with relevance scores.
/// </summary>
/// <typeparam name="T">The element type of the result set.</typeparam>
public sealed record ScoredObjectSetResult<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScoredObjectSetResult{T}"/> class.
    /// </summary>
    /// <param name="items">The result items.</param>
    /// <param name="totalCount">Total number of matching items before any limit was applied.</param>
    /// <param name="inclusion">Which data facets are included in the result items.</param>
    /// <param name="scores">Relevance scores corresponding 1-to-1 with <paramref name="items"/>.</param>
    public ScoredObjectSetResult(
        IReadOnlyList<T> items,
        int totalCount,
        ObjectSetInclusion inclusion,
        IReadOnlyList<double> scores)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(scores);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        if (scores.Count != items.Count)
        {
            throw new ArgumentException(
                $"Scores count ({scores.Count}) must match Items count ({items.Count}).");
        }

        Items = items;
        TotalCount = totalCount;
        Inclusion = inclusion;
        Scores = scores;
    }

    /// <summary>The result items.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Total number of matching items before any limit was applied.</summary>
    public int TotalCount { get; }

    /// <summary>Which data facets are included in the result items.</summary>
    public ObjectSetInclusion Inclusion { get; }

    /// <summary>Relevance scores corresponding 1-to-1 with <see cref="Items"/>.</summary>
    public IReadOnlyList<double> Scores { get; }
}
