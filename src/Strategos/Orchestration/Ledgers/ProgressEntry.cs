// =============================================================================
// <copyright file="ProgressEntry.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using MemoryPack;

namespace Strategos.Orchestration.Ledgers;

/// <summary>
/// Represents a single entry in the append-only progress ledger.
/// </summary>
/// <remarks>
/// <para>
/// Progress entries form a chronological record of all executor actions during
/// workflow execution. Each entry captures what was done, who did it, and what
/// resulted.
/// </para>
/// <para>
/// The progress ledger is analyzed for loop detection and used for checkpointing.
/// </para>
/// </remarks>
[MemoryPackable]
public partial record ProgressEntry
{
    /// <summary>
    /// Gets the unique identifier for this entry.
    /// </summary>
    public required string EntryId { get; init; }

    /// <summary>
    /// Gets the ID of the task this entry relates to.
    /// </summary>
    public required string TaskId { get; init; }

    /// <summary>
    /// Gets the identifier of the executor that performed this action.
    /// </summary>
    public required string ExecutorId { get; init; }

    /// <summary>
    /// Gets a description of the action performed.
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Gets the output or result of the action.
    /// </summary>
    /// <remarks>
    /// May be truncated for large outputs. Full output may be stored in artifacts.
    /// </remarks>
    public string? Output { get; init; }

    /// <summary>
    /// Gets a value indicating whether observable progress was made.
    /// </summary>
    /// <remarks>
    /// Used by loop detection to identify NoProgress patterns.
    /// </remarks>
    public required bool ProgressMade { get; init; }

    /// <summary>
    /// Gets the paths to any artifacts produced during this action.
    /// </summary>
    public IReadOnlyList<string> Artifacts { get; init; } = [];

    /// <summary>
    /// Gets the timestamp when this action occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the duration of the action, if measured.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// Gets the number of tokens consumed by this action.
    /// </summary>
    public int TokensConsumed { get; init; }

    /// <summary>
    /// Gets the signal emitted by the executor, if this is a signaling entry.
    /// </summary>
    public ExecutorSignal? Signal { get; init; }

    /// <summary>
    /// Gets the state of the executor during this action.
    /// </summary>
    public ExecutorState ExecutorState { get; init; } = ExecutorState.Executing;

    /// <summary>
    /// Gets optional metadata associated with this entry.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Creates a progress entry for an executor action.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="executorId">The executor identifier.</param>
    /// <param name="action">Description of the action.</param>
    /// <param name="progressMade">Whether progress was made.</param>
    /// <param name="output">Optional output.</param>
    /// <returns>A new progress entry.</returns>
    public static ProgressEntry Create(
        string taskId,
        string executorId,
        string action,
        bool progressMade,
        string? output = null)
    {
        ArgumentNullException.ThrowIfNull(taskId, nameof(taskId));
        ArgumentNullException.ThrowIfNull(executorId, nameof(executorId));
        ArgumentNullException.ThrowIfNull(action, nameof(action));

        return new ProgressEntry
        {
            EntryId = $"progress-{Guid.NewGuid():N}",
            TaskId = taskId,
            ExecutorId = executorId,
            Action = action,
            ProgressMade = progressMade,
            Output = output
        };
    }

    /// <summary>
    /// Creates a progress entry for an executor signal.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="signal">The signal emitted.</param>
    /// <param name="action">Description of the signaling action.</param>
    /// <returns>A new progress entry with the signal.</returns>
    public static ProgressEntry FromSignal(
        string taskId,
        ExecutorSignal signal,
        string action)
    {
        ArgumentNullException.ThrowIfNull(taskId, nameof(taskId));
        ArgumentNullException.ThrowIfNull(signal, nameof(signal));
        ArgumentNullException.ThrowIfNull(action, nameof(action));

        return new ProgressEntry
        {
            EntryId = $"progress-{Guid.NewGuid():N}",
            TaskId = taskId,
            ExecutorId = signal.ExecutorId,
            Action = action,
            ProgressMade = signal.Type == SignalType.Success,
            Signal = signal,
            ExecutorState = ExecutorState.Signaling,
            Output = signal.SuccessData?.Result ?? signal.FailureData?.Reason,
            Artifacts = signal.SuccessData?.Artifacts ?? []
        };
    }
}
