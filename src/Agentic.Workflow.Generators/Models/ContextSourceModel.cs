// -----------------------------------------------------------------------
// <copyright file="ContextSourceModel.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Agentic.Workflow.Generators.Models;

/// <summary>
/// Base type for context source models used in code generation.
/// </summary>
/// <remarks>
/// Context sources define how to obtain runtime context for agent steps.
/// Derived types represent different context acquisition strategies:
/// state access, RAG retrieval, or literal text.
/// </remarks>
internal abstract record ContextSourceModel;

/// <summary>
/// Context source that extracts a value from workflow state.
/// </summary>
/// <param name="PropertyPath">The property path expression (e.g., "CustomerName" or "Order.Summary").</param>
/// <param name="PropertyType">The type of the property (e.g., "string", "Order").</param>
/// <param name="AccessExpression">The full access expression (e.g., "state.CustomerName").</param>
internal sealed record StateContextSourceModel(
    string PropertyPath,
    string PropertyType,
    string AccessExpression) : ContextSourceModel;

/// <summary>
/// Context source that retrieves documents from a RAG collection.
/// </summary>
/// <param name="CollectionTypeName">The collection type name for DI resolution.</param>
/// <param name="QueryExpression">The dynamic query expression (state-dependent), or null if using LiteralQuery.</param>
/// <param name="LiteralQuery">The static query text, or null if using QueryExpression.</param>
/// <param name="TopK">The maximum number of documents to retrieve.</param>
/// <param name="MinRelevance">The minimum relevance score threshold.</param>
/// <param name="Filters">The metadata filters to apply.</param>
internal sealed record RetrievalContextSourceModel(
    string CollectionTypeName,
    string? QueryExpression,
    string? LiteralQuery,
    int TopK,
    decimal MinRelevance,
    IReadOnlyList<RetrievalFilterModel> Filters) : ContextSourceModel;

/// <summary>
/// A metadata filter for RAG retrieval.
/// </summary>
/// <param name="Key">The metadata key to filter on.</param>
/// <param name="StaticValue">The static filter value, or null if using ValueExpression.</param>
/// <param name="ValueExpression">The dynamic value expression (state-dependent), or null if using StaticValue.</param>
internal sealed record RetrievalFilterModel(
    string Key,
    string? StaticValue,
    string? ValueExpression)
{
    /// <summary>
    /// Gets a value indicating whether this is a static filter.
    /// </summary>
    public bool IsStatic => StaticValue is not null;
}

/// <summary>
/// Context source with a static literal string.
/// </summary>
/// <param name="Value">The literal context value.</param>
internal sealed record LiteralContextSourceModel(string Value) : ContextSourceModel;