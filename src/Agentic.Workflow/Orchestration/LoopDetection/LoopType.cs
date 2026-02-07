// =============================================================================
// <copyright file="LoopType.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Agentic.Workflow.Orchestration.LoopDetection;

/// <summary>
/// Defines the types of execution loops that can be detected in the progress ledger.
/// </summary>
/// <remarks>
/// <para>
/// The workflow engine analyzes the progress ledger to detect repetitive behavior patterns
/// that indicate the workflow is stuck. Without loop detection, the system could consume
/// infinite resources without producing useful results.
/// </para>
/// <para>
/// Each loop type has a corresponding <see cref="LoopRecoveryStrategy"/> for resolution.
/// </para>
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LoopType
{
    /// <summary>
    /// The same action sequence is being repeated exactly.
    /// </summary>
    /// <remarks>
    /// Detection: Duplicate action entries in the sliding window.
    /// Recovery: Inject variation constraint - "Do NOT use previous approach".
    /// </remarks>
    [JsonStringEnumMemberName("exact_repetition")]
    ExactRepetition,

    /// <summary>
    /// Similar outputs are being produced (cosine similarity above threshold).
    /// </summary>
    /// <remarks>
    /// Detection: Output embeddings have similarity > 0.85.
    /// Recovery: Force executor rotation - exclude recent executors from selection.
    /// </remarks>
    [JsonStringEnumMemberName("semantic_repetition")]
    SemanticRepetition,

    /// <summary>
    /// Actions are alternating in an A→B→A→B pattern.
    /// </summary>
    /// <remarks>
    /// Detection: Alternating executor or action pattern detected.
    /// Recovery: Synthesize - combine insights from both approaches.
    /// </remarks>
    [JsonStringEnumMemberName("oscillation")]
    Oscillation,

    /// <summary>
    /// Activity is occurring but no observable progress is being made.
    /// </summary>
    /// <remarks>
    /// Detection: All recent progress entries have ProgressMade = false.
    /// Recovery: Decompose task or escalate to user.
    /// </remarks>
    [JsonStringEnumMemberName("no_progress")]
    NoProgress
}