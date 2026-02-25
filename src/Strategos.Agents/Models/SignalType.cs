// =============================================================================
// <copyright file="SignalType.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Strategos.Agents.Models;

/// <summary>
/// Defines the signal types used by specialists to communicate completion status to the Orchestrator.
/// </summary>
/// <remarks>
/// <para>
/// All specialists use a unified signaling protocol to return control to the Orchestrator.
/// The signal type determines how the Orchestrator responds and updates the progress ledger.
/// </para>
/// <para>
/// This enables structured communication between the inner loop (specialists) and
/// outer loop (Orchestrator) of the Magentic-One pattern.
/// </para>
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SignalType
{
    /// <summary>
    /// Task completed successfully with confidence above threshold.
    /// </summary>
    /// <remarks>
    /// Orchestrator response: Update progress ledger, mark task complete, select next task.
    /// The signal should include result, confidence score, any artifacts, and optional
    /// suggestion for the next action.
    /// </remarks>
    [JsonStringEnumMemberName("success")]
    Success,

    /// <summary>
    /// Task failed due to a recoverable error.
    /// </summary>
    /// <remarks>
    /// Orchestrator response: Evaluate error type, potentially retry with feedback,
    /// or reassign to a different specialist. Examples include syntax errors,
    /// timeouts, or partial failures that may succeed on retry.
    /// </remarks>
    [JsonStringEnumMemberName("failure")]
    Failure,

    /// <summary>
    /// Specialist is blocked and requires Orchestrator intervention.
    /// </summary>
    /// <remarks>
    /// Orchestrator response: Analyze blocker type, decompose task if possible,
    /// request information from other specialists, or escalate to user.
    /// The specialist cannot make progress without external assistance.
    /// </remarks>
    [JsonStringEnumMemberName("help_needed")]
    HelpNeeded
}
