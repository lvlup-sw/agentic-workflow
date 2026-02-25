// =============================================================================
// <copyright file="AgentSelectionContext.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Selection;

/// <summary>
/// Context for agent selection decisions within workflow steps.
/// </summary>
/// <remarks>
/// <para>
/// Provides all information needed by the agent selector to make an informed choice:
/// the workflow context, task description for classification, and available agent pool.
/// </para>
/// </remarks>
public sealed record AgentSelectionContext
{
    /// <summary>
    /// Gets the workflow instance ID requesting agent selection.
    /// </summary>
    /// <remarks>
    /// Used for correlation and audit purposes.
    /// </remarks>
    public required Guid WorkflowId { get; init; }

    /// <summary>
    /// Gets the name of the step requesting agent selection.
    /// </summary>
    /// <remarks>
    /// Useful for diagnostics and understanding selection patterns per workflow step.
    /// </remarks>
    public required string StepName { get; init; }

    /// <summary>
    /// Gets the task description to classify for agent selection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The task description is analyzed to determine the <see cref="TaskCategory"/>,
    /// which informs which belief distribution to sample from for each agent.
    /// </para>
    /// <para>
    /// More specific descriptions enable better classification and thus better
    /// agent matching. For example, "implement a binary search algorithm" clearly
    /// indicates code generation, while "do the thing" defaults to General.
    /// </para>
    /// </remarks>
    public required string TaskDescription { get; init; }

    /// <summary>
    /// Gets the list of available agent IDs to choose from.
    /// </summary>
    /// <remarks>
    /// These are the candidate agents eligible for selection. The selector will
    /// sample from each agent's belief distribution and select the highest sample.
    /// </remarks>
    public required IReadOnlyList<string> AvailableAgents { get; init; }

    /// <summary>
    /// Gets optional agents to exclude from selection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this to prevent selecting agents that have recently failed or are
    /// temporarily unavailable. Excluded agents are removed from candidates
    /// before sampling.
    /// </para>
    /// </remarks>
    public IReadOnlyList<string>? ExcludedAgents { get; init; }
}
