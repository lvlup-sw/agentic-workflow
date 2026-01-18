// =============================================================================
// <copyright file="TaskLedgerHashingTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Security.Cryptography;
using MemoryPack;

namespace Agentic.Workflow.Infrastructure.Tests.Ledgers;

/// <summary>
/// Unit tests for TaskLedger content hashing using MemoryPack serialization.
/// </summary>
[Property("Category", "Unit")]
public sealed class TaskLedgerHashingTests
{
    /// <summary>
    /// Verifies that ComputeContentHash produces consistent hashes using MemoryPack serialization.
    /// </summary>
    /// <remarks>
    /// This test validates that the content hash is computed using MemoryPack binary serialization
    /// rather than JSON serialization, which provides better performance.
    /// </remarks>
    [Test]
    public async Task ComputeContentHash_MemoryPack_ProducesConsistentHash()
    {
        // Arrange
        var task1 = TaskEntry.CreateWithId("task-001", "First task", priority: 1);
        var task2 = TaskEntry.CreateWithId("task-002", "Second task", priority: 2);
        var tasks = new List<TaskEntry> { task1, task2 };
        var originalRequest = "Test request for hashing";

        // Create a ledger to get its hash
        var ledger = TaskLedger.Create(originalRequest, tasks);

        // Compute expected hash using MemoryPack serialization
        var hashContent = new TaskLedgerHashContent
        {
            OriginalRequest = originalRequest,
            TaskIds = tasks.Select(t => t.TaskId).ToList(),
            TaskDescriptions = tasks.Select(t => t.Description).ToList(),
        };
        var bytes = MemoryPackSerializer.Serialize(hashContent);
        var expectedHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        // Act
        var actualHash = ledger.ContentHash;

        // Assert - If using MemoryPack, hashes should match
        await Assert.That(actualHash).IsEqualTo(expectedHash);
    }

    /// <summary>
    /// Verifies that the same content always produces the same hash.
    /// </summary>
    [Test]
    public async Task ComputeContentHash_SameContent_ProducesSameHash()
    {
        // Arrange
        var task1 = TaskEntry.CreateWithId("task-001", "First task", priority: 1);
        var task2 = TaskEntry.CreateWithId("task-002", "Second task", priority: 2);
        var tasks = new List<TaskEntry> { task1, task2 };
        var originalRequest = "Test request for hashing";

        // Act
        var ledger1 = TaskLedger.Create(originalRequest, tasks);
        var ledger2 = TaskLedger.Create(originalRequest, tasks);

        // Assert - Same content should produce same hash
        await Assert.That(ledger1.ContentHash).IsEqualTo(ledger2.ContentHash);
    }

    /// <summary>
    /// Verifies that different content produces different hashes.
    /// </summary>
    [Test]
    public async Task ComputeContentHash_DifferentContent_ProducesDifferentHash()
    {
        // Arrange
        var task1 = TaskEntry.CreateWithId("task-001", "First task", priority: 1);
        var task2 = TaskEntry.CreateWithId("task-002", "Second task", priority: 2);
        var task3 = TaskEntry.CreateWithId("task-003", "Third task", priority: 3);

        var tasks1 = new List<TaskEntry> { task1, task2 };
        var tasks2 = new List<TaskEntry> { task1, task2, task3 };

        // Act
        var ledger1 = TaskLedger.Create("Test request", tasks1);
        var ledger2 = TaskLedger.Create("Test request", tasks2);

        // Assert - Different content should produce different hash
        await Assert.That(ledger1.ContentHash).IsNotEqualTo(ledger2.ContentHash);
    }

    /// <summary>
    /// Verifies that WithTask updates the content hash.
    /// </summary>
    [Test]
    public async Task WithTask_UpdatesContentHash()
    {
        // Arrange
        var task1 = TaskEntry.CreateWithId("task-001", "First task", priority: 1);
        var task2 = TaskEntry.CreateWithId("task-002", "Second task", priority: 2);
        var ledger = TaskLedger.Create("Test request", new List<TaskEntry> { task1 });

        // Act
        var updatedLedger = ledger.WithTask(task2);

        // Assert - Hash should be different after adding a task
        await Assert.That(updatedLedger.ContentHash).IsNotEqualTo(ledger.ContentHash);
    }
}

/// <summary>
/// MemoryPackable content structure for TaskLedger hashing.
/// </summary>
/// <remarks>
/// This type is used to compute content hashes using MemoryPack binary serialization.
/// It mirrors the structure used in TaskLedger.ComputeContentHash.
/// </remarks>
[MemoryPackable]
public partial class TaskLedgerHashContent
{
    /// <summary>
    /// Gets or sets the original request.
    /// </summary>
    public string OriginalRequest { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task IDs.
    /// </summary>
    public List<string> TaskIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the task descriptions.
    /// </summary>
    public List<string> TaskDescriptions { get; set; } = [];
}
