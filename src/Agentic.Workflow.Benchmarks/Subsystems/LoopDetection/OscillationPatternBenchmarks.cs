// =============================================================================
// <copyright file="OscillationPatternBenchmarks.cs" company="Levelup Software">
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
/// Benchmarks focused on oscillation pattern detection performance.
/// </summary>
/// <remarks>
/// <para>
/// These benchmarks measure the oscillation score calculation through the
/// public <see cref="LoopDetector.DetectAsync"/> method with controlled patterns:
/// <list type="bullet">
///   <item><description>Period 2: A-B-A-B pattern</description></item>
///   <item><description>Period 3: A-B-C-A-B-C pattern</description></item>
///   <item><description>No period: Random action sequence</description></item>
/// </list>
/// </para>
/// <para>
/// The internal <c>CalculateOscillationScore</c> method tests periods from 2 to
/// windowSize/2, so pattern detection overhead scales with window size.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class OscillationPatternBenchmarks
{
    private const int WindowSize = 20;

    private LoopDetector _detector = null!;
    private IProgressLedger _period2Ledger = null!;
    private IProgressLedger _period3Ledger = null!;
    private IProgressLedger _noPeriodLedger = null!;

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

        _period2Ledger = CreatePeriod2Ledger();
        _period3Ledger = CreatePeriod3Ledger();
        _noPeriodLedger = CreateNoPeriodLedger();
    }

    /// <summary>
    /// Benchmarks oscillation detection with A-B-A-B pattern (period 2).
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    [Benchmark(Baseline = true)]
    public async Task<LoopDetectionResult> CalculateOscillationScore_Period2()
    {
        return await _detector.DetectAsync(_period2Ledger).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmarks oscillation detection with A-B-C-A-B-C pattern (period 3).
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    [Benchmark]
    public async Task<LoopDetectionResult> CalculateOscillationScore_Period3()
    {
        return await _detector.DetectAsync(_period3Ledger).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmarks oscillation detection with random actions (no period).
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    [Benchmark]
    public async Task<LoopDetectionResult> CalculateOscillationScore_NoPeriod()
    {
        return await _detector.DetectAsync(_noPeriodLedger).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a ledger with A-B-A-B pattern (period 2).
    /// </summary>
    private static IProgressLedger CreatePeriod2Ledger()
    {
        var entries = Enumerable.Range(0, WindowSize)
            .Select(i => CreateEntry(i % 2 == 0 ? "action_a" : "action_b"))
            .ToArray();
        return CreateLedgerWithEntries(entries);
    }

    /// <summary>
    /// Creates a ledger with A-B-C-A-B-C pattern (period 3).
    /// </summary>
    private static IProgressLedger CreatePeriod3Ledger()
    {
        var actions = new[] { "action_a", "action_b", "action_c" };
        var entries = Enumerable.Range(0, WindowSize)
            .Select(i => CreateEntry(actions[i % 3]))
            .ToArray();
        return CreateLedgerWithEntries(entries);
    }

    /// <summary>
    /// Creates a ledger with random actions (no oscillation pattern).
    /// </summary>
    private static IProgressLedger CreateNoPeriodLedger()
    {
        // Use deterministic "random" sequence for reproducible benchmarks
        var entries = Enumerable.Range(0, WindowSize)
            .Select(i => CreateEntry($"random_action_{(i * 7 + 3) % 13}"))
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
    /// </summary>
    private sealed class NoOpSemanticSimilarityCalculator : ISemanticSimilarityCalculator
    {
        /// <inheritdoc/>
        public Task<double> CalculateMaxSimilarityAsync(
            IReadOnlyList<string?> outputs,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0.0);
        }
    }
}
