// =============================================================================
// <copyright file="IStepExecutionLedger.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Contract for memoizing expensive step operations to enable idempotent replay.
/// </summary>
/// <remarks>
/// <para>
/// The step execution ledger provides caching for expensive operations:
/// <list type="bullet">
///   <item><description>LLM API calls that are costly and slow</description></item>
///   <item><description>External service calls during workflow retry/recovery</description></item>
///   <item><description>Idempotent replay of previously executed steps</description></item>
/// </list>
/// </para>
/// <para>
/// Cache keys are computed from step name and a deterministic hash of the input.
/// Implementations may use Redis, in-memory cache, or database-backed storage.
/// </para>
/// </remarks>
public interface IStepExecutionLedger
{
    /// <summary>
    /// Attempts to retrieve a cached result for a previous step execution.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="stepName">The step name.</param>
    /// <param name="inputHash">Hash of the step input (from ComputeInputHash).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached result if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="stepName"/> or <paramref name="inputHash"/> is null or whitespace.
    /// </exception>
    /// <remarks>
    /// Returns <see cref="ValueTask{TResult}"/> to avoid Task allocation for synchronous
    /// cache lookups. Most implementations use in-memory dictionaries where the result
    /// is available immediately.
    /// </remarks>
    ValueTask<TResult?> TryGetCachedResultAsync<TResult>(
        string stepName,
        string inputHash,
        CancellationToken cancellationToken)
        where TResult : class;

    /// <summary>
    /// Caches a step execution result.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="stepName">The step name.</param>
    /// <param name="inputHash">Hash of the step input (from ComputeInputHash).</param>
    /// <param name="result">The result to cache.</param>
    /// <param name="ttl">Time-to-live for the cache entry; null uses implementation default.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="stepName"/> or <paramref name="inputHash"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is null.
    /// </exception>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CacheResultAsync<TResult>(
        string stepName,
        string inputHash,
        TResult result,
        TimeSpan? ttl,
        CancellationToken cancellationToken)
        where TResult : class;

    /// <summary>
    /// Computes a deterministic hash from step input for cache key generation.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <param name="input">The input to hash.</param>
    /// <returns>A deterministic hash string (e.g., SHA256 hex).</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="input"/> is null.
    /// </exception>
    /// <remarks>
    /// The hash must be deterministic - the same input must always produce the same hash.
    /// Implementations should serialize the input to JSON before hashing for consistency.
    /// </remarks>
    string ComputeInputHash<TInput>(TInput input)
        where TInput : class;
}
