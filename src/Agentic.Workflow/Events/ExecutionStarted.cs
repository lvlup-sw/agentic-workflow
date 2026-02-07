// =============================================================================
// <copyright file="ExecutionStarted.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Events;

/// <summary>
/// Event raised when an executor begins execution of a task.
/// </summary>
/// <remarks>
/// <para>
/// This event marks the beginning of task execution by a specific executor.
/// It is followed by either <see cref="ExecutionCompleted"/> on success or
/// <see cref="ExecutionFailed"/> on failure.
/// </para>
/// </remarks>
/// <param name="WorkflowId">The unique identifier for the workflow.</param>
/// <param name="TaskId">The unique identifier for the task being executed.</param>
/// <param name="ExecutorId">The identifier of the executor handling the task.</param>
/// <param name="Timestamp">The timestamp when execution started.</param>
public sealed record ExecutionStarted(
    Guid WorkflowId,
    string TaskId,
    string ExecutorId,
    DateTimeOffset Timestamp) : IWorkflowEvent;