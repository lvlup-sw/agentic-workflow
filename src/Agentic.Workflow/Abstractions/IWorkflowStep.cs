// =============================================================================
// <copyright file="IWorkflowStep.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Steps;

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Contract for workflow step implementations.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <remarks>
/// <para>
/// Workflow steps encapsulate discrete units of work within a workflow:
/// <list type="bullet">
///   <item><description>Execute business logic and state transitions</description></item>
///   <item><description>Receive immutable state and return updated state</description></item>
///   <item><description>Access execution context for correlation and metadata</description></item>
///   <item><description>Support cancellation for graceful shutdown</description></item>
/// </list>
/// </para>
/// <para>
/// Step implementations should be stateless and registered with DI for
/// automatic injection of dependencies.
/// </para>
/// </remarks>
public interface IWorkflowStep<TState>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Executes the workflow step asynchronously.
    /// </summary>
    /// <param name="state">The current workflow state.</param>
    /// <param name="context">The step execution context.</param>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
    /// <returns>A task representing the step result with updated state.</returns>
    /// <remarks>
    /// <para>
    /// Implementations should:
    /// <list type="bullet">
    ///   <item><description>Not mutate the input state - return new state via StepResult</description></item>
    ///   <item><description>Honor the cancellation token for long-running operations</description></item>
    ///   <item><description>Include confidence scores for agent steps requiring review</description></item>
    ///   <item><description>Add metadata for observability (execution time, model used, etc.)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Task<StepResult<TState>> ExecuteAsync(
        TState state,
        StepContext context,
        CancellationToken cancellationToken);
}
