// =============================================================================
// <copyright file="ILoopDetector.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Orchestration.LoopDetection;

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Detects execution loops in the progress ledger by analyzing recent entries
/// for repetitive behavior patterns.
/// </summary>
/// <remarks>
/// <para>
/// Loop detection prevents workflows from consuming infinite resources by identifying
/// repetitive behavior patterns. The detector analyzes a sliding window of recent
/// progress entries and computes a confidence score using weighted components:
/// </para>
/// <list type="bullet">
///   <item><description>Repetition score (0.4 weight): Ratio of duplicate actions</description></item>
///   <item><description>Semantic score (0.3 weight): Output similarity via embeddings</description></item>
///   <item><description>Time score (0.2 weight): Duration without new artifacts</description></item>
///   <item><description>Frustration score (0.1 weight): HelpNeeded/Failure signal ratio</description></item>
/// </list>
/// <para>
/// When confidence exceeds the recovery threshold (default 0.7), a loop is detected
/// with the following type-to-strategy mapping:
/// </para>
/// <list type="bullet">
///   <item><description>ExactRepetition → InjectVariation (change parameters)</description></item>
///   <item><description>SemanticRepetition → ForceRotation (switch executors)</description></item>
///   <item><description>Oscillation → Synthesize (combine conflicting approaches)</description></item>
///   <item><description>NoProgress → Decompose (break into sub-tasks)</description></item>
/// </list>
/// </remarks>
public interface ILoopDetector
{
    /// <summary>
    /// Analyzes the progress ledger to detect potential execution loops.
    /// </summary>
    /// <param name="ledger">The progress ledger containing execution history to analyze.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>
    /// A <see cref="LoopDetectionResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Whether a loop was detected (confidence &gt; threshold)</description></item>
    ///   <item><description>The detected loop type (if any)</description></item>
    ///   <item><description>Confidence score between 0.0 and 1.0</description></item>
    ///   <item><description>Recommended recovery strategy (if loop detected)</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="ledger"/> is null.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is cancelled.
    /// </exception>
    Task<LoopDetectionResult> DetectAsync(
        IProgressLedger ledger,
        CancellationToken cancellationToken = default);
}
