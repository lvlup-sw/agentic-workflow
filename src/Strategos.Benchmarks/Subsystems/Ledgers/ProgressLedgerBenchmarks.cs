// =============================================================================
// <copyright file="ProgressLedgerBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Infrastructure.Ledgers;
using Strategos.Orchestration.Ledgers;

using BenchmarkDotNet.Attributes;

namespace Strategos.Benchmarks.Subsystems.Ledgers;

/// <summary>
/// Benchmarks for <see cref="ProgressLedger"/> operations.
/// </summary>
/// <remarks>
/// <para>
/// Measures performance of core ledger operations including:
/// <list type="bullet">
///   <item><description>Append operations via WithEntry</description></item>
///   <item><description>Recent entry retrieval via TakeLast</description></item>
///   <item><description>Metrics computation and aggregation</description></item>
/// </list>
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ProgressLedgerBenchmarks
{
    private ProgressLedger _ledger = null!;
    private ProgressEntry _singleEntry = null!;

    /// <summary>
    /// Gets or sets the number of entries to pre-populate in the ledger.
    /// </summary>
    [Params(10, 100, 1000)]
    public int EntryCount { get; set; }

    /// <summary>
    /// Sets up the benchmark by creating a pre-populated ledger.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _ledger = ProgressLedger.Create("task-ledger-benchmark");

        // Pre-populate ledger with entries
        for (var i = 0; i < EntryCount; i++)
        {
            var entry = CreateTestEntry($"task-{i % 10}", $"executor-{i % 3}", i);
            _ledger = (ProgressLedger)_ledger.WithEntry(entry);
        }

        // Create a single entry for append benchmarks
        _singleEntry = CreateTestEntry("task-benchmark", "executor-benchmark", EntryCount);
    }

    /// <summary>
    /// Benchmarks appending a single entry to the ledger.
    /// </summary>
    /// <returns>The new ledger with the appended entry.</returns>
    [Benchmark(Description = "WithEntry: Single append")]
    public IProgressLedger WithEntry_Append()
    {
        return _ledger.WithEntry(_singleEntry);
    }

    /// <summary>
    /// Benchmarks retrieving recent entries using TakeLast.
    /// </summary>
    /// <returns>The most recent entries.</returns>
    [Benchmark(Description = "GetRecentEntries: TakeLast(5)")]
    public IReadOnlyList<ProgressEntry> GetRecentEntries_TakeLast()
    {
        return _ledger.GetRecentEntries(5);
    }

    /// <summary>
    /// Benchmarks computing aggregate metrics over all entries.
    /// </summary>
    /// <returns>The computed metrics.</returns>
    [Benchmark(Description = "GetMetrics: Aggregate computation")]
    public ProgressLedgerMetrics GetMetrics_Aggregate()
    {
        return _ledger.GetMetrics();
    }

    /// <summary>
    /// Creates a test progress entry with realistic data.
    /// </summary>
    private static ProgressEntry CreateTestEntry(string taskId, string executorId, int index)
    {
        var hasSignal = index % 5 == 0;
        var signalType = index % 10 == 0 ? SignalType.Failure : SignalType.Success;

        return new ProgressEntry
        {
            EntryId = $"progress-{Guid.NewGuid():N}",
            TaskId = taskId,
            ExecutorId = executorId,
            Action = $"Benchmark action {index}",
            ProgressMade = true,
            Output = $"Output for entry {index}",
            TokensConsumed = 100 + (index % 500),
            Duration = TimeSpan.FromMilliseconds(50 + (index % 100)),
            Artifacts = index % 3 == 0 ? [$"artifact-{index}.txt"] : [],
            Signal = hasSignal
                ? new ExecutorSignal
                {
                    ExecutorId = executorId,
                    Type = signalType,
                    SuccessData = signalType == SignalType.Success
                        ? new ExecutorSuccessData { Result = "Success result" }
                        : null,
                    FailureData = signalType == SignalType.Failure
                        ? new ExecutorFailureData { Reason = "Benchmark failure" }
                        : null
                }
                : null
        };
    }
}
