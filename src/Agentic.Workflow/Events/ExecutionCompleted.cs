// =============================================================================
// <copyright file="ExecutionCompleted.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Events;

/// <summary>
/// Event raised when an executor successfully completes a task.
/// </summary>
/// <remarks>
/// <para>
/// This event captures the successful result of task execution, including
/// the output produced and confidence level of the executor.
/// </para>
/// </remarks>
/// <param name="WorkflowId">The unique identifier for the workflow.</param>
/// <param name="TaskId">The unique identifier for the completed task.</param>
/// <param name="ExecutorId">The identifier of the executor that completed the task.</param>
/// <param name="Result">The output produced by the executor.</param>
/// <param name="Confidence">The executor's confidence in the result (0.0 to 1.0).</param>
/// <param name="Duration">The time taken to complete execution.</param>
/// <param name="Timestamp">The timestamp when execution completed.</param>
public sealed record ExecutionCompleted(
    Guid WorkflowId,
    string TaskId,
    string ExecutorId,
    string Result,
    double Confidence,
    TimeSpan Duration,
    DateTimeOffset Timestamp) : IWorkflowEvent;
