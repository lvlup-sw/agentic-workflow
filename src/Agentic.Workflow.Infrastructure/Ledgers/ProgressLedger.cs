// =============================================================================
// <copyright file="ProgressLedger.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Orchestration.Ledgers;

namespace Agentic.Workflow.Infrastructure.Ledgers;

/// <summary>
/// Represents the append-only progress ledger containing execution history.
/// </summary>
/// <remarks>
/// <para>
/// The progress ledger tracks WHAT HAS BEEN DONE. It provides a chronological record
/// of all executor actions, enabling:
/// <list type="bullet">
///   <item><description>Loop detection by analyzing recent entries</description></item>
///   <item><description>Workflow recovery by replaying from checkpoints</description></item>
///   <item><description>Observability by providing execution audit trails</description></item>
/// </list>
/// </para>
/// <para>
/// This is an immutable record. All modification methods return new instances.
/// </para>
/// </remarks>
public sealed record ProgressLedger : IProgressLedger
{
    /// <inheritdoc />
    public required string LedgerId { get; init; }

    /// <inheritdoc />
    public required string TaskLedgerId { get; init; }

    /// <inheritdoc />
    public required IReadOnlyList<ProgressEntry> Entries { get; init; }

    /// <summary>
    /// Gets the timestamp when this ledger was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the timestamp when this ledger was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates an empty progress ledger for a task ledger.
    /// </summary>
    /// <param name="taskLedgerId">The ID of the associated task ledger.</param>
    /// <returns>A new empty progress ledger.</returns>
    /// <exception cref="ArgumentNullException">Thrown when taskLedgerId is null.</exception>
    public static ProgressLedger Create(string taskLedgerId)
    {
        ArgumentNullException.ThrowIfNull(taskLedgerId, nameof(taskLedgerId));

        return new ProgressLedger
        {
            LedgerId = $"progress-ledger-{Guid.NewGuid():N}",
            TaskLedgerId = taskLedgerId,
            Entries = []
        };
    }

    /// <inheritdoc />
    public IProgressLedger WithEntry(ProgressEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry, nameof(entry));

        // Pre-allocate capacity to avoid internal reallocations
        var newEntries = new List<ProgressEntry>(Entries.Count + 1);
        newEntries.AddRange(Entries);
        newEntries.Add(entry);

        return this with
        {
            Entries = newEntries,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <inheritdoc />
    public IProgressLedger WithEntries(IEnumerable<ProgressEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries, nameof(entries));

        // Materialize to count for pre-allocation if not already a collection
        var entriesToAdd = entries as IReadOnlyCollection<ProgressEntry> ?? entries.ToList();

        // Pre-allocate capacity to avoid internal reallocations
        var newEntries = new List<ProgressEntry>(Entries.Count + entriesToAdd.Count);
        newEntries.AddRange(Entries);
        newEntries.AddRange(entriesToAdd);

        return this with
        {
            Entries = newEntries,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <inheritdoc />
    public IReadOnlyList<ProgressEntry> GetRecentEntries(int windowSize = 5)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(windowSize, nameof(windowSize));

        return Entries.TakeLast(windowSize).ToList();
    }

    /// <summary>
    /// Gets all entries for a specific task.
    /// </summary>
    /// <param name="taskId">The task ID to filter by.</param>
    /// <returns>Entries related to the specified task.</returns>
    /// <exception cref="ArgumentNullException">Thrown when taskId is null.</exception>
    public IEnumerable<ProgressEntry> GetEntriesForTask(string taskId)
    {
        ArgumentNullException.ThrowIfNull(taskId, nameof(taskId));

        return Entries.Where(e => e.TaskId == taskId);
    }

    /// <summary>
    /// Gets all entries since a specific timestamp.
    /// </summary>
    /// <param name="since">The timestamp to filter from.</param>
    /// <returns>Entries after the specified timestamp.</returns>
    public IEnumerable<ProgressEntry> GetEntriesSince(DateTimeOffset since)
    {
        return Entries.Where(e => e.Timestamp > since);
    }

    /// <inheritdoc />
    public ProgressLedgerMetrics GetMetrics()
    {
        var totalTokens = Entries.Sum(e => e.TokensConsumed);
        var totalDuration = Entries
            .Where(e => e.Duration.HasValue)
            .Aggregate(TimeSpan.Zero, (acc, e) => acc + e.Duration!.Value);
        var artifactCount = Entries.SelectMany(e => e.Artifacts).Distinct().Count();
        var successCount = Entries.Count(e => e.Signal?.Type == SignalType.Success);
        var failureCount = Entries.Count(e => e.Signal?.Type == SignalType.Failure);

        return new ProgressLedgerMetrics
        {
            TotalEntries = Entries.Count,
            TotalTokensConsumed = totalTokens,
            TotalDuration = totalDuration,
            UniqueArtifactCount = artifactCount,
            SuccessfulSignalCount = successCount,
            FailedSignalCount = failureCount
        };
    }
}
