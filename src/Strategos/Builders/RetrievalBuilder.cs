// =============================================================================
// <copyright file="RetrievalBuilder.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Definitions;

namespace Strategos.Builders;

/// <summary>
/// Internal implementation of the retrieval builder.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <typeparam name="TCollection">The collection type marker.</typeparam>
internal sealed class RetrievalBuilder<TState, TCollection> : IRetrievalBuilder<TState, TCollection>
    where TState : class, IWorkflowState
{
    private RetrievalDefinition _definition = new();

    /// <summary>
    /// Gets the built retrieval definition.
    /// </summary>
    internal RetrievalDefinition Definition => _definition;

    /// <inheritdoc/>
    public IRetrievalBuilder<TState, TCollection> Query(Func<TState, string> queryFactory)
    {
        ArgumentNullException.ThrowIfNull(queryFactory, nameof(queryFactory));

        _definition = _definition with
        {
            QueryFactory = queryFactory,
            LiteralQuery = null,
        };

        return this;
    }

    /// <inheritdoc/>
    public IRetrievalBuilder<TState, TCollection> Query(string queryText)
    {
        ArgumentNullException.ThrowIfNull(queryText, nameof(queryText));

        _definition = _definition with
        {
            LiteralQuery = queryText,
            QueryFactory = null,
        };

        return this;
    }

    /// <inheritdoc/>
    public IRetrievalBuilder<TState, TCollection> TopK(int count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1, nameof(count));

        _definition = _definition with { TopK = count };
        return this;
    }

    /// <inheritdoc/>
    public IRetrievalBuilder<TState, TCollection> MinRelevance(decimal threshold)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(threshold, 0.0m, nameof(threshold));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(threshold, 1.0m, nameof(threshold));

        _definition = _definition with { MinRelevance = threshold };
        return this;
    }

    /// <inheritdoc/>
    public IRetrievalBuilder<TState, TCollection> Filter(string key, object value)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        var filter = RetrievalFilter.Static(key, value);
        _definition = _definition.WithFilter(filter);
        return this;
    }

    /// <inheritdoc/>
    public IRetrievalBuilder<TState, TCollection> Filter<TValue>(
        string key,
        Func<TState, TValue> valueSelector)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(valueSelector, nameof(valueSelector));

        var filter = RetrievalFilter.Dynamic(key, valueSelector);
        _definition = _definition.WithFilter(filter);
        return this;
    }
}
