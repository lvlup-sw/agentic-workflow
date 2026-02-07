// =============================================================================
// <copyright file="TaskCompleted.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Orchestration.Ledgers;

namespace Agentic.Workflow.Events;

/// <summary>
/// Event raised when a task in the task ledger is marked as complete.
/// </summary>
/// <remarks>
/// <para>
/// This event is raised after the orchestrator reviews the execution result
/// and updates the task status. It captures the final status and result.
/// </para>
/// </remarks>
/// <param name="WorkflowId">The unique identifier for the workflow.</param>
/// <param name="TaskId">The unique identifier for the completed task.</param>
/// <param name="FinalStatus">The final status of the task.</param>
/// <param name="Result">The result produced by the task, if any.</param>
/// <param name="Timestamp">The timestamp when the task was marked complete.</param>
public sealed record TaskCompleted(
    Guid WorkflowId,
    string TaskId,
    WorkflowTaskStatus FinalStatus,
    string? Result,
    DateTimeOffset Timestamp) : IWorkflowEvent;