// =============================================================================
// <copyright file="ITaskLedger.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Orchestration.Ledgers;

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Contract for the immutable task ledger containing the goal specification for a workflow.
/// </summary>
/// <remarks>
/// <para>
/// The task ledger defines WHAT needs to be done. It is created during the planning phase
/// and remains append-only throughout the workflow. New tasks can be added but existing
/// tasks cannot be removed (though they can be marked as skipped).
/// </para>
/// <para>
/// The task ledger is part of the recoverable state tuple used for workflow checkpointing.
/// </para>
/// </remarks>
public interface ITaskLedger
{
    /// <summary>
    /// Gets the unique identifier for this ledger.
    /// </summary>
    string LedgerId { get; }

    /// <summary>
    /// Gets the original user request that initiated this workflow.
    /// </summary>
    string OriginalRequest { get; }

    /// <summary>
    /// Gets the list of tasks derived from the original request.
    /// </summary>
    IReadOnlyList<TaskEntry> Tasks { get; }

    /// <summary>
    /// Gets the timestamp when this ledger was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the content hash for integrity verification.
    /// </summary>
    /// <remarks>
    /// SHA-256 hash of the original request and task definitions.
    /// Used during checkpoint recovery to verify ledger integrity.
    /// </remarks>
    string ContentHash { get; }

    /// <summary>
    /// Creates a new ledger with an additional task.
    /// </summary>
    /// <param name="task">The task to add.</param>
    /// <returns>A new ledger with the additional task.</returns>
    /// <exception cref="ArgumentNullException">Thrown when task is null.</exception>
    ITaskLedger WithTask(TaskEntry task);

    /// <summary>
    /// Creates a new ledger with an updated task.
    /// </summary>
    /// <param name="taskId">The ID of the task to update.</param>
    /// <param name="updatedTask">The updated task entry.</param>
    /// <returns>A new ledger with the updated task.</returns>
    /// <exception cref="ArgumentNullException">Thrown when taskId or updatedTask is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when task ID is not found.</exception>
    ITaskLedger WithUpdatedTask(string taskId, TaskEntry updatedTask);

    /// <summary>
    /// Gets all tasks that are ready to be executed (pending with satisfied dependencies).
    /// </summary>
    /// <returns>Tasks ready for delegation, ordered by priority (descending).</returns>
    IEnumerable<TaskEntry> GetReadyTasks();

    /// <summary>
    /// Checks if all tasks have been completed or skipped.
    /// </summary>
    /// <returns>True if the workflow is complete; otherwise, false.</returns>
    bool IsComplete();

    /// <summary>
    /// Verifies the content hash matches the current content.
    /// </summary>
    /// <returns>True if the hash is valid; otherwise, false.</returns>
    bool VerifyIntegrity();
}