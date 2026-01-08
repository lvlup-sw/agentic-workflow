// =============================================================================
// <copyright file="AssembledContextBuilder.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Agents.Models;

/// <summary>
/// Fluent builder for constructing <see cref="AssembledContext"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// The builder provides a fluent API for gathering context from multiple sources:
/// <list type="bullet">
///   <item><description>State values from the workflow state</description></item>
///   <item><description>Retrieved documents from vector search</description></item>
///   <item><description>Literal text injections</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class AssembledContextBuilder
{
    private readonly List<ContextSegment> _segments = [];

    /// <summary>
    /// Adds a state context segment with a value from workflow state.
    /// </summary>
    /// <param name="name">The name of the state field.</param>
    /// <param name="value">The value from the workflow state.</param>
    /// <returns>This builder for method chaining.</returns>
    public AssembledContextBuilder AddStateContext(string name, object? value)
    {
        _segments.Add(new StateContextSegment(name, value));
        return this;
    }

    /// <summary>
    /// Adds a retrieval context segment with results from vector search.
    /// </summary>
    /// <param name="collectionName">The name of the vector collection.</param>
    /// <param name="results">The retrieval results from the search.</param>
    /// <returns>This builder for method chaining.</returns>
    public AssembledContextBuilder AddRetrievalContext(
        string collectionName,
        IReadOnlyList<RetrievalResult> results)
    {
        _segments.Add(new RetrievalContextSegment(collectionName, results));
        return this;
    }

    /// <summary>
    /// Adds a literal context segment with static text.
    /// </summary>
    /// <param name="value">The literal text to include.</param>
    /// <returns>This builder for method chaining.</returns>
    public AssembledContextBuilder AddLiteralContext(string value)
    {
        _segments.Add(new LiteralContextSegment(value));
        return this;
    }

    /// <summary>
    /// Builds the assembled context from all added segments.
    /// </summary>
    /// <returns>
    /// The assembled context, or <see cref="AssembledContext.Empty"/>
    /// if no segments were added.
    /// </returns>
    public AssembledContext Build() =>
        _segments.Count == 0 ? AssembledContext.Empty : new AssembledContext(_segments.ToList());
}
