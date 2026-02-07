// =============================================================================
// <copyright file="SignalType.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Agentic.Workflow.Orchestration.Ledgers;

/// <summary>
/// Defines the types of signals an executor can emit during workflow execution.
/// </summary>
/// <remarks>
/// <para>
/// Signals are the structured way executors communicate their state and needs
/// back to the workflow orchestrator. The signal type determines how the
/// orchestrator responds.
/// </para>
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SignalType
{
    /// <summary>
    /// The executor completed successfully with results.
    /// </summary>
    [JsonStringEnumMemberName("success")]
    Success,

    /// <summary>
    /// The executor failed and cannot continue.
    /// </summary>
    [JsonStringEnumMemberName("failure")]
    Failure,

    /// <summary>
    /// The executor needs human assistance to proceed.
    /// </summary>
    [JsonStringEnumMemberName("help_needed")]
    HelpNeeded,

    /// <summary>
    /// The executor is blocked by a dependency.
    /// </summary>
    [JsonStringEnumMemberName("blocked")]
    Blocked,

    /// <summary>
    /// The executor is making progress and continuing.
    /// </summary>
    [JsonStringEnumMemberName("in_progress")]
    InProgress
}