// =============================================================================
// <copyright file="ContextDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Collections.Immutable;

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Immutable definition of context assembly for a workflow step.
/// </summary>
/// <remarks>
/// <para>
/// Context definitions aggregate multiple context sources that are assembled
/// at runtime to provide RAG context for agent steps.
/// </para>
/// <para>
/// Sources are assembled in order, concatenated with newlines.
/// </para>
/// </remarks>
public sealed record ContextDefinition
{
    /// <summary>
    /// Gets the ordered list of context sources.
    /// </summary>
    public IReadOnlyList<ContextSourceDefinition> Sources { get; init; }
        = ImmutableList<ContextSourceDefinition>.Empty;

    /// <summary>
    /// Gets an empty context definition.
    /// </summary>
    public static ContextDefinition Empty { get; } = new();

    /// <summary>
    /// Returns a new definition with the specified source added.
    /// </summary>
    /// <param name="source">The context source to add.</param>
    /// <returns>A new context definition with the source appended.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> is null.
    /// </exception>
    public ContextDefinition WithSource(ContextSourceDefinition source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        var sources = Sources.ToImmutableList().Add(source);
        return this with { Sources = sources };
    }

    /// <summary>
    /// Gets whether this definition has any context sources.
    /// </summary>
    public bool HasSources => Sources.Count > 0;
}

/// <summary>
/// Base type for context source definitions.
/// </summary>
/// <remarks>
/// Context sources are discriminated union types representing different
/// ways to obtain context at runtime.
/// </remarks>
public abstract record ContextSourceDefinition;

/// <summary>
/// Context source that extracts a value from workflow state.
/// </summary>
/// <remarks>
/// <para>
/// The PropertyPath stores the expression string for source generation.
/// The Selector delegate is used at runtime for value extraction.
/// </para>
/// </remarks>
public sealed record StateContextSource : ContextSourceDefinition
{
    /// <summary>
    /// Gets the property path expression string (for source generation).
    /// </summary>
    /// <remarks>
    /// Example: "state => state.CustomerName" or "state => state.Order.Summary".
    /// </remarks>
    public required string PropertyPath { get; init; }

    /// <summary>
    /// Gets the selector delegate for runtime value extraction.
    /// </summary>
    /// <remarks>
    /// This delegate is compiled from the expression and cached for performance.
    /// It returns object to support any property type.
    /// </remarks>
    public required Delegate Selector { get; init; }

    /// <summary>
    /// Gets the return type of the selector expression.
    /// </summary>
    public required Type ValueType { get; init; }
}

/// <summary>
/// Context source that retrieves documents from a RAG collection.
/// </summary>
/// <remarks>
/// <para>
/// Retrieval sources specify:
/// <list type="bullet">
///   <item><description>Collection type for DI resolution</description></item>
///   <item><description>Retrieval configuration (query, filters, limits)</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record RetrievalContextSource : ContextSourceDefinition
{
    /// <summary>
    /// Gets the collection type marker for DI resolution.
    /// </summary>
    public required Type CollectionType { get; init; }

    /// <summary>
    /// Gets the retrieval configuration.
    /// </summary>
    public required RetrievalDefinition Retrieval { get; init; }
}

/// <summary>
/// Context source with a static literal string.
/// </summary>
public sealed record LiteralContextSource : ContextSourceDefinition
{
    /// <summary>
    /// Gets the literal context value.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// Creates a new literal context source.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>A new literal context source.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is null.
    /// </exception>
    public static LiteralContextSource Create(string value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        return new LiteralContextSource { Value = value };
    }
}

/// <summary>
/// Configuration for RAG retrieval operations.
/// </summary>
/// <remarks>
/// <para>
/// Retrieval definitions specify how to query a RAG collection:
/// <list type="bullet">
///   <item><description>Query: Dynamic (state-dependent) or static text</description></item>
///   <item><description>TopK: Maximum results to return</description></item>
///   <item><description>MinRelevance: Relevance score threshold</description></item>
///   <item><description>Filters: Metadata filters for narrowing results</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record RetrievalDefinition
{
    /// <summary>
    /// Gets the dynamic query factory delegate.
    /// </summary>
    /// <remarks>
    /// Mutually exclusive with LiteralQuery. One must be set.
    /// </remarks>
    public Delegate? QueryFactory { get; init; }

    /// <summary>
    /// Gets the static query text.
    /// </summary>
    /// <remarks>
    /// Mutually exclusive with QueryFactory. One must be set.
    /// </remarks>
    public string? LiteralQuery { get; init; }

    /// <summary>
    /// Gets the maximum number of documents to retrieve.
    /// </summary>
    /// <remarks>
    /// Defaults to 5 if not specified.
    /// </remarks>
    public int TopK { get; init; } = 5;

    /// <summary>
    /// Gets the minimum relevance score threshold.
    /// </summary>
    /// <remarks>
    /// Results below this threshold are excluded.
    /// Defaults to 0.7 (70% similarity).
    /// </remarks>
    public decimal MinRelevance { get; init; } = 0.7m;

    /// <summary>
    /// Gets the metadata filters to apply.
    /// </summary>
    public IReadOnlyList<RetrievalFilter> Filters { get; init; }
        = ImmutableList<RetrievalFilter>.Empty;

    /// <summary>
    /// Returns a new definition with the specified filter added.
    /// </summary>
    /// <param name="filter">The filter to add.</param>
    /// <returns>A new retrieval definition with the filter appended.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="filter"/> is null.
    /// </exception>
    public RetrievalDefinition WithFilter(RetrievalFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter, nameof(filter));

        var filters = Filters.ToImmutableList().Add(filter);
        return this with { Filters = filters };
    }
}

/// <summary>
/// A metadata filter for RAG retrieval.
/// </summary>
/// <remarks>
/// Filters can be static (fixed value) or dynamic (state-dependent).
/// </remarks>
public sealed record RetrievalFilter
{
    /// <summary>
    /// Gets the metadata key to filter on.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the static filter value.
    /// </summary>
    /// <remarks>
    /// Mutually exclusive with ValueSelector. One must be set.
    /// </remarks>
    public object? StaticValue { get; init; }

    /// <summary>
    /// Gets the dynamic value selector delegate.
    /// </summary>
    /// <remarks>
    /// Mutually exclusive with StaticValue. One must be set.
    /// </remarks>
    public Delegate? ValueSelector { get; init; }

    /// <summary>
    /// Gets whether this is a static filter.
    /// </summary>
    public bool IsStatic => StaticValue is not null;

    /// <summary>
    /// Creates a static filter with a fixed value.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The filter value.</param>
    /// <returns>A new static retrieval filter.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="key"/> or <paramref name="value"/> is null.
    /// </exception>
    public static RetrievalFilter Static(string key, object value)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        return new RetrievalFilter { Key = key, StaticValue = value };
    }

    /// <summary>
    /// Creates a dynamic filter with a state-dependent value.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="valueSelector">The value selector delegate.</param>
    /// <returns>A new dynamic retrieval filter.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="key"/> or <paramref name="valueSelector"/> is null.
    /// </exception>
    public static RetrievalFilter Dynamic(string key, Delegate valueSelector)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(valueSelector, nameof(valueSelector));

        return new RetrievalFilter { Key = key, ValueSelector = valueSelector };
    }
}