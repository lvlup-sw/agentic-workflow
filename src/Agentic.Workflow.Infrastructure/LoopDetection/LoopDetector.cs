// =============================================================================
// <copyright file="LoopDetector.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Configuration;
using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Orchestration.Ledgers;
using Agentic.Workflow.Orchestration.LoopDetection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Agentic.Workflow.Infrastructure.LoopDetection;

/// <summary>
/// Detects execution loops in the progress ledger by analyzing recent entries
/// for repetitive behavior patterns.
/// </summary>
/// <remarks>
/// <para>
/// The detector analyzes a sliding window of recent progress entries and computes
/// a confidence score using weighted components. When confidence exceeds the
/// recovery threshold, a loop is detected with an appropriate recovery strategy.
/// </para>
/// </remarks>
public sealed class LoopDetector : ILoopDetector
{
    private readonly ILogger<LoopDetector> _logger;
    private readonly LoopDetectionOptions _options;
    private readonly ISemanticSimilarityCalculator _similarityCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopDetector"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="options">Configuration options for loop detection.</param>
    /// <param name="similarityCalculator">Calculator for semantic similarity.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is null.
    /// </exception>
    public LoopDetector(
        ILogger<LoopDetector> logger,
        IOptions<LoopDetectionOptions> options,
        ISemanticSimilarityCalculator similarityCalculator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(similarityCalculator, nameof(similarityCalculator));

        _logger = logger;
        _options = options.Value;
        _similarityCalculator = similarityCalculator;
    }

    /// <inheritdoc/>
    public async Task<LoopDetectionResult> DetectAsync(
        IProgressLedger ledger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ledger, nameof(ledger));

        // Handle empty/insufficient data
        var recentEntries = ledger.GetRecentEntries(_options.WindowSize);

        if (recentEntries.Count < _options.WindowSize)
        {
            return LoopDetectionResult.NoLoop(
                confidence: 0.0,
                diagnosticMessage: $"Insufficient entries: {recentEntries.Count} < {_options.WindowSize}");
        }

        // Calculate repetition score
        var repetitionScore = CalculateRepetitionScore(recentEntries);

        // Early return: Skip expensive semantic similarity when exact repetition detected
        if (repetitionScore >= 1.0 - double.Epsilon)
        {
            var earlyConfidence = _options.RepetitionScoreWeight * repetitionScore;
            return LoopDetectionResult.Detected(
                loopType: LoopType.ExactRepetition,
                confidence: Math.Max(earlyConfidence, _options.RecoveryThreshold),
                strategy: LoopRecoveryStrategy.InjectVariation,
                diagnosticMessage: $"All {recentEntries.Count} entries have identical action");
        }

        // Calculate no-progress score
        var noProgressScore = CalculateNoProgressScore(recentEntries);

        // Early return: Skip expensive semantic similarity when perfect no-progress detected
        if (noProgressScore >= 1.0 - double.Epsilon)
        {
            var earlyConfidence = _options.TimeScoreWeight * noProgressScore;
            return LoopDetectionResult.Detected(
                loopType: LoopType.NoProgress,
                confidence: Math.Max(earlyConfidence, _options.RecoveryThreshold),
                strategy: LoopRecoveryStrategy.Decompose,
                diagnosticMessage: $"All {recentEntries.Count} entries show no progress");
        }

        // Calculate oscillation score
        var oscillationScore = CalculateOscillationScore(recentEntries);

        // Only compute semantic similarity if no high-confidence signal detected
        var outputs = recentEntries.Select(e => e.Output).ToList();
        var semanticScore = await _similarityCalculator
            .CalculateMaxSimilarityAsync(outputs, cancellationToken)
            .ConfigureAwait(false);

        // Calculate frustration score
        var frustrationScore = CalculateFrustrationScore(recentEntries);

        // Calculate weighted confidence
        // repetition (0.4) + semantic (0.3) + time/no-progress (0.2) + frustration (0.1)
        var confidence = (_options.RepetitionScoreWeight * repetitionScore)
            + (_options.SemanticScoreWeight * semanticScore)
            + (_options.TimeScoreWeight * noProgressScore)
            + (_options.FrustrationScoreWeight * frustrationScore);

        // Detect Oscillation patterns (A-B-A-B or A-B-C-A-B-C)
        if (oscillationScore >= 0.8)
        {
            var oscillationConfidence = Math.Max(confidence + oscillationScore * 0.5, _options.RecoveryThreshold);
            return LoopDetectionResult.Detected(
                loopType: LoopType.Oscillation,
                confidence: oscillationConfidence,
                strategy: LoopRecoveryStrategy.Synthesize,
                diagnosticMessage: $"Oscillation pattern detected with score {oscillationScore:P0}");
        }

        // Detect SemanticRepetition when outputs are highly similar
        if (semanticScore >= _options.SimilarityThreshold)
        {
            var semanticConfidence = Math.Max(confidence, _options.RecoveryThreshold);
            return LoopDetectionResult.Detected(
                loopType: LoopType.SemanticRepetition,
                confidence: semanticConfidence,
                strategy: LoopRecoveryStrategy.ForceRotation,
                diagnosticMessage: $"Semantic similarity {semanticScore:P0} exceeds threshold {_options.SimilarityThreshold:P0}");
        }

