// =============================================================================
// <copyright file="BranchPointDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Immutable definition of a branch point in a workflow.
/// </summary>
/// <remarks>
/// <para>
/// A branch point represents a decision point where the workflow can take
/// one of several paths based on state conditions. Each path is defined
/// by a <see cref="BranchPathDefinition"/>.
/// </para>
/// <para>
/// Branch points can optionally rejoin at a common step after the branch.
/// </para>
/// </remarks>
public sealed record BranchPointDefinition
{
    /// <summary>
    /// Gets the unique identifier for this branch point.
    /// </summary>
    public required string BranchPointId { get; init; }

    /// <summary>
    /// Gets the step ID where this branch originates from.
    /// </summary>
    public required string FromStepId { get; init; }

    /// <summary>
    /// Gets the branch paths available at this branch point.
    /// </summary>
    public IReadOnlyList<BranchPathDefinition> Paths { get; init; } = [];

    /// <summary>
    /// Gets the step ID where branches rejoin (null if branches don't rejoin).
    /// </summary>
    public string? RejoinStepId { get; init; }

    /// <summary>
    /// Creates a new branch point definition.
    /// </summary>
    /// <param name="fromStepId">The step ID where branching occurs.</param>
    /// <param name="paths">The available branch paths.</param>
    /// <param name="rejoinStepId">The step ID where branches rejoin (optional).</param>
    /// <returns>A new branch point definition.</returns>
    public static BranchPointDefinition Create(
        string fromStepId,
        IReadOnlyList<BranchPathDefinition> paths,
        string? rejoinStepId = null)
    {
        ArgumentNullException.ThrowIfNull(fromStepId, nameof(fromStepId));
        ArgumentNullException.ThrowIfNull(paths, nameof(paths));

        return new BranchPointDefinition
        {
            BranchPointId = Guid.NewGuid().ToString("N"),
            FromStepId = fromStepId,
            Paths = paths,
            RejoinStepId = rejoinStepId,
        };
    }
}