// =============================================================================
// <copyright file="BlockerType.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Strategos.Agents.Models;

/// <summary>
/// Defines the types of blockers that prevent a specialist from completing a task.
/// </summary>
/// <remarks>
/// Used with <see cref="SignalType.HelpNeeded"/> to provide structured information
/// about why the specialist cannot proceed. The Orchestrator uses this to determine
/// the appropriate intervention strategy.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BlockerType
{
    /// <summary>
    /// The specialist lacks sufficient context or data to proceed.
    /// </summary>
    /// <remarks>
    /// Resolution: Orchestrator may need to invoke another specialist to gather
    /// additional information before retrying this task.
    /// </remarks>
    [JsonStringEnumMemberName("insufficient_data")]
    InsufficientData,

    /// <summary>
    /// The task requires capabilities that this specialist does not possess.
    /// </summary>
    /// <remarks>
    /// Resolution: Orchestrator should reassign to a specialist with matching
    /// capabilities. This indicates a selection error or task miscategorization.
    /// </remarks>
    [JsonStringEnumMemberName("capability_mismatch")]
    CapabilityMismatch,

    /// <summary>
    /// The task goal is unclear or contains contradictions.
    /// </summary>
    /// <remarks>
    /// Resolution: Orchestrator may need to decompose the task further or
    /// escalate to the user for clarification.
    /// </remarks>
    [JsonStringEnumMemberName("ambiguous_goal")]
    AmbiguousGoal,

    /// <summary>
    /// The task depends on an external resource that is unavailable.
    /// </summary>
    /// <remarks>
    /// Resolution: Wait for external resource, use cached data if available,
    /// or fail gracefully with partial results.
    /// </remarks>
    [JsonStringEnumMemberName("external_dependency")]
    ExternalDependency,

    /// <summary>
    /// Budget or resource quota has been exhausted.
    /// </summary>
    /// <remarks>
    /// Resolution: Orchestrator should evaluate scarcity level and either
    /// allocate additional budget, reduce scope, or terminate gracefully.
    /// </remarks>
    [JsonStringEnumMemberName("resource_exhausted")]
    ResourceExhausted,

    /// <summary>
    /// An unexpected error or state occurred that the specialist cannot handle.
    /// </summary>
    /// <remarks>
    /// Resolution: Log the error for debugging, attempt recovery if possible,
    /// or escalate to user with error details.
    /// </remarks>
    [JsonStringEnumMemberName("unexpected_state")]
    UnexpectedState
}
