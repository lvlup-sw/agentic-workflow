// =============================================================================
// <copyright file="ContextBuilder.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Linq.Expressions;

using Strategos.Definitions;

namespace Strategos.Builders;

/// <summary>
/// Internal implementation of the context builder.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
internal sealed class ContextBuilder<TState> : IContextBuilder<TState>
    where TState : class, IWorkflowState
{
    private ContextDefinition _definition = ContextDefinition.Empty;

    /// <summary>
    /// Gets the built context definition.
    /// </summary>
    internal ContextDefinition Definition => _definition;

    /// <inheritdoc/>
    public IContextBuilder<TState> FromState<TValue>(Expression<Func<TState, TValue>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));

        var propertyPath = selector.Body.ToString();
        var compiledSelector = selector.Compile();

        var source = new StateContextSource
        {
            PropertyPath = propertyPath,
            Selector = compiledSelector,
            ValueType = typeof(TValue),
        };

        _definition = _definition.WithSource(source);
        return this;
    }

    /// <inheritdoc/>
    public IContextBuilder<TState> FromRetrieval<TCollection>(
        Action<IRetrievalBuilder<TState, TCollection>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        var retrievalBuilder = new RetrievalBuilder<TState, TCollection>();
        configure(retrievalBuilder);

        var source = new RetrievalContextSource
        {
            CollectionType = typeof(TCollection),
            Retrieval = retrievalBuilder.Definition,
        };

        _definition = _definition.WithSource(source);
        return this;
    }

    /// <inheritdoc/>
    public IContextBuilder<TState> FromLiteral(string literal)
    {
        ArgumentNullException.ThrowIfNull(literal, nameof(literal));

        var source = LiteralContextSource.Create(literal);
        _definition = _definition.WithSource(source);
        return this;
    }
}
