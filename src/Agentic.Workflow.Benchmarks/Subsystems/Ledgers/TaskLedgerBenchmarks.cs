// =============================================================================
// <copyright file="TaskLedgerBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Infrastructure.Ledgers;
using Agentic.Workflow.Orchestration.Ledgers;

using BenchmarkDotNet.Attributes;

namespace Agentic.Workflow.Benchmarks.Subsystems.Ledgers;

/// <summary>
/// Benchmarks for <see cref="TaskLedger"/> operations.
/// </summary>
/// <remarks>
/// <para>
/// Focuses on hash computation overhead including:
/// <list type="bullet">
///   <item><description>Append with hash recomputation via WithTask</description></item>
///   <item><description>Integrity verification cost via VerifyIntegrity</description></item>
///   <item><description>JSON serialization and SHA256 hash computation cost</description></item>
/// </list>
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class TaskLedgerBenchmarks
{
    private TaskLedger _ledger = null!;
    private TaskEntry _newTask = null!;
    private string _originalRequest = null!;
    private IReadOnlyList<TaskEntry> _tasks = null!;

    /// <summary>
    /// Gets or sets the number of tasks in the ledger.
    /// </summary>
    [Params(10, 100, 1000)]
    public int TaskCount { get; set; }

    /// <summary>
    /// Sets up the benchmark by creating a pre-populated task ledger.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _originalRequest = "Benchmark request: Implement comprehensive test suite with multiple components";

        var tasks = new List<TaskEntry>();
        for (var i = 0; i < TaskCount; i++)
        {
            tasks.Add(CreateTestTask(i));
        }

        _tasks = tasks;
        _ledger = TaskLedger.Create(_originalRequest, _tasks);

        // Create a new task for append benchmarks
        _newTask = TaskEntry.Create(
            description: "New benchmark task to append",
            priority: 5,
            dependencies: TaskCount > 0 ? [$"task-{TaskCount - 1}"] : null);
    }

    /// <summary>
    /// Benchmarks appending a task with hash recomputation.
    /// </summary>
    /// <returns>The new ledger with the appended task.</returns>
    [Benchmark(Description = "WithTask: Append with hash recompute")]
    public ITaskLedger WithTask_HashComputation()
    {
        return _ledger.WithTask(_newTask);
    }

    /// <summary>
    /// Benchmarks verifying integrity via hash validation.
    /// </summary>
    /// <returns>True if integrity is valid.</returns>
    [Benchmark(Description = "VerifyIntegrity: Hash validation")]
    public bool VerifyIntegrity_Validation()
    {
        return _ledger.VerifyIntegrity();
    }

    /// <summary>
    /// Benchmarks the raw hash computation (JSON serialization + SHA256).
    /// </summary>
    /// <returns>The computed hash string.</returns>
    [Benchmark(Description = "ComputeContentHash: JSON + SHA256")]
    public string ComputeContentHash_Serialization()
    {
        return ComputeContentHash(_originalRequest, _tasks);
    }

    /// <summary>
    /// Creates a test task entry with realistic data.
    /// </summary>
    private static TaskEntry CreateTestTask(int index)
    {
        var dependencies = index > 0 && index % 3 == 0
            ? new List<string> { $"task-{index - 1}" }
            : null;

        return TaskEntry.CreateWithId(
            taskId: $"task-{index}",
            description: $"Benchmark task {index}: Implement feature component with tests and documentation",
            priority: index % 5,
            dependencies: dependencies);
    }

    /// <summary>
    /// Computes a SHA-256 hash of the ledger content (mirrors TaskLedger implementation).
    /// </summary>
    private static string ComputeContentHash(string originalRequest, IReadOnlyList<TaskEntry> tasks)
    {
        var content = new
        {
            OriginalRequest = originalRequest,
            TaskIds = tasks.Select(t => t.TaskId).ToList(),
            TaskDescriptions = tasks.Select(t => t.Description).ToList()
        };

        var json = JsonSerializer.Serialize(content);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
