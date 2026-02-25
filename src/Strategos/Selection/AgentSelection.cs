// =============================================================================
// <copyright file="AgentSelection.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Selection;

/// <summary>
/// Result of agent selection via Thompson Sampling.
/// </summary>
/// <remarks>
/// <para>
/// Contains the selected agent along with diagnostic information about the
/// selection process, including the inferred task category and sampling metrics.
/// </para>
/// </remarks>
public sealed record AgentSelection
{
    /// <summary>
    /// Gets the selected agent ID.
    /// </summary>
    /// <remarks>
    /// This is the agent that had the highest sampled success probability
    /// from its Beta distribution for the inferred task category.
    /// </remarks>
    public required string SelectedAgentId { get; init; }

    /// <summary>
    /// Gets the classified task category used for selection.
    /// </summary>
    /// <remarks>
    /// Determined by analyzing the task description. Each agent has a separate
    /// belief distribution per task category.
    /// </remarks>
    public required TaskCategory TaskCategory { get; init; }

    /// <summary>
    /// Gets the sampled theta value for the selected agent (diagnostic).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the value sampled from Beta(α, β) that won the selection.
    /// Range: [0, 1], representing sampled success probability.
    /// </para>
    /// <para>
    /// Useful for debugging and understanding why an agent was selected.
    /// Higher values indicate the sampling favored this agent strongly.
    /// </para>
    /// </remarks>
    public double SampledTheta { get; init; }

    /// <summary>
    /// Gets the confidence in the selection based on observation count.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Range: [0, 1], computed as min(1.0, observationCount / 20.0).
    /// </para>
    /// <para>
    /// Low confidence (few observations) indicates more exploration is occurring.
    /// High confidence (many observations) indicates more exploitation.
    /// </para>
    /// </remarks>
    public double SelectionConfidence { get; init; }

    /// <summary>
    /// Gets the extracted task features used for selection (optional).
    /// </summary>
    /// <remarks>
    /// <para>
    /// When set, provides diagnostic information about the features extracted
    /// from the task description that informed the selection process.
    /// </para>
    /// <para>
    /// Includes:
    /// <list type="bullet">
    ///   <item><description>Category classification</description></item>
    ///   <item><description>Complexity estimation</description></item>
    ///   <item><description>Matched keywords</description></item>
    ///   <item><description>Custom features (for extensibility)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public TaskFeatures? Features { get; init; }
}
