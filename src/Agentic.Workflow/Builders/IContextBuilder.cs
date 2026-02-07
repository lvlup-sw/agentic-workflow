// =============================================================================
// <copyright file="IContextBuilder.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Linq.Expressions;

namespace Agentic.Workflow.Builders;

/// <summary>
/// Fluent builder interface for assembling RAG context from multiple sources.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <remarks>
/// <para>
/// Context builders enable declarative specification of context sources:
/// <list type="bullet">
///   <item><description>FromState: Extract values from workflow state</description></item>
///   <item><description>FromRetrieval: Semantic search against typed collections</description></item>
///   <item><description>FromLiteral: Static context strings</description></item>
/// </list>
/// </para>
/// <para>
/// Sources are assembled in order, with later sources appended after earlier ones.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// .WithContext(ctx => ctx
///     .FromState(s => s.CustomerName)
///     .FromRetrieval&lt;StyleCardCollection&gt;(r => r
///         .Query(s => s.DesiredStyle)
///         .TopK(3))
///     .FromLiteral("Follow brand guidelines."))
/// </code>
/// </para>
/// </remarks>
public interface IContextBuilder<TState>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Adds a state property value as context source.
    /// </summary>
    /// <typeparam name="TValue">The property value type.</typeparam>
    /// <param name="selector">Expression selecting the state property.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="selector"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The selected value is converted to string via ToString() or JSON serialization.
    /// For complex objects, consider using a projection to a formatted string property.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// .FromState(s => s.OrderSummary)
    /// .FromState(s => $"Customer: {s.CustomerName}")
    /// </code>
    /// </para>
    /// </remarks>
    IContextBuilder<TState> FromState<TValue>(Expression<Func<TState, TValue>> selector);

    /// <summary>
    /// Adds a RAG collection retrieval as context source.
    /// </summary>
    /// <typeparam name="TCollection">
    /// The collection type marker. Used for DI resolution of IRagCollection&lt;TDocument&gt;.
    /// </typeparam>
    /// <param name="configure">Action to configure retrieval parameters.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configure"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The collection type is used as a marker for dependency injection.
    /// Register collections via AddRagCollection&lt;TDocument&gt;().
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// .FromRetrieval&lt;ResearchLibrary&gt;(r => r
    ///     .Query(s => s.ResearchQuestion)
    ///     .TopK(5)
    ///     .MinRelevance(0.75m)
    ///     .Filter("category", s => s.DocumentCategory))
    /// </code>
    /// </para>
    /// </remarks>
    IContextBuilder<TState> FromRetrieval<TCollection>(
        Action<IRetrievalBuilder<TState, TCollection>> configure);

    /// <summary>
    /// Adds a static literal string as context source.
    /// </summary>
    /// <param name="literal">The literal context string.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="literal"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Literal context is useful for:
    /// <list type="bullet">
    ///   <item><description>System instructions: "Always cite sources."</description></item>
    ///   <item><description>Formatting guidance: "Respond in markdown."</description></item>
    ///   <item><description>Domain constraints: "Use formal language."</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    IContextBuilder<TState> FromLiteral(string literal);
}