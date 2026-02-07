// =============================================================================
// <copyright file="IWorkflowEvent.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Events;

/// <summary>
/// Base interface for all workflow events in the event sourcing system.
/// </summary>
/// <remarks>
/// <para>
/// All workflow events implement this interface to ensure they can be stored in
/// event streams and projected into read models.
/// </para>
/// <para>
/// The <see cref="WorkflowId"/> serves as the stream identity, grouping all events
/// for a single workflow execution.
/// </para>
/// </remarks>
public interface IWorkflowEvent
{
    /// <summary>
    /// Gets the unique identifier for the workflow this event belongs to.
    /// </summary>
    /// <remarks>
    /// This serves as the event stream identity. All events with the same
    /// WorkflowId are stored in the same stream and projected together.
    /// </remarks>
    Guid WorkflowId { get; }

    /// <summary>
    /// Gets the timestamp when this event occurred.
    /// </summary>
    /// <remarks>
    /// Events are ordered by timestamp within a stream for projection and replay.
    /// </remarks>
    DateTimeOffset Timestamp { get; }
}