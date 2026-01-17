// =============================================================================
// <copyright file="InMemoryStepExecutionLedger.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Infrastructure.ExecutionLedgers;

/// <summary>
/// In-memory implementation of <see cref="IStepExecutionLedger"/> for testing and development.
/// </summary>
/// <remarks>
/// <para>
/// This implementation stores cached step results in memory using a concurrent dictionary.
/// It is suitable for testing, development, and single-process scenarios.
/// </para>
/// <para>
/// For distributed scenarios, use a Redis or database-backed implementation.
/// </para>
/// <list type="bullet">
///   <item><description>Thread-safe via <see cref="ConcurrentDictionary{TKey, TValue}"/></description></item>
///   <item><description>Supports time-based expiration via <see cref="TimeProvider"/></description></item>
///   <item><description>Uses SHA256 for deterministic input hashing</description></item>
/// </list>
/// </remarks>
public sealed class InMemoryStepExecutionLedger : IStepExecutionLedger
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryStepExecutionLedger"/> class.
    /// </summary>
    /// <param name="timeProvider">The time provider for TTL calculations.</param>
    public InMemoryStepExecutionLedger(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="stepName"/> or <paramref name="inputHash"/> is null or whitespace.
    /// </exception>
    public ValueTask<TResult?> TryGetCachedResultAsync<TResult>(
        string stepName,
        string inputHash,
        CancellationToken cancellationToken)
        where TResult : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName, nameof(stepName));
        ArgumentException.ThrowIfNullOrWhiteSpace(inputHash, nameof(inputHash));

        var key = BuildCacheKey(stepName, inputHash);

        if (!_cache.TryGetValue(key, out var entry))
        {
            // Return default ValueTask without allocation
            return default;
        }

        // Check TTL expiration
        if (entry.ExpiresAt.HasValue && _timeProvider.GetUtcNow() > entry.ExpiresAt.Value)
        {
            _cache.TryRemove(key, out _);
            return default;
        }

        var result = JsonSerializer.Deserialize<TResult>(entry.Json);
        return new ValueTask<TResult?>(result);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="stepName"/> or <paramref name="inputHash"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is null.
    /// </exception>
    public Task CacheResultAsync<TResult>(
        string stepName,
        string inputHash,
        TResult result,
        TimeSpan? ttl,
        CancellationToken cancellationToken)
        where TResult : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName, nameof(stepName));
        ArgumentException.ThrowIfNullOrWhiteSpace(inputHash, nameof(inputHash));
        ArgumentNullException.ThrowIfNull(result, nameof(result));

        var key = BuildCacheKey(stepName, inputHash);
        var json = JsonSerializer.Serialize(result);

        DateTimeOffset? expiresAt = ttl.HasValue
            ? _timeProvider.GetUtcNow().Add(ttl.Value)
            : null;

        var entry = new CacheEntry(json, expiresAt);
        _cache[key] = entry;

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="input"/> is null.
    /// </exception>
    /// <remarks>
    /// The hash is computed by serializing the input to JSON and then
    /// computing SHA256 hash of the UTF-8 encoded JSON bytes.
    /// </remarks>
    public string ComputeInputHash<TInput>(TInput input)
        where TInput : class
    {
        ArgumentNullException.ThrowIfNull(input, nameof(input));

        var json = JsonSerializer.Serialize(input);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hashBytes = SHA256.HashData(bytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static string BuildCacheKey(string stepName, string inputHash)
        => $"{stepName}:{inputHash}";

    private sealed record CacheEntry(string Json, DateTimeOffset? ExpiresAt);
}
