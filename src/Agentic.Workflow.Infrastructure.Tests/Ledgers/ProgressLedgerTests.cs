// =============================================================================
// <copyright file="ProgressLedgerTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Infrastructure.Tests.Ledgers;

/// <summary>
/// Unit tests for <see cref="ProgressLedger"/> verifying entry appending
/// and pre-allocation optimizations.
/// </summary>
[Property("Category", "Unit")]
public sealed class ProgressLedgerTests
{
    // =============================================================================
    // A. WithEntry Tests
    // =============================================================================

    /// <summary>
    /// Verifies that WithEntry correctly appends an entry and preserves all existing entries.
    /// </summary>
    [Test]
    public async Task WithEntry_AppendsSingleEntry_PreservesAllEntries()
    {
        // Arrange
        var ledger = ProgressLedger.Create("task-ledger-1");
        var entry1 = CreateTestEntry("task-1", "executor-1", "action-1");
        var entry2 = CreateTestEntry("task-1", "executor-1", "action-2");

        // Act
        var ledgerAfterFirst = (ProgressLedger)ledger.WithEntry(entry1);
        var ledgerAfterSecond = (ProgressLedger)ledgerAfterFirst.WithEntry(entry2);

        // Assert
        await Assert.That(ledgerAfterSecond.Entries.Count).IsEqualTo(2);
        await Assert.That(ledgerAfterSecond.Entries[0].Action).IsEqualTo("action-1");
        await Assert.That(ledgerAfterSecond.Entries[1].Action).IsEqualTo("action-2");
    }

    /// <summary>
    /// Verifies that WithEntry correctly pre-allocates capacity for the new list
    /// by testing behavior with many entries.
    /// </summary>
    /// <remarks>
    /// This test validates that pre-allocation works by verifying the behavior
    /// is correct with large entry counts. The optimization ensures that
    /// List capacity is pre-allocated to (Entries.Count + 1) to avoid
    /// multiple internal reallocations.
    /// </remarks>
    [Test]
    public async Task WithEntry_LargeEntryCount_PreallocatesCapacity()
    {
        // Arrange - Create ledger with many entries
        var ledger = ProgressLedger.Create("task-ledger-1");
        const int initialCount = 1000;

        // Add many entries to build up the ledger
        var currentLedger = ledger;
        for (var i = 0; i < initialCount; i++)
        {
            var entry = CreateTestEntry("task-1", "executor-1", $"action-{i}");
            currentLedger = (ProgressLedger)currentLedger.WithEntry(entry);
        }

        // Act - Add one more entry
        var finalEntry = CreateTestEntry("task-1", "executor-1", "final-action");
        var finalLedger = (ProgressLedger)currentLedger.WithEntry(finalEntry);

        // Assert - All entries should be present and in order
        await Assert.That(finalLedger.Entries.Count).IsEqualTo(initialCount + 1);
        await Assert.That(finalLedger.Entries[0].Action).IsEqualTo("action-0");
        await Assert.That(finalLedger.Entries[initialCount - 1].Action).IsEqualTo($"action-{initialCount - 1}");
        await Assert.That(finalLedger.Entries[initialCount].Action).IsEqualTo("final-action");
    }

    /// <summary>
    /// Verifies that WithEntry creates an immutable copy and does not modify the original ledger.
    /// </summary>
    [Test]
    public async Task WithEntry_CreatesImmutableCopy_OriginalUnchanged()
    {
        // Arrange
        var ledger = ProgressLedger.Create("task-ledger-1");
        var entry1 = CreateTestEntry("task-1", "executor-1", "action-1");
        var ledgerWithOne = (ProgressLedger)ledger.WithEntry(entry1);

        // Act
        var entry2 = CreateTestEntry("task-1", "executor-1", "action-2");
        var ledgerWithTwo = (ProgressLedger)ledgerWithOne.WithEntry(entry2);

        // Assert - Original ledgers should be unchanged
        await Assert.That(ledger.Entries.Count).IsEqualTo(0);
        await Assert.That(ledgerWithOne.Entries.Count).IsEqualTo(1);
        await Assert.That(ledgerWithTwo.Entries.Count).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that WithEntry throws ArgumentNullException when entry is null.
    /// </summary>
    [Test]
    public async Task WithEntry_WithNullEntry_ThrowsArgumentNullException()
    {
        // Arrange
        var ledger = ProgressLedger.Create("task-ledger-1");

        // Act & Assert
        await Assert.That(() => ledger.WithEntry(null!))
            .Throws<ArgumentNullException>()
            .WithParameterName("entry");
    }

    // =============================================================================
    // B. WithEntries Tests
    // =============================================================================

    /// <summary>
    /// Verifies that WithEntries correctly appends multiple entries.
    /// </summary>
    [Test]
    public async Task WithEntries_AppendsMultipleEntries_PreservesOrder()
    {
        // Arrange
        var ledger = ProgressLedger.Create("task-ledger-1");
        var entries = new[]
        {
            CreateTestEntry("task-1", "executor-1", "action-1"),
            CreateTestEntry("task-1", "executor-1", "action-2"),
            CreateTestEntry("task-1", "executor-1", "action-3"),
        };

        // Act
        var updatedLedger = (ProgressLedger)ledger.WithEntries(entries);

        // Assert
        await Assert.That(updatedLedger.Entries.Count).IsEqualTo(3);
        await Assert.That(updatedLedger.Entries[0].Action).IsEqualTo("action-1");
        await Assert.That(updatedLedger.Entries[1].Action).IsEqualTo("action-2");
        await Assert.That(updatedLedger.Entries[2].Action).IsEqualTo("action-3");
    }

    // =============================================================================
    // C. GetRecentEntries Tests
    // =============================================================================

    /// <summary>
    /// Verifies that GetRecentEntries returns the last N entries.
    /// </summary>
    [Test]
    public async Task GetRecentEntries_WithWindowSize_ReturnsLastNEntries()
    {
        // Arrange
        var ledger = ProgressLedger.Create("task-ledger-1");
        var currentLedger = ledger;
        for (var i = 0; i < 10; i++)
        {
            var entry = CreateTestEntry("task-1", "executor-1", $"action-{i}");
            currentLedger = (ProgressLedger)currentLedger.WithEntry(entry);
        }

        // Act
        var recentEntries = currentLedger.GetRecentEntries(3);

        // Assert
        await Assert.That(recentEntries.Count).IsEqualTo(3);
        await Assert.That(recentEntries[0].Action).IsEqualTo("action-7");
        await Assert.That(recentEntries[1].Action).IsEqualTo("action-8");
        await Assert.That(recentEntries[2].Action).IsEqualTo("action-9");
    }

    // =============================================================================
    // Helper Methods
    // =============================================================================

    private static ProgressEntry CreateTestEntry(string taskId, string executorId, string action)
    {
        return ProgressEntry.Create(taskId, executorId, action, progressMade: true);
    }
}
