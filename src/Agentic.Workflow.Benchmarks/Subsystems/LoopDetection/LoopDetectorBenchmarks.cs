// =============================================================================
// <copyright file="LoopDetectorBenchmarks.cs" company="Levelup Software">
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
/// Benchmarks for the <see cref="LoopDetector"/> component measuring detection
/// performance across different loop patterns and window sizes.
/// </summary>
/// <remarks>
/// <para>
/// These benchmarks measure:
/// <list type="bullet">
///   <item><description>Baseline detection with no loop patterns</description></item>
///   <item><description>Repetition loop detection (early exit path)</description></item>
///   <item><description>Oscillation pattern detection (A-B-A-B patterns)</description></item>
/// </list>
/// </para>
/// <para>
/// Early exit optimizations should be visible: repetition detection should skip
/// semantic similarity calculation, resulting in faster execution.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class LoopDetectorBenchmarks
{
    private LoopDetector _detector = null!;
    private IProgressLedger _noLoopLedger = null!;
    private IProgressLedger _repetitionLedger = null!;
    private IProgressLedger _oscillationLedger = null!;

    /// <summary>
    /// Gets or sets the window size for loop detection analysis.
    /// </summary>
    [Params(10, 20, 50)]
    public int WindowSize { get; set; }

    /// <summary>
    /// Sets up the benchmark environment including loop detector and test ledgers.
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

        _detector = new LoopDetector(
            NullLogger<LoopDetector>.Instance,
            Options.Create(options),
            new NoOpSemanticSimilarityCalculator());

        _noLoopLedger = CreateNoLoopLedger(WindowSize);
        _repetitionLedger = CreateRepetitionLedger(WindowSize);
        _oscillationLedger = CreateOscillationLedger(WindowSize);
    }

    /// <summary>
    /// Benchmarks loop detection with normal entries (no loop pattern).
    /// This establishes a baseline for detection overhead.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    [Benchmark(Baseline = true)]
    public async Task<LoopDetectionResult> DetectAsync_NoLoop()
    {
        return await _detector.DetectAsync(_noLoopLedger).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmarks loop detection with a repetition pattern (all same action).
    /// This should trigger early exit before semantic similarity calculation.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    [Benchmark]
    public async Task<LoopDetectionResult> DetectAsync_RepetitionLoop()
    {
        return await _detector.DetectAsync(_repetitionLedger).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmarks loop detection with an A-B-A-B oscillation pattern.
    /// This tests the oscillation score calculation path.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    [Benchmark]
    public async Task<LoopDetectionResult> DetectAsync_OscillationLoop()
    {
        return await _detector.DetectAsync(_oscillationLedger).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a ledger with distinct actions (no loop pattern).
    /// </summary>
    private static IProgressLedger CreateNoLoopLedger(int entryCount)
    {
        var entries = Enumerable.Range(0, entryCount)
            .Select(i => CreateEntry($"distinct_action_{i}"))
            .ToArray();
        return CreateLedgerWithEntries(entries);
    }

    /// <summary>
    /// Creates a ledger with all identical actions (repetition pattern).
    /// </summary>
    private static IProgressLedger CreateRepetitionLedger(int entryCount)
    {
        var entries = Enumerable.Range(0, entryCount)
            .Select(_ => CreateEntry("repeated_action"))
            .ToArray();
        return CreateLedgerWithEntries(entries);
    }

    /// <summary>
    /// Creates a ledger with A-B-A-B alternating pattern (oscillation).
    /// </summary>
    private static IProgressLedger CreateOscillationLedger(int entryCount)
    {
        var entries = Enumerable.Range(0, entryCount)
            .Select(i => CreateEntry(i % 2 == 0 ? "action_a" : "action_b"))
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
    /// No-op semantic similarity calculator for benchmarking.
    /// Returns 0.0 to isolate loop detector overhead from similarity calculation.
    /// </summary>
    private sealed class NoOpSemanticSimilarityCalculator : ISemanticSimilarityCalculator
    {
        /// <inheritdoc/>
        public Task<double> CalculateMaxSimilarityAsync(
            IEnumerable<string?> outputs,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0.0);
        }
    }
}
