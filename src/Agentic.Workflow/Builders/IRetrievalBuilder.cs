// =============================================================================
// <copyright file="IRetrievalBuilder.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Builders;

/// <summary>
/// Fluent builder interface for configuring RAG retrieval parameters.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <typeparam name="TCollection">The collection type marker for DI resolution.</typeparam>
/// <remarks>
/// <para>
/// Retrieval builders configure semantic search queries against typed collections:
/// <list type="bullet">
///   <item><description>Query: The search query (dynamic or static)</description></item>
///   <item><description>TopK: Maximum documents to retrieve</description></item>
///   <item><description>MinRelevance: Relevance score threshold</description></item>
///   <item><description>Filter: Metadata filters for narrowing results</description></item>
/// </list>
/// </para>
/// <para>
/// Example usage:
/// <code>
/// .FromRetrieval&lt;ResearchLibrary&gt;(r => r
///     .Query(s => s.ResearchQuestion)
///     .TopK(5)
///     .MinRelevance(0.8m)
///     .Filter("documentType", "research-paper")
///     .Filter("year", s => s.TargetYear))
/// </code>
/// </para>
/// </remarks>
public interface IRetrievalBuilder<TState, TCollection>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Sets the query text using a dynamic state selector.
    /// </summary>
    /// <param name="queryFactory">Function to extract query from state.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="queryFactory"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Dynamic queries enable context-aware retrieval based on workflow state.
    /// The query is evaluated at runtime when the step executes.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// .Query(s => s.UserQuestion)
    /// .Query(s => $"Find documents about {s.Topic} in {s.Domain}")
    /// </code>
    /// </para>
    /// </remarks>
    IRetrievalBuilder<TState, TCollection> Query(Func<TState, string> queryFactory);

    /// <summary>
    /// Sets the query text using a static literal.
    /// </summary>
    /// <param name="queryText">The static query text.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="queryText"/> is null.
    /// </exception>
    /// <remarks>
    /// Use static queries when retrieval is not dependent on workflow state.
    /// </remarks>
    IRetrievalBuilder<TState, TCollection> Query(string queryText);

    /// <summary>
    /// Sets the maximum number of documents to retrieve.
    /// </summary>
    /// <param name="count">The maximum document count.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="count"/> is less than 1.
    /// </exception>
    /// <remarks>
    /// Defaults to 5 if not specified. Higher values provide more context
    /// but increase token usage and may dilute relevance.
    /// </remarks>
    IRetrievalBuilder<TState, TCollection> TopK(int count);

    /// <summary>
    /// Sets the minimum relevance score threshold.
    /// </summary>
    /// <param name="threshold">The minimum relevance (0.0 to 1.0).</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="threshold"/> is not between 0.0 and 1.0.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Results with relevance below this threshold are excluded.
    /// Defaults to 0.7 (70% similarity) if not specified.
    /// </para>
    /// <para>
    /// Higher thresholds return fewer but more relevant results.
    /// Lower thresholds return more results with potential noise.
    /// </para>
    /// </remarks>
    IRetrievalBuilder<TState, TCollection> MinRelevance(decimal threshold);

    /// <summary>
    /// Adds a static metadata filter.
    /// </summary>
    /// <param name="key">The metadata key to filter on.</param>
    /// <param name="value">The required value.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="key"/> or <paramref name="value"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Filters are applied as exact-match predicates on document metadata.
    /// Multiple filters are combined with AND logic.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// .Filter("category", "research-paper")
    /// .Filter("year", 2024)
    /// </code>
    /// </para>
    /// </remarks>
    IRetrievalBuilder<TState, TCollection> Filter(string key, object value);

    /// <summary>
    /// Adds a dynamic metadata filter using state selector.
    /// </summary>
    /// <typeparam name="TValue">The filter value type.</typeparam>
    /// <param name="key">The metadata key to filter on.</param>
    /// <param name="valueSelector">Function to extract filter value from state.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="key"/> or <paramref name="valueSelector"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Dynamic filters enable state-dependent narrowing of results.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// .Filter("department", s => s.UserDepartment)
    /// .Filter("author", s => s.PreferredAuthor)
    /// </code>
    /// </para>
    /// </remarks>
    IRetrievalBuilder<TState, TCollection> Filter<TValue>(
        string key,
        Func<TState, TValue> valueSelector);
}