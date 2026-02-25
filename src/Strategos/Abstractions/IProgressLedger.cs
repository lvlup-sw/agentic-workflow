// =============================================================================
// <copyright file="IProgressLedger.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Orchestration.Ledgers;

namespace Strategos.Abstractions;

/// <summary>
/// Contract for an append-only ledger that tracks workflow execution progress.
/// </summary>
/// <remarks>
/// <para>
/// The progress ledger tracks WHAT HAS BEEN DONE during workflow execution.
/// It provides a chronological record of all executor actions, enabling:
/// <list type="bullet">
///   <item><description>Loop detection by analyzing recent entries</description></item>
///   <item><description>Workflow recovery by replaying from checkpoints</description></item>
///   <item><description>Observability by providing execution audit trails</description></item>
/// </list>
/// </para>
/// <para>
/// Implementations should be immutable. Operations that modify the ledger
/// return new instances rather than mutating in place.
/// </para>
/// </remarks>
public interface IProgressLedger
{
    /// <summary>
    /// Gets the unique identifier for this ledger.
    /// </summary>
    string LedgerId { get; }

    /// <summary>
    /// Gets the ID of the task ledger this progress ledger tracks.
    /// </summary>
    string TaskLedgerId { get; }

    /// <summary>
    /// Gets the chronological list of progress entries.
    /// </summary>
    IReadOnlyList<ProgressEntry> Entries { get; }

    /// <summary>
    /// Gets the timestamp when this ledger was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the timestamp when this ledger was last updated.
    /// </summary>
    DateTimeOffset UpdatedAt { get; }

    /// <summary>
    /// Creates a new ledger with an additional entry appended.
    /// </summary>
    /// <param name="entry">The entry to append.</param>
    /// <returns>A new ledger with the entry appended.</returns>
    IProgressLedger WithEntry(ProgressEntry entry);

    /// <summary>
    /// Creates a new ledger with multiple entries appended.
    /// </summary>
    /// <param name="entries">The entries to append.</param>
    /// <returns>A new ledger with the entries appended.</returns>
    IProgressLedger WithEntries(IEnumerable<ProgressEntry> entries);

    /// <summary>
    /// Gets the most recent entries within a sliding window.
    /// </summary>
    /// <param name="windowSize">The number of entries to return.</param>
    /// <returns>The most recent entries.</returns>
    IReadOnlyList<ProgressEntry> GetRecentEntries(int windowSize = 5);

    /// <summary>
    /// Gets all entries for a specific task.
    /// </summary>
    /// <param name="taskId">The task ID to filter by.</param>
    /// <returns>Entries related to the specified task.</returns>
    IEnumerable<ProgressEntry> GetEntriesForTask(string taskId);

    /// <summary>
    /// Gets all entries since a specific timestamp.
    /// </summary>
    /// <param name="since">The timestamp to filter from.</param>
    /// <returns>Entries after the specified timestamp.</returns>
    IEnumerable<ProgressEntry> GetEntriesSince(DateTimeOffset since);

    /// <summary>
    /// Calculates aggregate metrics for the ledger.
    /// </summary>
    /// <returns>Aggregate metrics including total tokens, duration, and artifact count.</returns>
    ProgressLedgerMetrics GetMetrics();
}
