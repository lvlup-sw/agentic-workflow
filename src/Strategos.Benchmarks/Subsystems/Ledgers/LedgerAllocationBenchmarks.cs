// =============================================================================
// <copyright file="LedgerAllocationBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Infrastructure.Ledgers;
using Strategos.Orchestration.Ledgers;

using BenchmarkDotNet.Attributes;

namespace Strategos.Benchmarks.Subsystems.Ledgers;

/// <summary>
/// Benchmarks focused on memory allocations per ledger append operation.
/// </summary>
/// <remarks>
/// <para>
/// Measures memory allocations for append operations to validate the
/// target of less than 1KB per append. Key measurements include:
/// <list type="bullet">
///   <item><description>ProgressLedger WithEntry allocation cost</description></item>
///   <item><description>TaskLedger WithTask allocation cost</description></item>
/// </list>
/// </para>
/// <para>
/// Gen0 and Allocated columns should be populated for allocation analysis.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class LedgerAllocationBenchmarks
{
    private ProgressLedger _progressLedger = null!;
    private TaskLedger _taskLedger = null!;
    private ProgressEntry _progressEntry = null!;
    private TaskEntry _taskEntry = null!;

    /// <summary>
    /// Gets or sets the size of the existing ledger (affects list copy cost).
    /// </summary>
    [Params(10, 100, 1000)]
    public int ExistingEntryCount { get; set; }

    /// <summary>
    /// Sets up the benchmark with pre-populated ledgers.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        // Setup progress ledger
        _progressLedger = ProgressLedger.Create("task-ledger-allocation-test");
        for (var i = 0; i < ExistingEntryCount; i++)
        {
            var entry = CreateProgressEntry(i);
            _progressLedger = (ProgressLedger)_progressLedger.WithEntry(entry);
        }

        // Setup task ledger
        var tasks = new List<TaskEntry>();
        for (var i = 0; i < ExistingEntryCount; i++)
        {
            tasks.Add(CreateTaskEntry(i));
        }

        _taskLedger = TaskLedger.Create(
            "Allocation benchmark: Test memory usage per append operation",
            tasks);

        // Create entries for append benchmarks
        _progressEntry = CreateProgressEntry(ExistingEntryCount);
        _taskEntry = TaskEntry.Create(
            description: "New task for allocation measurement",
            priority: 1);
    }

    /// <summary>
    /// Benchmarks memory allocation for ProgressLedger WithEntry.
    /// </summary>
    /// <remarks>
    /// Target: Less than 1KB allocated per append.
    /// </remarks>
    /// <returns>The new ledger with the appended entry.</returns>
    [Benchmark(Description = "ProgressLedger.WithEntry allocations")]
    public IProgressLedger ProgressLedger_WithEntry_Allocations()
    {
        return _progressLedger.WithEntry(_progressEntry);
    }

    /// <summary>
    /// Benchmarks memory allocation for TaskLedger WithTask.
    /// </summary>
    /// <remarks>
    /// Target: Less than 1KB allocated per append.
    /// Includes hash recomputation overhead.
    /// </remarks>
    /// <returns>The new ledger with the appended task.</returns>
    [Benchmark(Description = "TaskLedger.WithTask allocations")]
    public ITaskLedger TaskLedger_WithTask_Allocations()
    {
        return _taskLedger.WithTask(_taskEntry);
    }

    /// <summary>
    /// Creates a test progress entry with minimal allocations.
    /// </summary>
    private static ProgressEntry CreateProgressEntry(int index)
    {
        return new ProgressEntry
        {
            EntryId = $"progress-alloc-{index}",
            TaskId = $"task-{index % 10}",
            ExecutorId = $"executor-{index % 3}",
            Action = $"Action {index}",
            ProgressMade = true,
            TokensConsumed = 100,
            Duration = TimeSpan.FromMilliseconds(50)
        };
    }

    /// <summary>
    /// Creates a test task entry with minimal allocations.
    /// </summary>
    private static TaskEntry CreateTaskEntry(int index)
    {
        return TaskEntry.CreateWithId(
            taskId: $"task-{index}",
            description: $"Task {index}: Implementation task",
            priority: index % 5);
    }
}
