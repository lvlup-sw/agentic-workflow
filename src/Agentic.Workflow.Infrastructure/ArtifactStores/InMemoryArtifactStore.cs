// =============================================================================
// <copyright file="InMemoryArtifactStore.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Infrastructure.ArtifactStores;

/// <summary>
/// In-memory implementation of <see cref="IArtifactStore"/> for testing and development.
/// </summary>
/// <remarks>
/// <para>
/// This implementation stores artifacts in memory using a concurrent dictionary.
/// It is suitable for testing, development, and scenarios where durability is not required.
/// </para>
/// <para>
/// For production use with durability requirements, use <see cref="FileSystemArtifactStore"/>
/// or a cloud-based implementation.
/// </para>
/// <list type="bullet">
///   <item><description>Thread-safe via <see cref="ConcurrentDictionary{TKey, TValue}"/></description></item>
///   <item><description>Uses JSON serialization for artifact storage</description></item>
///   <item><description>URI scheme: memory://artifacts/{category}/{id}</description></item>
/// </list>
/// </remarks>
public sealed class InMemoryArtifactStore : IArtifactStore
{
    private readonly ConcurrentDictionary<string, string> _artifacts = new();
    private long _counter;

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="artifact"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="category"/> is null or whitespace.
    /// </exception>
    public Task<Uri> StoreAsync<T>(T artifact, string category, CancellationToken cancellationToken)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(artifact, nameof(artifact));
        ArgumentException.ThrowIfNullOrWhiteSpace(category, nameof(category));

        var id = Interlocked.Increment(ref _counter);
        var key = $"{category}/{id}";
        var json = JsonSerializer.Serialize(artifact);

        _artifacts[key] = json;

        var uri = new Uri($"memory://artifacts/{key}");
        return Task.FromResult(uri);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="reference"/> is null.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no artifact exists at the specified reference.
    /// </exception>
    public Task<T> RetrieveAsync<T>(Uri reference, CancellationToken cancellationToken)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(reference, nameof(reference));

        var key = ExtractKeyFromUri(reference);

        if (!_artifacts.TryGetValue(key, out var json))
        {
            throw new KeyNotFoundException($"Artifact not found: {reference}");
        }

        var artifact = JsonSerializer.Deserialize<T>(json)
            ?? throw new InvalidOperationException($"Failed to deserialize artifact: {reference}");

        return Task.FromResult(artifact);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="reference"/> is null.
    /// </exception>
    /// <remarks>
    /// This method is idempotent - deleting a non-existent artifact succeeds silently.
    /// </remarks>
    public Task DeleteAsync(Uri reference, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reference, nameof(reference));

        var key = ExtractKeyFromUri(reference);
        _artifacts.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    private static string ExtractKeyFromUri(Uri uri)
    {
        // URI format: memory://artifacts/{category}/{id}
        // Path will be: /artifacts/{category}/{id}
        var path = uri.AbsolutePath;

        if (path.StartsWith("/artifacts/", StringComparison.OrdinalIgnoreCase))
        {
            return path["/artifacts/".Length..];
        }

        return path.TrimStart('/');
    }
}
