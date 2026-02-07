// =============================================================================
// <copyright file="ExecutorState.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Agentic.Workflow.Orchestration.Ledgers;

/// <summary>
/// Defines the possible states of an executor during workflow execution.
/// </summary>
/// <remarks>
/// <para>
/// Executor state tracks the lifecycle of individual executor invocations
/// within a workflow. This enables monitoring and debugging of workflow
/// progress.
/// </para>
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExecutorState
{
    /// <summary>
    /// The executor is idle and awaiting work.
    /// </summary>
    [JsonStringEnumMemberName("idle")]
    Idle,

    /// <summary>
    /// The executor is actively executing a task.
    /// </summary>
    [JsonStringEnumMemberName("executing")]
    Executing,

    /// <summary>
    /// The executor is emitting a signal to the orchestrator.
    /// </summary>
    [JsonStringEnumMemberName("signaling")]
    Signaling,

    /// <summary>
    /// The executor has completed its task successfully.
    /// </summary>
    [JsonStringEnumMemberName("complete")]
    Complete,

    /// <summary>
    /// The executor has failed and cannot continue.
    /// </summary>
    [JsonStringEnumMemberName("failed")]
    Failed
}