// -----------------------------------------------------------------------
// <copyright file="StateModel.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Polyfills;
using Agentic.Workflow.Generators.Utilities;

namespace Agentic.Workflow.Generators.Models;

/// <summary>
/// Complete state model extracted for reducer code generation.
/// </summary>
/// <remarks>
/// This is the intermediate representation used by the StateReducerEmitter
/// to generate state reducer classes.
/// </remarks>
/// <param name="TypeName">The state type name (e.g., "OrderState").</param>
/// <param name="Namespace">The containing namespace.</param>
/// <param name="Properties">The list of properties with their reducer semantics.</param>
internal sealed record StateModel(
    string TypeName,
    string Namespace,
    IReadOnlyList<StatePropertyModel> Properties)
{
    /// <summary>
    /// Gets the derived reducer class name.
    /// </summary>
    public string ReducerClassName => $"{TypeName}Reducer";

    /// <summary>
    /// Creates a new <see cref="StateModel"/> with validation.
    /// </summary>
    /// <param name="typeName">The state type name. Must be a valid C# identifier.</param>
    /// <param name="namespace">The containing namespace. Cannot be null or whitespace.</param>
    /// <param name="properties">The list of properties with their reducer semantics.</param>
    /// <returns>A validated <see cref="StateModel"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static StateModel Create(
        string typeName,
        string @namespace,
        IReadOnlyList<StatePropertyModel> properties)
    {
        ThrowHelper.ThrowIfNull(typeName, nameof(typeName));
        IdentifierValidator.ValidateIdentifier(typeName, nameof(typeName));
        ThrowHelper.ThrowIfNullOrWhiteSpace(@namespace, nameof(@namespace));
        ThrowHelper.ThrowIfNull(properties, nameof(properties));

        return new StateModel(
            TypeName: typeName,
            Namespace: @namespace,
            Properties: properties);
    }
}
