// =============================================================================
// <copyright file="SpecialistState.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Strategos.Agents.Models;

/// <summary>
/// Defines the nested hierarchical state machine (HSM) states for specialist agents.
/// </summary>
/// <remarks>
/// Each specialist type (WebSurfer, Analyst, Coder, FileSurfer) shares this common state space.
/// The Orchestrator manages meta-level transitions between specialists, while specialists
/// manage their internal workflows using these states.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SpecialistState
{
    /// <summary>
    /// Receiving context and task assignment from the Orchestrator.
    /// </summary>
    /// <remarks>
    /// Initial state when a specialist is activated. The specialist parses the
    /// delegated task, available context, and any constraints.
    /// </remarks>
    [JsonStringEnumMemberName("receiving")]
    Receiving,

    /// <summary>
    /// Analyzing the task and determining the approach.
    /// </summary>
    /// <remarks>
    /// The specialist reasons about the best strategy to accomplish the task,
    /// considering available tools and prior context from the progress ledger.
    /// </remarks>
    [JsonStringEnumMemberName("reasoning")]
    Reasoning,

    /// <summary>
    /// Generating Python code to execute the planned approach.
    /// </summary>
    /// <remarks>
    /// In the "Everything is a Coder" architecture, all specialists generate
    /// Python code as their primary action mechanism.
    /// </remarks>
    [JsonStringEnumMemberName("generating")]
    Generating,

    /// <summary>
    /// Executing generated code via the Code Execution Bridge.
    /// </summary>
    /// <remarks>
    /// Code is submitted to the ControlPlane for execution in the Sandbox.
    /// The specialist awaits results while monitoring progress via SSE streaming.
    /// </remarks>
    [JsonStringEnumMemberName("executing")]
    Executing,

    /// <summary>
    /// Waiting for external resources or tool callbacks to complete.
    /// </summary>
    /// <remarks>
    /// During execution, the Sandbox may issue tool callbacks to the ControlPlane.
    /// This state represents waiting for those hairpin calls to return.
    /// </remarks>
    [JsonStringEnumMemberName("waiting")]
    Waiting,

    /// <summary>
    /// Interpreting execution results and extracting insights.
    /// </summary>
    /// <remarks>
    /// The specialist analyzes stdout, stderr, and any artifacts produced
    /// by code execution to determine success and extract relevant information.
    /// </remarks>
    [JsonStringEnumMemberName("interpreting")]
    Interpreting,

    /// <summary>
    /// Signaling completion status back to the Orchestrator.
    /// </summary>
    /// <remarks>
    /// Terminal state where the specialist emits a signal (SUCCESS, FAILURE, or HELP_NEEDED)
    /// and returns control to the Orchestrator for the next iteration.
    /// </remarks>
    [JsonStringEnumMemberName("signaling")]
    Signaling
}
