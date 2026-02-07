// =============================================================================
// <copyright file="TaskEntry.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using MemoryPack;

namespace Agentic.Workflow.Orchestration.Ledgers;

/// <summary>
/// Represents a single task in the task ledger.
/// </summary>
/// <remarks>
/// <para>
/// Tasks are created during the planning phase when the orchestrator decomposes
/// the user request. Each task represents a discrete unit of work that can be
/// delegated to an executor.
/// </para>
/// <para>
/// Tasks may have dependencies on other tasks, forming a directed acyclic graph
/// that determines execution order.
/// </para>
/// </remarks>
[MemoryPackable]
public sealed partial record TaskEntry
{
    /// <summary>
    /// Gets the unique identifier for this task.
    /// </summary>
    public required string TaskId { get; init; }

    /// <summary>
    /// Gets the human-readable description of what needs to be accomplished.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the current status of this task.
    /// </summary>
    public WorkflowTaskStatus Status { get; init; } = WorkflowTaskStatus.Pending;

    /// <summary>
    /// Gets the priority of this task (higher = more important).
    /// </summary>
    /// <remarks>
    /// Used to determine execution order when multiple tasks are available.
    /// Default priority is 0.
    /// </remarks>
    public int Priority { get; init; }

    /// <summary>
    /// Gets the IDs of tasks that must complete before this task can start.
    /// </summary>
    /// <remarks>
    /// Forms a dependency graph. A task cannot be delegated until all
    /// dependencies have status Completed.
    /// </remarks>
    public IReadOnlyList<string> Dependencies { get; init; } = [];

    /// <summary>
    /// Gets the timestamp when this task was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the optional deadline for this task.
    /// </summary>
    /// <remarks>
    /// If set, the task should be completed before this time.
    /// Used for time-sensitive workflows.
    /// </remarks>
    public DateTimeOffset? Deadline { get; init; }

    /// <summary>
    /// Gets the result produced when this task completed successfully.
    /// </summary>
    /// <remarks>
    /// Captured from the executor signal during the review step.
    /// Contains the actual output/answer from the executor.
    /// </remarks>
    public string? Result { get; init; }

    /// <summary>
    /// Gets the preferred executor identifier for this task, if any.
    /// </summary>
    /// <remarks>
    /// Hints to the orchestrator which executor is best suited for this task.
    /// The orchestrator may choose a different executor based on availability
    /// and capability matching.
    /// </remarks>
    public string? PreferredExecutorId { get; init; }

    /// <summary>
    /// Gets the required capabilities for this task.
    /// </summary>
    /// <remarks>
    /// Used for discriminative executor selection. The selected executor
    /// must have capabilities that satisfy these requirements.
    /// </remarks>
    public Capability RequiredCapabilities { get; init; } = Capability.None;

    /// <summary>
    /// Gets optional metadata associated with this task.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Creates a new task with the specified description.
    /// </summary>
    /// <param name="description">The task description.</param>
    /// <param name="priority">Optional priority (default 0).</param>
    /// <param name="dependencies">Optional task dependencies.</param>
    /// <returns>A new task entry.</returns>
    /// <exception cref="ArgumentNullException">Thrown when description is null.</exception>
    public static TaskEntry Create(
        string description,
        int priority = 0,
        IReadOnlyList<string>? dependencies = null)
    {
        ArgumentNullException.ThrowIfNull(description, nameof(description));

        return new TaskEntry
        {
            TaskId = $"task-{Guid.NewGuid():N}",
            Description = description,
            Priority = priority,
            Dependencies = dependencies ?? []
        };
    }

    /// <summary>
    /// Creates a new task with an externally-provided TaskId.
    /// </summary>
    /// <param name="taskId">The unique identifier for this task.</param>
    /// <param name="description">The task description.</param>
    /// <param name="priority">Optional priority (default 0).</param>
    /// <param name="dependencies">Optional task dependencies.</param>
    /// <returns>A new task entry with the specified TaskId.</returns>
    /// <exception cref="ArgumentNullException">Thrown when taskId or description is null.</exception>
    /// <remarks>
    /// <para>
    /// Use this factory method when TaskId must be consistent across workflow handlers.
    /// The TaskId generated by the planning handler should flow through all commands to ensure
    /// the review handler can locate and update the correct TaskEntry.
    /// </para>
    /// </remarks>
    public static TaskEntry CreateWithId(
        string taskId,
        string description,
        int priority = 0,
        IReadOnlyList<string>? dependencies = null)
    {
        ArgumentNullException.ThrowIfNull(taskId, nameof(taskId));
        ArgumentNullException.ThrowIfNull(description, nameof(description));

        return new TaskEntry
        {
            TaskId = taskId,
            Description = description,
            Priority = priority,
            Dependencies = dependencies ?? []
        };
    }

    /// <summary>
    /// Creates a new task entry with an updated status.
    /// </summary>
    /// <param name="newStatus">The new status.</param>
    /// <returns>A new task entry with the updated status.</returns>
    public TaskEntry WithStatus(WorkflowTaskStatus newStatus)
    {
        return this with { Status = newStatus };
    }

    /// <summary>
    /// Creates a new task entry with the specified result.
    /// </summary>
    /// <param name="result">The result from task execution.</param>
    /// <returns>A new task entry with the result set.</returns>
    public TaskEntry WithResult(string result)
    {
        return this with { Result = result };
    }

    /// <summary>
    /// Determines if this task is ready to be executed (all dependencies satisfied).
    /// </summary>
    /// <param name="completedTaskIds">The set of completed task IDs.</param>
    /// <returns>True if all dependencies are satisfied; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when completedTaskIds is null.</exception>
    public bool IsReadyToExecute(IReadOnlySet<string> completedTaskIds)
    {
        ArgumentNullException.ThrowIfNull(completedTaskIds, nameof(completedTaskIds));

        return Status == WorkflowTaskStatus.Pending &&
               Dependencies.All(dep => completedTaskIds.Contains(dep));
    }
}