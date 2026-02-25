// =============================================================================
// <copyright file="ContextSegment.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Agents.Models;

/// <summary>
/// Base class for context segments that can be assembled into LLM prompts.
/// </summary>
/// <remarks>
/// <para>
/// Context segments represent discrete pieces of information that can be
/// combined to form a complete context for agent prompts. Each segment
/// knows how to render itself as a prompt string.
/// </para>
/// </remarks>
public abstract record ContextSegment
{
    /// <summary>
    /// Converts the segment content to a string suitable for inclusion in a prompt.
    /// </summary>
    /// <returns>The prompt-ready string representation of this segment.</returns>
    public abstract string ToPromptString();
}

/// <summary>
/// A context segment derived from workflow state values.
/// </summary>
/// <remarks>
/// <para>
/// State context segments capture values from the workflow state that
/// should be included in the LLM context. The value is converted to
/// a string using <see cref="object.ToString"/>.
/// </para>
/// </remarks>
/// <param name="Name">The name of the state field.</param>
/// <param name="Value">The value from the workflow state.</param>
public record StateContextSegment(string Name, object? Value) : ContextSegment
{
    /// <inheritdoc/>
    public override string ToPromptString() => Value?.ToString() ?? string.Empty;
}

/// <summary>
/// A context segment containing literal text.
/// </summary>
/// <remarks>
/// <para>
/// Literal context segments contain static or computed text that should
/// be included in the LLM context without transformation.
/// </para>
/// </remarks>
/// <param name="Value">The literal text value.</param>
public record LiteralContextSegment(string Value) : ContextSegment
{
    /// <inheritdoc/>
    public override string ToPromptString() => Value;
}

/// <summary>
/// A context segment containing results from vector similarity search.
/// </summary>
/// <remarks>
/// <para>
/// Retrieval context segments represent documents retrieved from RAG
/// (Retrieval-Augmented Generation) queries. Multiple results are
/// joined with a separator for clear delineation.
/// </para>
/// </remarks>
/// <param name="CollectionName">The name of the vector collection searched.</param>
/// <param name="Results">The retrieval results from the search.</param>
public record RetrievalContextSegment(
    string CollectionName,
    IReadOnlyList<RetrievalResult> Results) : ContextSegment
{
    /// <inheritdoc/>
    public override string ToPromptString() =>
        string.Join("\n---\n", Results.Select(r => r.Content));
}
