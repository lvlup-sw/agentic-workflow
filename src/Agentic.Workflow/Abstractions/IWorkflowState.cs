// =============================================================================
// <copyright file="IWorkflowState.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Base contract for all workflow state types.
/// </summary>
/// <remarks>
/// <para>
/// All workflow state must implement this interface to ensure proper
/// tracking and event sourcing via Marten.
/// </para>
/// <para>
/// State records should be immutable with <c>{ get; init; }</c> properties.
/// </para>
/// </remarks>
public interface IWorkflowState
{
    /// <summary>
    /// Gets the unique identifier for this workflow instance.
    /// </summary>
    Guid WorkflowId { get; }
}
