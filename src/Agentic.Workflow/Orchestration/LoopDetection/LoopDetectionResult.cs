// =============================================================================
// <copyright file="LoopDetectionResult.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Orchestration.LoopDetection;

/// <summary>
/// Represents the result of loop detection analysis on the progress ledger.
/// </summary>
/// <remarks>
/// <para>
/// Loop detection uses confidence scoring rather than binary detection to reduce
/// false positives for legitimate deep work that may superficially resemble repetition.
/// </para>
/// <para>
/// Confidence thresholds:
/// <list type="bullet">
///   <item><description>&lt; 0.3: Normal operation</description></item>
///   <item><description>0.3 - 0.5: Mild concern (log warning)</description></item>
///   <item><description>0.5 - 0.7: Potential loop (increase monitoring)</description></item>
///   <item><description>&gt; 0.7: Likely loop (trigger recovery)</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record LoopDetectionResult
{
    /// <summary>
    /// Gets a value indicating whether a loop was detected.
    /// </summary>
    /// <remarks>
    /// True when <see cref="Confidence"/> exceeds the recovery threshold (typically 0.7).
    /// </remarks>
    public required bool LoopDetected { get; init; }

    /// <summary>
    /// Gets the type of loop detected, if any.
    /// </summary>
    /// <value>The detected loop type, or <c>null</c> if no loop was detected.</value>
    public LoopType? DetectedType { get; init; }

    /// <summary>
    /// Gets the confidence score for the loop detection (0.0 to 1.0).
    /// </summary>
    /// <remarks>
    /// Computed from weighted combination of:
    /// <list type="bullet">
    ///   <item><description>Repetition score (0.4 weight)</description></item>
    ///   <item><description>Semantic similarity (0.3 weight)</description></item>
    ///   <item><description>Time since last artifact (0.2 weight)</description></item>
    ///   <item><description>Executor frustration (0.1 weight)</description></item>
    /// </list>
    /// </remarks>
    public required double Confidence { get; init; }

    /// <summary>
    /// Gets the recommended recovery strategy for the detected loop.
    /// </summary>
    /// <value>The recommended strategy, or <c>null</c> if no loop was detected.</value>
    public LoopRecoveryStrategy? RecommendedStrategy { get; init; }

    /// <summary>
    /// Gets the timestamp when this analysis was performed.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets optional diagnostic message with analysis details.
    /// </summary>
    public string? DiagnosticMessage { get; init; }

    /// <summary>
    /// Creates a result indicating no loop was detected.
    /// </summary>
    /// <param name="confidence">The confidence score (should be below threshold).</param>
    /// <param name="diagnosticMessage">Optional diagnostic message.</param>
    /// <returns>A no-loop detection result.</returns>
    public static LoopDetectionResult NoLoop(double confidence = 0.0, string? diagnosticMessage = null)
    {
        return new LoopDetectionResult
        {
            LoopDetected = false,
            Confidence = confidence,
            DiagnosticMessage = diagnosticMessage
        };
    }

    /// <summary>
    /// Creates a result indicating a loop was detected.
    /// </summary>
    /// <param name="loopType">The type of loop detected.</param>
    /// <param name="confidence">The confidence score.</param>
    /// <param name="strategy">The recommended recovery strategy.</param>
    /// <param name="diagnosticMessage">Optional diagnostic message.</param>
    /// <returns>A loop detection result with recovery recommendation.</returns>
    public static LoopDetectionResult Detected(
        LoopType loopType,
        double confidence,
        LoopRecoveryStrategy strategy,
        string? diagnosticMessage = null)
    {
        return new LoopDetectionResult
        {
            LoopDetected = true,
            DetectedType = loopType,
            Confidence = confidence,
            RecommendedStrategy = strategy,
            DiagnosticMessage = diagnosticMessage
        };
    }

    /// <summary>
    /// Gets the recommended recovery strategy based on the detected loop type.
    /// </summary>
    /// <param name="loopType">The type of loop detected.</param>
    /// <returns>The default recovery strategy for the loop type.</returns>
    public static LoopRecoveryStrategy GetDefaultStrategy(LoopType loopType)
    {
        return loopType switch
        {
            LoopType.ExactRepetition => LoopRecoveryStrategy.InjectVariation,
            LoopType.SemanticRepetition => LoopRecoveryStrategy.ForceRotation,
            LoopType.Oscillation => LoopRecoveryStrategy.Synthesize,
            LoopType.NoProgress => LoopRecoveryStrategy.Decompose,
            _ => LoopRecoveryStrategy.Escalate
        };
    }
}
