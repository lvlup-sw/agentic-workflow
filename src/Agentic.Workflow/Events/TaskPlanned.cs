// =============================================================================
// <copyright file="TaskPlanned.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Events;

/// <summary>
/// Event raised when the orchestrator plans tasks from a user request.
/// </summary>
/// <remarks>
/// <para>
/// This event captures the decomposition of the original user request into
/// discrete, executable tasks. It is raised during the planning phase.
/// </para>
/// </remarks>
/// <param name="WorkflowId">The unique identifier for the workflow.</param>
/// <param name="TaskId">The unique identifier for the planned task.</param>
/// <param name="Description">The human-readable description of the task.</param>
/// <param name="Priority">The priority of the task (higher = more important).</param>
/// <param name="Dependencies">The IDs of tasks that must complete before this one.</param>
/// <param name="Timestamp">The timestamp when the task was planned.</param>
public sealed record TaskPlanned(
    Guid WorkflowId,
    string TaskId,
    string Description,
    int Priority,
    IReadOnlyList<string> Dependencies,
    DateTimeOffset Timestamp) : IWorkflowEvent;