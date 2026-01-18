// =============================================================================
// <copyright file="SemanticSimilarityBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Configuration;
using Agentic.Workflow.Infrastructure.Ledgers;
using Agentic.Workflow.Infrastructure.LoopDetection;
using Agentic.Workflow.Orchestration.Ledgers;
using Agentic.Workflow.Orchestration.LoopDetection;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Agentic.Workflow.Benchmarks.Subsystems.LoopDetection;

/// <summary>
/// Benchmarks measuring semantic similarity integration with loop detection.
/// </summary>
/// <remarks>
/// <para>
/// These benchmarks use a mock similarity calculator to:
/// <list type="bullet">
///   <item><description>Isolate loop detector overhead from actual similarity calculation</description></item>
///   <item><description>Verify early exit paths when high confidence is achieved</description></item>
///   <item><description>Track call frequency to the similarity calculator</description></item>
/// </list>
/// </para>
/// <para>
/// The benchmarks demonstrate that:
/// <list type="bullet">
///   <item><description>High confidence (repetition/oscillation) still calls similarity for weighted scoring</description></item>
///   <item><description>Low confidence scenarios always invoke similarity calculation</description></item>
/// </list>
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class SemanticSimilarityBenchmarks
{
    private const int WindowSize = 10;

    private LoopDetector _detectorWithTracking = null!;
    private IProgressLedger _highConfidenceLedger = null!;
    private IProgressLedger _lowConfidenceLedger = null!;
    private TrackingSemanticSimilarityCalculator _trackingCalculator = null!;

    /// <summary>
    /// Gets the number of times the similarity calculator was invoked.
    /// Exposed for validation in benchmark analysis.
    /// </summary>
    public int SimilarityCallCount => _trackingCalculator.CallCount;

    /// <summary>
    /// Sets up the benchmark environment with tracking similarity calculator.
    /// </summary>
    [GlobalSetup]
    public void GlobalSetup()
    {
        var options = new LoopDetectionOptions
        {
            WindowSize = WindowSize,
            RecoveryThreshold = 0.7,
            SimilarityThreshold = 0.85,
            RepetitionScoreWeight = 0.4,
            SemanticScoreWeight = 0.3,
            TimeScoreWeight = 0.2,
            FrustrationScoreWeight = 0.1
        };

        _trackingCalculator = new TrackingSemanticSimilarityCalculator();
        _detectorWithTracking = new LoopDetector(
            NullLogger<LoopDetector>.Instance,
            Options.Create(options),
            _trackingCalculator);

        // High confidence: All repeated actions (should detect ExactRepetition)
        _highConfidenceLedger = CreateHighConfidenceLedger();

        // Low confidence: Distinct actions (requires semantic analysis)
        _lowConfidenceLedger = CreateLowConfidenceLedger();
    }

    /// <summary>
    /// Resets the call count before each iteration.
    /// </summary>
    [IterationSetup]
    public void IterationSetup()
    {
        _trackingCalculator.ResetCallCount();
    }

    /// <summary>
    /// Benchmarks detection with high confidence pattern.
    /// Repetition pattern achieves high confidence early; similarity is still called
    /// as part of the weighted scoring calculation before loop type determination.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    [Benchmark(Baseline = true)]
    public async Task<LoopDetectionResult> DetectAsync_HighConfidence_CallsSimilarity()
    {
        return await _detectorWithTracking.DetectAsync(_highConfidenceLedger).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmarks detection with low confidence pattern requiring similarity.
    /// Distinct actions require semantic similarity calculation for accurate detection.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    [Benchmark]
    public async Task<LoopDetectionResult> DetectAsync_LowConfidence_CallsSimilarity()
    {
        return await _detectorWithTracking.DetectAsync(_lowConfidenceLedger).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a ledger with all identical actions (high confidence for repetition).
    /// </summary>
    private static IProgressLedger CreateHighConfidenceLedger()
    {
        var entries = Enumerable.Range(0, WindowSize)
            .Select(_ => CreateEntry("repeated_action"))
            .ToArray();
        return CreateLedgerWithEntries(entries);
    }

    /// <summary>
    /// Creates a ledger with distinct actions (low confidence, needs semantic).
    /// </summary>
    private static IProgressLedger CreateLowConfidenceLedger()
    {
        var entries = Enumerable.Range(0, WindowSize)
            .Select(i => CreateEntry($"distinct_action_{i}"))
            .ToArray();
        return CreateLedgerWithEntries(entries);
    }

    /// <summary>
    /// Creates a progress entry with the specified action.
    /// </summary>
    private static ProgressEntry CreateEntry(string action)
    {
        return new ProgressEntry
        {
            EntryId = $"entry-{Guid.NewGuid():N}",
            TaskId = "benchmark-task",
            ExecutorId = "Benchmark",
            Action = action,
            Output = $"Output for {action}",
            ProgressMade = true
        };
    }

    /// <summary>
    /// Creates a progress ledger with the specified entries.
    /// </summary>
    private static IProgressLedger CreateLedgerWithEntries(ProgressEntry[] entries)
    {
        var ledger = ProgressLedger.Create("benchmark-task-ledger");
        return ledger.WithEntries(entries);
    }

    /// <summary>
    /// Semantic similarity calculator that tracks invocation count.
    /// </summary>
    private sealed class TrackingSemanticSimilarityCalculator : ISemanticSimilarityCalculator
    {
        private int _callCount;

        /// <summary>
        /// Gets the number of times <see cref="CalculateMaxSimilarityAsync"/> was called.
        /// </summary>
        public int CallCount => _callCount;

        /// <summary>
        /// Resets the call count to zero.
        /// </summary>
        public void ResetCallCount()
        {
            _callCount = 0;
        }

        /// <inheritdoc/>
        public Task<double> CalculateMaxSimilarityAsync(
            IReadOnlyList<string?> outputs,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);

            // Return low similarity to not trigger semantic repetition detection
            return Task.FromResult(0.2);
        }
    }
}
