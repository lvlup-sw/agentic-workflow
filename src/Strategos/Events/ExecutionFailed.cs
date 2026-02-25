// =============================================================================
// <copyright file="ExecutionFailed.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Events;

/// <summary>
/// Event raised when an executor fails to complete a task.
/// </summary>
/// <remarks>
/// <para>
/// This event captures the failure of task execution, including the reason
/// for failure and whether the failure is recoverable.
/// </para>
/// </remarks>
/// <param name="WorkflowId">The unique identifier for the workflow.</param>
/// <param name="TaskId">The unique identifier for the failed task.</param>
/// <param name="ExecutorId">The identifier of the executor that failed.</param>
/// <param name="Reason">The reason for the failure.</param>
/// <param name="IsRecoverable">Whether the failure can be recovered from.</param>
/// <param name="Duration">The time elapsed before failure.</param>
/// <param name="Timestamp">The timestamp when the failure occurred.</param>
public sealed record ExecutionFailed(
    Guid WorkflowId,
    string TaskId,
    string ExecutorId,
    string Reason,
    bool IsRecoverable,
    TimeSpan Duration,
    DateTimeOffset Timestamp) : IWorkflowEvent;
