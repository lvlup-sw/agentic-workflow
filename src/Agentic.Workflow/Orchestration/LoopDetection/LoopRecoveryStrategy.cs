// =============================================================================
// <copyright file="LoopRecoveryStrategy.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Agentic.Workflow.Orchestration.LoopDetection;

/// <summary>
/// Defines the strategies for recovering from detected execution loops.
/// </summary>
/// <remarks>
/// <para>
/// Each <see cref="LoopType"/> maps to one or more recovery strategies.
/// The workflow engine selects the appropriate strategy based on the loop type
/// and available options.
/// </para>
/// <para>
/// Recovery strategies are applied through prompt injection, executor
/// constraints, or task decomposition.
/// </para>
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LoopRecoveryStrategy
{
    /// <summary>
    /// Inject a variation constraint into the executor's prompt.
    /// </summary>
    /// <remarks>
    /// Used for <see cref="LoopType.ExactRepetition"/>.
    /// Adds constraint: "Do NOT use the previous approach. Try a different method."
    /// </remarks>
    [JsonStringEnumMemberName("inject_variation")]
    InjectVariation,

    /// <summary>
    /// Force rotation to a different executor.
    /// </summary>
    /// <remarks>
    /// Used for <see cref="LoopType.SemanticRepetition"/>.
    /// Excludes recently used executors from selection to force fresh perspective.
    /// </remarks>
    [JsonStringEnumMemberName("force_rotation")]
    ForceRotation,

    /// <summary>
    /// Synthesize insights from multiple approaches.
    /// </summary>
    /// <remarks>
    /// Used for <see cref="LoopType.Oscillation"/>.
    /// Combines findings from both sides of the oscillation to break the cycle.
    /// </remarks>
    [JsonStringEnumMemberName("synthesize")]
    Synthesize,

    /// <summary>
    /// Decompose the task into smaller subtasks.
    /// </summary>
    /// <remarks>
    /// Used for <see cref="LoopType.NoProgress"/>.
    /// Breaks the task into more granular steps that executors can complete.
    /// </remarks>
    [JsonStringEnumMemberName("decompose")]
    Decompose,

    /// <summary>
    /// Escalate to the user for manual intervention.
    /// </summary>
    /// <remarks>
    /// Used when other strategies fail or the task cannot be decomposed further.
    /// Requests user guidance to break the loop.
    /// </remarks>
    [JsonStringEnumMemberName("escalate")]
    Escalate
}
