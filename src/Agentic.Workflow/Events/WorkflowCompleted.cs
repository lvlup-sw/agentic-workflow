// =============================================================================
// <copyright file="WorkflowCompleted.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Events;

/// <summary>
/// Event raised when a workflow reaches its terminal state.
/// </summary>
/// <remarks>
/// <para>
/// This is the final event in any workflow stream. It captures the overall
/// outcome, the final answer to the original request, and the total duration.
/// </para>
/// <para>
/// The <see cref="FinalAnswer"/> synthesizes results from all completed
/// tasks into a cohesive response to the original user request.
/// </para>
/// </remarks>
/// <param name="WorkflowId">The unique identifier for this workflow.</param>
/// <param name="Outcome">The final outcome of the workflow.</param>
/// <param name="FinalAnswer">The synthesized answer to the original request.</param>
/// <param name="TotalDuration">The total time from workflow start to completion.</param>
/// <param name="Timestamp">The timestamp when the workflow completed.</param>
public sealed record WorkflowCompleted(
    Guid WorkflowId,
    WorkflowOutcome Outcome,
    string FinalAnswer,
    TimeSpan TotalDuration,
    DateTimeOffset Timestamp) : IWorkflowEvent;