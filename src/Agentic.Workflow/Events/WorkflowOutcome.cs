// =============================================================================
// <copyright file="WorkflowOutcome.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Agentic.Workflow.Events;

/// <summary>
/// Defines the possible terminal outcomes for a workflow execution.
/// </summary>
/// <remarks>
/// <para>
/// This enum is used in <see cref="WorkflowCompleted"/> events to indicate
/// how the workflow terminated.
/// </para>
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkflowOutcome
{
    /// <summary>
    /// The workflow completed all tasks successfully.
    /// </summary>
    /// <remarks>
    /// All tasks in the TaskLedger have corresponding SUCCESS entries in the ProgressLedger.
    /// </remarks>
    [JsonStringEnumMemberName("success")]
    Success,

    /// <summary>
    /// The workflow failed after exhausting recovery attempts.
    /// </summary>
    /// <remarks>
    /// The workflow encountered unrecoverable errors or exceeded maximum reset attempts.
    /// </remarks>
    [JsonStringEnumMemberName("failed")]
    Failed,

    /// <summary>
    /// The workflow was cancelled before completion.
    /// </summary>
    /// <remarks>
    /// Cancellation may occur due to user request, budget exhaustion, or external termination.
    /// </remarks>
    [JsonStringEnumMemberName("cancelled")]
    Cancelled,

    /// <summary>
    /// The workflow was rejected by a human approver.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The workflow requested human-in-the-loop approval and the human chose to reject
    /// the action or workflow continuation. This is distinct from failure (automatic)
    /// and cancellation (user request) as it represents an explicit human decision.
    /// </para>
    /// </remarks>
    [JsonStringEnumMemberName("rejected")]
    Rejected,

    /// <summary>
    /// The workflow timed out while awaiting human approval.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The workflow requested human-in-the-loop approval but no response was received
    /// within the configured timeout period. This triggers automatic workflow failure.
    /// </para>
    /// </remarks>
    [JsonStringEnumMemberName("timed_out")]
    TimedOut
}
