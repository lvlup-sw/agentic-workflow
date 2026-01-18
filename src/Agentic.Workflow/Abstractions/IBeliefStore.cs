// =============================================================================
// <copyright file="IBeliefStore.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Primitives;
using Agentic.Workflow.Selection;

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Contract for persisting agent belief states for Thompson Sampling agent selection.
/// </summary>
/// <remarks>
/// <para>
/// The belief store maintains Beta distribution parameters for each (agent, task category)
/// pair. These beliefs are updated after each agent execution based on success/failure
/// outcomes, enabling online learning of agent performance across different task types.
/// </para>
/// <para>
/// Implementations may use in-memory storage for testing or event-sourced storage
/// (e.g., Marten) for production use with persistence and audit trails.
/// </para>
/// </remarks>
public interface IBeliefStore
{
    /// <summary>
    /// Gets the belief state for a specific agent and task category.
    /// </summary>
    /// <param name="agentId">The agent identifier.</param>
    /// <param name="taskCategory">The task category.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>
    /// The belief state if found, or a default prior if no belief exists.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method always returns a valid belief - if no prior observations exist,
    /// it returns a default prior belief with Alpha=2, Beta=2 (uninformative prior).
    /// </para>
    /// <para>
    /// Returns <see cref="ValueTask{TResult}"/> to avoid allocations when the belief
    /// is retrieved synchronously from an in-memory cache.
    /// </para>
    /// </remarks>
    ValueTask<Result<AgentBelief>> GetBeliefAsync(
        string agentId,
        string taskCategory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the belief state for an agent based on task execution outcome.
    /// </summary>
    /// <param name="agentId">The agent identifier.</param>
    /// <param name="taskCategory">The task category.</param>
    /// <param name="success">True if the agent succeeded; false if failed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A result indicating success or failure of the update operation.</returns>
    /// <remarks>
    /// <para>
    /// On success, Alpha is incremented by 1. On failure, Beta is incremented by 1.
    /// </para>
    /// <para>
    /// If no prior belief exists for this (agent, task category) pair, a new belief
    /// is created with default prior values before applying the update.
    /// </para>
    /// <para>
    /// Returns <see cref="ValueTask{TResult}"/> to avoid allocations when the update
    /// completes synchronously against an in-memory store.
    /// </para>
    /// </remarks>
    ValueTask<Result<Unit>> UpdateBeliefAsync(
        string agentId,
        string taskCategory,
        bool success,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all beliefs for a specific agent across all task categories.
    /// </summary>
    /// <param name="agentId">The agent identifier.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of all beliefs for the specified agent.</returns>
    /// <remarks>
    /// <para>
    /// Returns an empty list if no beliefs exist for the agent.
    /// </para>
    /// <para>
    /// Returns <see cref="ValueTask{TResult}"/> to avoid allocations when beliefs
    /// are retrieved synchronously from an in-memory index.
    /// </para>
    /// </remarks>
    ValueTask<Result<IReadOnlyList<AgentBelief>>> GetBeliefsForAgentAsync(
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all beliefs for a specific task category across all agents.
    /// </summary>
    /// <param name="taskCategory">The task category.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of all beliefs for the specified task category.</returns>
    /// <remarks>
    /// <para>
    /// Returns an empty list if no beliefs exist for the task category.
    /// </para>
    /// <para>
    /// Returns <see cref="ValueTask{TResult}"/> to avoid allocations when beliefs
    /// are retrieved synchronously from an in-memory index.
    /// </para>
    /// </remarks>
    ValueTask<Result<IReadOnlyList<AgentBelief>>> GetBeliefsForCategoryAsync(
        string taskCategory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a belief state directly to the store.
    /// </summary>
    /// <param name="belief">The belief to save.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A result indicating success or failure of the save operation.</returns>
    /// <remarks>
    /// <para>
    /// This method allows saving a belief with arbitrary Alpha/Beta values, enabling
    /// partial credit updates via <see cref="AgentBelief.WithOutcome"/>.
    /// </para>
    /// <para>
    /// If a belief already exists for the same (agent, task category) pair, it is replaced.
    /// </para>
    /// <para>
    /// Returns <see cref="ValueTask{TResult}"/> to avoid allocations when the save
    /// completes synchronously against an in-memory store.
    /// </para>
    /// </remarks>
    ValueTask<Result<Unit>> SaveBeliefAsync(
        AgentBelief belief,
        CancellationToken cancellationToken = default);
}
