// =============================================================================
// <copyright file="IFailureBuilder.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Builders;

/// <summary>
/// Fluent builder interface for constructing failure handler paths within a workflow.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <remarks>
/// <para>
/// Failure builders allow defining steps for error recovery:
/// <code>
/// .OnFailure(failure => failure
///     .Then&lt;LogFailure&gt;()
///     .Then&lt;NotifyAdmin&gt;()
///     .Complete())
/// </code>
/// </para>
/// <para>
/// By default, failure handlers rejoin the main flow after recovery.
/// Use <see cref="Complete"/> to terminate a failure path without rejoining.
/// </para>
/// </remarks>
public interface IFailureBuilder<TState>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Adds a step to this failure handler path.
    /// </summary>
    /// <typeparam name="TStep">The step implementation type.</typeparam>
    /// <returns>The builder for fluent chaining.</returns>
    IFailureBuilder<TState> Then<TStep>()
        where TStep : class, IWorkflowStep<TState>;

    /// <summary>
    /// Adds a step to this failure handler path with an instance name.
    /// </summary>
    /// <typeparam name="TStep">The step implementation type.</typeparam>
    /// <param name="instanceName">
    /// The instance name for this step. Enables reusing the same step type
    /// in different contexts with distinct identities.
    /// </param>
    /// <returns>The builder for fluent chaining.</returns>
    IFailureBuilder<TState> Then<TStep>(string instanceName)
        where TStep : class, IWorkflowStep<TState>;

    /// <summary>
    /// Marks this failure handler as terminal (does not rejoin the main flow).
    /// </summary>
    /// <remarks>
    /// When a failure handler calls Complete(), it terminates the workflow
    /// and does not transition back to the main flow.
    /// </remarks>
    void Complete();
}
