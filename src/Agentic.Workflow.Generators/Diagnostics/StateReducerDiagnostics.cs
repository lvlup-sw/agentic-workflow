// -----------------------------------------------------------------------
// <copyright file="StateReducerDiagnostics.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Generators.Diagnostics;

/// <summary>
/// Defines diagnostic descriptors for the state reducer source generator.
/// </summary>
internal static class StateReducerDiagnostics
{
    /// <summary>
    /// Diagnostic category for all state reducer generator diagnostics.
    /// </summary>
    public const string Category = "Agentic.Workflow.StateReducer";

    /// <summary>
    /// AGSR001: [Append] attribute applied to non-collection property.
    /// </summary>
    /// <remarks>
    /// Reported when the [Append] attribute is applied to a property that is not a collection type.
    /// </remarks>
    public static readonly DiagnosticDescriptor AppendOnNonCollection = new(
        id: "AGSR001",
        title: "Append attribute on non-collection property",
        messageFormat: "Property '{0}' has [Append] attribute but type '{1}' does not implement IEnumerable<T>",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The [Append] attribute can only be applied to properties that implement IEnumerable<T>, such as IReadOnlyList<T>, List<T>, or arrays.");

    /// <summary>
    /// AGSR002: [Merge] attribute applied to non-dictionary property.
    /// </summary>
    /// <remarks>
    /// Reported when the [Merge] attribute is applied to a property that is not a dictionary type.
    /// </remarks>
    public static readonly DiagnosticDescriptor MergeOnNonDictionary = new(
        id: "AGSR002",
        title: "Merge attribute on non-dictionary property",
        messageFormat: "Property '{0}' has [Merge] attribute but type '{1}' is not a dictionary type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The [Merge] attribute can only be applied to properties that implement IReadOnlyDictionary<TKey, TValue> or IDictionary<TKey, TValue>.");
}
