// =============================================================================
// <copyright file="IArtifactStore.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Contract for storing and retrieving large artifacts using the claim-check pattern.
/// </summary>
/// <remarks>
/// <para>
/// The artifact store enables workflows to handle large payloads efficiently:
/// <list type="bullet">
///   <item><description>LLM responses that exceed event size limits</description></item>
///   <item><description>Assembled context for agent steps</description></item>
///   <item><description>RAG retrieval results</description></item>
/// </list>
/// </para>
/// <para>
/// Implementations may use Azure Blob Storage, S3, local filesystem, or other backends.
/// The returned URI should be opaque to consumers - only the same store implementation
/// can retrieve artifacts from its URIs.
/// </para>
/// </remarks>
public interface IArtifactStore
{
    /// <summary>
    /// Stores an artifact and returns a URI reference.
    /// </summary>
    /// <typeparam name="T">The artifact type.</typeparam>
    /// <param name="artifact">The artifact to store.</param>
    /// <param name="category">Category for organization (e.g., "llm-response", "context").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A URI reference to the stored artifact.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="artifact"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="category"/> is null or whitespace.
    /// </exception>
    Task<Uri> StoreAsync<T>(T artifact, string category, CancellationToken cancellationToken)
        where T : class;

    /// <summary>
    /// Retrieves an artifact by its URI reference.
    /// </summary>
    /// <typeparam name="T">The expected artifact type.</typeparam>
    /// <param name="reference">The URI reference from a previous StoreAsync call.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized artifact.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="reference"/> is null.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no artifact exists at the specified reference.
    /// </exception>
    Task<T> RetrieveAsync<T>(Uri reference, CancellationToken cancellationToken)
        where T : class;

    /// <summary>
    /// Deletes an artifact by its URI reference.
    /// </summary>
    /// <param name="reference">The URI reference to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="reference"/> is null.
    /// </exception>
    /// <remarks>
    /// This method is idempotent - deleting a non-existent artifact succeeds silently.
    /// </remarks>
    Task DeleteAsync(Uri reference, CancellationToken cancellationToken);
}
