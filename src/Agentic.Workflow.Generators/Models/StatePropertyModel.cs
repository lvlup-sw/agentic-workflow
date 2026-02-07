// -----------------------------------------------------------------------
// <copyright file="StatePropertyModel.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Polyfills;
using Agentic.Workflow.Generators.Utilities;

namespace Agentic.Workflow.Generators.Models;

/// <summary>
/// Represents a property within a workflow state type for reducer generation.
/// </summary>
/// <param name="Name">The property name (e.g., "Items").</param>
/// <param name="TypeName">The property type name (e.g., "IReadOnlyList&lt;string&gt;").</param>
/// <param name="Kind">The property kind determining reducer behavior.</param>
internal sealed record StatePropertyModel(
    string Name,
    string TypeName,
    StatePropertyKind Kind)
{
    /// <summary>
    /// Creates a new <see cref="StatePropertyModel"/> with validation.
    /// </summary>
    /// <param name="name">The property name. Must be a valid C# identifier.</param>
    /// <param name="typeName">The property type name. Cannot be null or whitespace.</param>
    /// <param name="kind">The property kind determining reducer behavior.</param>
    /// <returns>A validated <see cref="StatePropertyModel"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static StatePropertyModel Create(
        string name,
        string typeName,
        StatePropertyKind kind)
    {
        ThrowHelper.ThrowIfNull(name, nameof(name));
        IdentifierValidator.ValidateIdentifier(name, nameof(name));
        ThrowHelper.ThrowIfNullOrWhiteSpace(typeName, nameof(typeName));

        return new StatePropertyModel(
            Name: name,
            TypeName: typeName,
            Kind: kind);
    }
}