        // Return result with current confidence (may or may not exceed threshold)
        if (confidence >= _options.RecoveryThreshold)
        {
            // Determine loop type based on dominant signal
            var loopType = DetermineLoopType(repetitionScore, noProgressScore, semanticScore);
            var strategy = LoopDetectionResult.GetDefaultStrategy(loopType);

            return LoopDetectionResult.Detected(
                loopType: loopType,
                confidence: confidence,
                strategy: strategy,
                diagnosticMessage: $"Loop detected - repetition: {repetitionScore:P0}, semantic: {semanticScore:P0}, no-progress: {noProgressScore:P0}");
        }

        return LoopDetectionResult.NoLoop(
            confidence: confidence,
            diagnosticMessage: $"Repetition: {repetitionScore:P0}, semantic: {semanticScore:P0}, no-progress: {noProgressScore:P0}, oscillation: {oscillationScore:P0}");
    }

    /// <summary>
    /// Determines the loop type based on the dominant score component.
    /// </summary>
    private static LoopType DetermineLoopType(double repetitionScore, double noProgressScore, double semanticScore)
    {
        var max = Math.Max(Math.Max(repetitionScore, noProgressScore), semanticScore);

        if (Math.Abs(max - semanticScore) < double.Epsilon)
        {
            return LoopType.SemanticRepetition;
        }

        if (Math.Abs(max - noProgressScore) < double.Epsilon)
        {
            return LoopType.NoProgress;
        }

        return LoopType.ExactRepetition;
    }

    /// <summary>
    /// Calculates the repetition score based on duplicate actions in the window.
    /// </summary>
    /// <param name="entries">Recent progress entries to analyze.</param>
    /// <returns>
    /// Score between 0.0 and 1.0 where 1.0 means all actions are identical.
    /// </returns>
    private static double CalculateRepetitionScore(IReadOnlyList<ProgressEntry> entries)
    {
        if (entries.Count == 0)
        {
            return 0.0;
        }

        // Group by action and find the most frequent - avoid intermediate list allocation
        var maxCount = entries.GroupBy(e => e.Action).Max(g => g.Count());
        return (double)maxCount / entries.Count;
    }

    /// <summary>
    /// Calculates the no-progress score based on entries with ProgressMade = false.
    /// </summary>
    /// <param name="entries">Recent progress entries to analyze.</param>
    /// <returns>
    /// Score between 0.0 and 1.0 where 1.0 means all entries show no progress.
    /// </returns>
    private static double CalculateNoProgressScore(IReadOnlyList<ProgressEntry> entries)
    {
        if (entries.Count == 0)
        {
            return 0.0;
        }

        var noProgressCount = entries.Count(e => !e.ProgressMade);
        return (double)noProgressCount / entries.Count;
    }

    /// <summary>
    /// Calculates the oscillation score by detecting repeating patterns (A-B-A-B or A-B-C-A-B-C).
    /// </summary>
    /// <param name="entries">Recent progress entries to analyze.</param>
    /// <returns>
    /// Score between 0.0 and 1.0 where 1.0 means a perfect repeating pattern.
    /// </returns>
    private static double CalculateOscillationScore(IReadOnlyList<ProgressEntry> entries)
    {
        if (entries.Count < 3)
        {
            return 0.0;
        }

        var actions = entries.Select(e => e.Action).ToArray();
        var maxScore = 0.0;

        // Try different period lengths (2 to half of window size)
        var maxPeriod = entries.Count / 2;
        for (var period = 2; period <= maxPeriod; period++)
        {
            var score = CalculatePeriodScore(actions, period);
            maxScore = Math.Max(maxScore, score);
        }

        return maxScore;
    }

    /// <summary>
    /// Calculates how well the actions match a repeating pattern of the given period.
    /// </summary>
    /// <param name="actions">The action sequence to analyze.</param>
    /// <param name="period">The period length to check.</param>
    /// <returns>Score between 0.0 and 1.0.</returns>
    private static double CalculatePeriodScore(string[] actions, int period)
    {
        if (actions.Length < period * 2)
        {
            return 0.0;
        }

        var matches = 0;
        var comparisons = 0;

        // Check if each action matches its corresponding position in the previous period
        for (var i = period; i < actions.Length; i++)
        {
            comparisons++;
            if (string.Equals(actions[i], actions[i % period], StringComparison.Ordinal))
            {
                matches++;
            }
        }

        return comparisons > 0 ? (double)matches / comparisons : 0.0;
    }

    /// <summary>
    /// Calculates the frustration score based on HelpNeeded and Failure signals.
    /// </summary>
    /// <param name="entries">Recent progress entries to analyze.</param>
    /// <returns>
    /// Score between 0.0 and 1.0 where 1.0 means all entries have frustration signals.
    /// </returns>
    private static double CalculateFrustrationScore(IReadOnlyList<ProgressEntry> entries)
    {
        if (entries.Count == 0)
        {
            return 0.0;
        }

        // Count entries with HelpNeeded or Failure signals
        var frustrationCount = entries.Count(e =>
            e.Signal is not null
            && (e.Signal.Type == SignalType.HelpNeeded || e.Signal.Type == SignalType.Failure));

        return (double)frustrationCount / entries.Count;
    }
}
