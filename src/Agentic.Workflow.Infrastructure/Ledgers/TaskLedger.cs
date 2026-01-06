// =============================================================================
// <copyright file="TaskLedger.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Orchestration.Ledgers;

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

        var newTasks = Tasks.Append(task).ToList();
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

        var newTasks = Tasks.Select(t => t.TaskId == taskId ? updatedTask : t).ToList();

        if (!newTasks.Any(t => t.TaskId == taskId))
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
