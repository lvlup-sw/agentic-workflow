// =============================================================================
// <copyright file="TaskLedger.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Security.Cryptography;

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Orchestration.Ledgers;

using MemoryPack;

namespace Agentic.Workflow.Infrastructure.Ledgers;

/// <summary>
/// Represents the immutable task ledger containing the goal specification for a workflow.
/// </summary>
/// <remarks>
/// <para>
/// The task ledger defines WHAT needs to be done. It is created during the PLANNING phase
/// and remains append-only throughout the workflow. New tasks can be added but existing
/// tasks cannot be removed (though they can be marked as skipped).
/// </para>
/// <para>
/// The task ledger is part of the recoverable state tuple used for workflow checkpointing.
/// </para>
/// </remarks>
public sealed record TaskLedger : ITaskLedger
{
    /// <inheritdoc />
    public required string LedgerId { get; init; }

    /// <inheritdoc />
    public required string OriginalRequest { get; init; }

    /// <inheritdoc />
    public required IReadOnlyList<TaskEntry> Tasks { get; init; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public required string ContentHash { get; init; }

    /// <summary>
    /// Creates a new task ledger from a user request and decomposed tasks.
    /// </summary>
    /// <param name="originalRequest">The original user request.</param>
    /// <param name="tasks">The decomposed tasks.</param>
    /// <returns>A new task ledger with computed content hash.</returns>
    /// <exception cref="ArgumentNullException">Thrown when originalRequest or tasks is null.</exception>
    public static TaskLedger Create(string originalRequest, IReadOnlyList<TaskEntry> tasks)
    {
        ArgumentNullException.ThrowIfNull(originalRequest, nameof(originalRequest));
        ArgumentNullException.ThrowIfNull(tasks, nameof(tasks));

        var ledgerId = $"ledger-{Guid.NewGuid():N}";
        var contentHash = ComputeContentHash(originalRequest, tasks);

        return new TaskLedger
        {
            LedgerId = ledgerId,
            OriginalRequest = originalRequest,
            Tasks = tasks,
            ContentHash = contentHash
        };
    }

    /// <inheritdoc />
    public ITaskLedger WithTask(TaskEntry task)
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));

        var newTasks = new List<TaskEntry>(Tasks.Count + 1);
        newTasks.AddRange(Tasks);
        newTasks.Add(task);

        var newHash = ComputeContentHash(OriginalRequest, newTasks);

        return this with
        {
            Tasks = newTasks,
            ContentHash = newHash
        };
    }

    /// <inheritdoc />
    public ITaskLedger WithUpdatedTask(string taskId, TaskEntry updatedTask)
    {
        ArgumentNullException.ThrowIfNull(taskId, nameof(taskId));
        ArgumentNullException.ThrowIfNull(updatedTask, nameof(updatedTask));

        var newTasks = new List<TaskEntry>(Tasks.Count);
        var found = false;

        foreach (var t in Tasks)
        {
            if (t.TaskId == taskId)
            {
                newTasks.Add(updatedTask);
                found = true;
            }
            else
            {
                newTasks.Add(t);
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException($"Task with ID '{taskId}' not found in ledger.");
        }

        var newHash = ComputeContentHash(OriginalRequest, newTasks);

        return this with
        {
            Tasks = newTasks,
            ContentHash = newHash
        };
    }

    /// <inheritdoc />
    public IEnumerable<TaskEntry> GetReadyTasks()
    {
        var completedIds = Tasks
            .Where(t => t.Status == WorkflowTaskStatus.Completed)
            .Select(t => t.TaskId)
            .ToHashSet();

        return Tasks
            .Where(t => t.IsReadyToExecute(completedIds))
            .OrderByDescending(t => t.Priority);
    }

    /// <inheritdoc />
    public bool IsComplete()
    {
        return Tasks.All(t =>
            t.Status == WorkflowTaskStatus.Completed ||
            t.Status == WorkflowTaskStatus.Skipped ||
            t.Status == WorkflowTaskStatus.Failed);
    }

    /// <inheritdoc />
    public bool VerifyIntegrity()
    {
        var computedHash = ComputeContentHash(OriginalRequest, Tasks);
        return string.Equals(ContentHash, computedHash, StringComparison.Ordinal);
    }

    /// <summary>
    /// Computes a SHA-256 hash of the ledger content.
    /// </summary>
    private static string ComputeContentHash(string originalRequest, IReadOnlyList<TaskEntry> tasks)
    {
        var content = new TaskLedgerHashContent
        {
            OriginalRequest = originalRequest,
            TaskIds = tasks.Select(t => t.TaskId).ToList(),
            TaskDescriptions = tasks.Select(t => t.Description).ToList(),
        };

        var bytes = MemoryPackSerializer.Serialize(content);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
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
public sealed partial class TaskLedgerHashContent
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