// =============================================================================
// <copyright file="WorkflowTaskStatus.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Agentic.Workflow.Orchestration.Ledgers;

/// <summary>
/// Defines the lifecycle status of a task in the task ledger.
/// </summary>
/// <remarks>
/// <para>
/// Named WorkflowTaskStatus to avoid collision with System.Threading.Tasks.TaskStatus.
/// </para>
/// <para>
/// Tasks progress through this lifecycle as executors work on them.
/// The orchestrator uses this status to determine which tasks are available
/// for delegation and when the workflow is complete.
/// </para>
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkflowTaskStatus
{
    /// <summary>
    /// Task is waiting to be started.
    /// </summary>
    /// <remarks>
    /// Initial state for all tasks. Available for delegation when dependencies are satisfied.
    /// </remarks>
    [JsonStringEnumMemberName("pending")]
    Pending,

    /// <summary>
    /// Task has been delegated to an executor and is being executed.
    /// </summary>
    /// <remarks>
    /// The task is actively being worked on. Only one executor should be
    /// working on a task at a time.
    /// </remarks>
    [JsonStringEnumMemberName("in_progress")]
    InProgress,

    /// <summary>
    /// Task has been completed successfully.
    /// </summary>
    /// <remarks>
    /// The executor returned a SUCCESS signal with confidence above threshold.
    /// The task's results are available for dependent tasks.
    /// </remarks>
    [JsonStringEnumMemberName("completed")]
    Completed,

    /// <summary>
    /// Task has failed after exhausting retry attempts.
    /// </summary>
    /// <remarks>
    /// The task cannot be completed with available resources or approaches.
    /// The workflow may continue with reduced scope or fail entirely depending
    /// on the task's criticality.
    /// </remarks>
    [JsonStringEnumMemberName("failed")]
    Failed,

    /// <summary>
    /// Task was intentionally skipped.
    /// </summary>
    /// <remarks>
    /// Used when a task becomes unnecessary due to changed circumstances,
    /// scope reduction during resource scarcity, or user intervention.
    /// </remarks>
    [JsonStringEnumMemberName("skipped")]
    Skipped
}