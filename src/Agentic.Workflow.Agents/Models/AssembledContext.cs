// =============================================================================
// <copyright file="AssembledContext.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Agents.Models;

/// <summary>
/// Represents assembled context ready for injection into an LLM prompt.
/// </summary>
/// <remarks>
/// <para>
/// Assembled context is the result of gathering information from multiple
/// sources (state, vector search, literals) and combining them into a
/// coherent context for agent prompts.
/// </para>
/// </remarks>
public sealed class AssembledContext
{
    /// <summary>
    /// Gets an empty assembled context with no segments.
    /// </summary>
    public static readonly AssembledContext Empty = new([]);

    /// <summary>
    /// Gets the context segments in order of assembly.
    /// </summary>
    public IReadOnlyList<ContextSegment> Segments { get; }

    /// <summary>
    /// Gets a value indicating whether this context has no segments.
    /// </summary>
    public bool IsEmpty => Segments.Count == 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssembledContext"/> class.
    /// </summary>
    /// <param name="segments">The context segments.</param>
    public AssembledContext(IReadOnlyList<ContextSegment> segments)
    {
        Segments = segments;
    }

    /// <summary>
    /// Converts all segments to a single prompt string.
    /// </summary>
    /// <returns>The combined prompt string with segments separated by double newlines.</returns>
    public string ToPromptString() =>
        string.Join("\n\n", Segments.Select(s => s.ToPromptString()));
}