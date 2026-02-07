// =============================================================================
// <copyright file="Workflow.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Builders;

/// <summary>
/// Static entry point for creating workflow definitions using the fluent DSL.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <remarks>
/// <para>
/// Usage:
/// <code>
/// var workflow = Workflow&lt;OrderState&gt;
///     .Create("process-order")
///     .StartWith&lt;ValidateOrder&gt;()
///     .Then&lt;ProcessPayment&gt;()
///     .Finally&lt;SendConfirmation&gt;();
/// </code>
/// </para>
/// </remarks>
public static class Workflow<TState>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Creates a new workflow builder with the specified name.
    /// </summary>
    /// <param name="name">The workflow name (used for identification and code generation).</param>
    /// <returns>A workflow builder for fluent configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or whitespace.</exception>
    public static IWorkflowBuilder<TState> Create(string name)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return new WorkflowBuilder<TState>(name);
    }
}