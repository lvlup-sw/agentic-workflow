// =============================================================================
// <copyright file="IAgentSelector.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Primitives;
using Strategos.Selection;

namespace Strategos.Abstractions;

/// <summary>
/// Contract for selecting agents within workflow steps using learned preferences.
/// </summary>
/// <remarks>
/// <para>
/// The agent selector uses Thompson Sampling to balance exploration (trying different agents)
/// with exploitation (using agents known to perform well) for each task category.
/// </para>
/// <para>
/// Selection is contextual: the same agent may be preferred for code generation tasks
/// but not for data analysis, based on observed performance in each category.
/// </para>
/// </remarks>
public interface IAgentSelector
{
    /// <summary>
    /// Selects an agent from available candidates for the given context.
    /// </summary>
    /// <param name="context">The selection context including task description and available agents.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>
    /// The selected agent along with classification metadata, or an error if selection fails.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Selection uses Thompson Sampling: for each candidate agent, a success probability
    /// is sampled from that agent's Beta distribution for the inferred task category.
    /// The agent with the highest sampled probability is selected.
    /// </para>
    /// <para>
    /// Returns an error if no available agents remain after applying exclusions.
    /// </para>
    /// </remarks>
    Task<Result<AgentSelection>> SelectAgentAsync(
        AgentSelectionContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records the outcome of an agent execution to update beliefs.
    /// </summary>
    /// <param name="agentId">The agent that was executed.</param>
    /// <param name="taskCategory">The task category that was performed.</param>
    /// <param name="outcome">The execution outcome.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A result indicating success or failure of the update.</returns>
    /// <remarks>
    /// <para>
    /// Call this method after an agent completes a task to update the belief model.
    /// Success outcomes increase the agent's expected success rate for that task category;
    /// failure outcomes decrease it.
    /// </para>
    /// </remarks>
    Task<Result<Unit>> RecordOutcomeAsync(
        string agentId,
        string taskCategory,
        AgentOutcome outcome,
        CancellationToken cancellationToken = default);
}
