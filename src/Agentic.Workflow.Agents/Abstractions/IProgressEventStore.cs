// =============================================================================
// <copyright file="IProgressEventStore.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Agents.Abstractions;

/// <summary>
/// Contract for persisting and retrieving progress events from event streams.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the event store operations for progress tracking,
/// enabling event sourcing patterns for workflow orchestration.
/// </para>
/// <para>
/// Events are organized by workflow ID, with each workflow having its own
/// event stream. Events are appended immutably and can be replayed to
/// reconstruct workflow state.
/// </para>
/// </remarks>
public interface IProgressEventStore
{
    /// <summary>
    /// Appends a single progress event to the workflow's event stream.
    /// </summary>
    /// <param name="evt">The progress event to append.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="evt"/> is null.
    /// </exception>
    Task AppendEventAsync(IProgressEvent evt, CancellationToken ct = default);

    /// <summary>
    /// Appends multiple progress events to a workflow's event stream atomically.
    /// </summary>
    /// <param name="workflowId">The workflow ID identifying the event stream.</param>
    /// <param name="events">The progress events to append.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="events"/> is null.
    /// </exception>
    Task AppendEventsAsync(Guid workflowId, IEnumerable<IProgressEvent> events, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all progress events for a specific workflow.
    /// </summary>
    /// <param name="workflowId">The workflow ID to fetch events for.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A read-only list of progress events in chronological order.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="workflowId"/> is empty.
    /// </exception>
    Task<IReadOnlyList<IProgressEvent>> GetEventsAsync(Guid workflowId, CancellationToken ct = default);
}