// =============================================================================
// <copyright file="StepDelegate.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;

namespace Strategos.Steps;

/// <summary>
/// Delegate for inline lambda step implementations.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <param name="state">The current workflow state.</param>
/// <param name="context">The step execution context.</param>
/// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
/// <returns>A task representing the step result with updated state.</returns>
/// <remarks>
/// <para>
/// Lambda steps allow inline step definitions without creating separate classes:
/// <code>
/// .Then("ProcessData", async (state, context, ct) =>
/// {
///     var updatedState = state with { Processed = true };
///     return StepResult&lt;MyState&gt;.Success(updatedState);
/// })
/// </code>
/// </para>
/// <para>
/// Lambda steps are useful for simple transformations and one-off logic.
/// For complex or reusable steps, prefer class-based implementations.
/// </para>
/// </remarks>
public delegate Task<StepResult<TState>> StepDelegate<TState>(
    TState state,
    StepContext context,
    CancellationToken cancellationToken)
    where TState : class, IWorkflowState;
