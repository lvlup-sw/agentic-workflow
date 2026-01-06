// =============================================================================
// <copyright file="WorkflowStarted.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Events;

/// <summary>
/// Event raised when a new workflow begins execution.
/// </summary>
/// <remarks>
/// <para>
/// This is the first event in any workflow stream and initializes the
/// progress ledger read model.
/// </para>
/// <para>
/// The <see cref="OriginalRequest"/> captures the user's initial input that
/// triggered the workflow.
/// </para>
/// </remarks>
/// <param name="WorkflowId">The unique identifier for this workflow.</param>
/// <param name="OriginalRequest">The original user request that initiated the workflow.</param>
/// <param name="Timestamp">The timestamp when the workflow started.</param>
public sealed record WorkflowStarted(
    Guid WorkflowId,
    string OriginalRequest,
    DateTimeOffset Timestamp) : IWorkflowEvent;
