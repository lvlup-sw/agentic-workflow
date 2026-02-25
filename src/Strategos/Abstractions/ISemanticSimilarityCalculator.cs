// =============================================================================
// <copyright file="ISemanticSimilarityCalculator.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Abstractions;

/// <summary>
/// Calculates semantic similarity between text outputs for loop detection.
/// </summary>
/// <remarks>
/// <para>
/// Semantic similarity measures how conceptually similar two pieces of text are,
/// beyond simple string matching. This is used by loop detection to identify
/// semantic repetition where outputs are textually different but convey the same meaning.
/// </para>
/// <para>
/// Production implementations should use embedding models (e.g., text-embedding-3-small)
/// and cosine similarity. Placeholder implementations may return 0.0 for all comparisons,
/// effectively disabling semantic repetition detection until a real implementation
/// is provided.
/// </para>
/// </remarks>
public interface ISemanticSimilarityCalculator
{
    /// <summary>
    /// Calculates the maximum pairwise semantic similarity among a collection of outputs.
    /// </summary>
    /// <param name="outputs">Collection of text outputs to compare.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>
    /// The maximum similarity score between any two outputs, ranging from 0.0 (completely
    /// different) to 1.0 (identical meaning). Returns 0.0 if <paramref name="outputs"/>
    /// is null, empty, or contains fewer than 2 non-null entries.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is cancelled.
    /// </exception>
    /// <remarks>
    /// Accepting <see cref="IEnumerable{T}"/> allows callers to pass LINQ expressions
    /// directly without materializing intermediate collections, reducing allocations.
    /// </remarks>
    Task<double> CalculateMaxSimilarityAsync(
        IEnumerable<string?> outputs,
        CancellationToken cancellationToken = default);
}
