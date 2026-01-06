// =============================================================================
// <copyright file="IForkJoinBuilder.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Builders;

/// <summary>
/// Intermediate builder for completing a fork with a join step.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <remarks>
/// <para>
/// This builder is returned by <see cref="IWorkflowBuilder{TState}.Fork"/> and
/// requires a join step to be specified before the workflow can continue:
/// <code>
/// .Fork(
///     path => path.Then&lt;ProcessPayment&gt;(),
///     path => path.Then&lt;ReserveInventory&gt;())
/// .Join&lt;SynthesizeResults&gt;()
/// .Then&lt;SendConfirmation&gt;()
/// </code>
/// </para>
/// </remarks>
public interface IForkJoinBuilder<TState>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Specifies the join step that merges results from all fork paths.
    /// </summary>
    /// <typeparam name="TJoinStep">The join step implementation type.</typeparam>
    /// <returns>The workflow builder for fluent chaining.</returns>
    /// <remarks>
    /// <para>
    /// The join step executes only after all fork paths reach a terminal status
    /// (Success, Failed, or FailedWithRecovery).
    /// </para>
    /// <para>
    /// The join step receives a <see cref="Steps.ForkContext{TState}"/> containing
    /// the results from each path, including their statuses and state values.
    /// </para>
    /// </remarks>
    IWorkflowBuilder<TState> Join<TJoinStep>()
        where TJoinStep : class, IWorkflowStep<TState>;
}
