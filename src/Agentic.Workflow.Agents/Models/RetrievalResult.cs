// =============================================================================
// <copyright file="RetrievalResult.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Agents.Models;

/// <summary>
/// Represents a single result from a vector similarity search.
/// </summary>
/// <remarks>
/// <para>
/// Retrieval results capture documents returned from RAG (Retrieval-Augmented Generation)
/// queries, including their content, relevance score, and source information.
/// </para>
/// </remarks>
/// <param name="Content">The retrieved text content.</param>
/// <param name="Score">The similarity score (0.0 to 1.0) indicating relevance.</param>
/// <param name="SourceId">Optional identifier of the source document.</param>
/// <param name="Metadata">Optional metadata about the retrieved content.</param>
public record RetrievalResult(
    string Content,
    double Score,
    string? SourceId = null,
    IReadOnlyDictionary<string, object?>? Metadata = null);