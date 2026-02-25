// =============================================================================
// <copyright file="ForkPointDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Definitions;

/// <summary>
/// Immutable definition of a fork point in a workflow.
/// </summary>
/// <remarks>
/// <para>
/// A fork point represents a location where the workflow splits into multiple
/// parallel execution paths. All paths execute concurrently and must complete
/// (or handle failures) before the join step executes.
/// </para>
/// <para>
/// Unlike branches (which select one path based on a condition), forks execute
/// all paths simultaneously.
/// </para>
/// </remarks>
public sealed record ForkPointDefinition
{
    /// <summary>
    /// Gets the unique identifier for this fork point.
    /// </summary>
    public required string ForkPointId { get; init; }

    /// <summary>
    /// Gets the step ID where this fork originates from.
    /// </summary>
    public required string FromStepId { get; init; }

    /// <summary>
    /// Gets the parallel paths in this fork.
    /// </summary>
    /// <remarks>
    /// A fork must have at least two paths to be meaningful.
    /// All paths execute concurrently.
    /// </remarks>
    public IReadOnlyList<ForkPathDefinition> Paths { get; init; } = [];

    /// <summary>
    /// Gets the step ID where all paths converge (the join step).
    /// </summary>
    /// <remarks>
    /// The join step receives merged state from all completed paths
    /// and executes only after all paths reach a terminal status.
    /// </remarks>
    public required string JoinStepId { get; init; }

    /// <summary>
    /// Creates a new fork point definition.
    /// </summary>
    /// <param name="fromStepId">The step ID where forking occurs.</param>
    /// <param name="paths">The parallel paths (minimum 2 required).</param>
    /// <param name="joinStepId">The step ID where paths converge.</param>
    /// <returns>A new fork point definition.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="fromStepId"/>, <paramref name="paths"/>,
    /// or <paramref name="joinStepId"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="paths"/> contains fewer than 2 paths.
    /// </exception>
    public static ForkPointDefinition Create(
        string fromStepId,
        IReadOnlyList<ForkPathDefinition> paths,
        string joinStepId)
    {
        ArgumentNullException.ThrowIfNull(fromStepId, nameof(fromStepId));
        ArgumentNullException.ThrowIfNull(paths, nameof(paths));
        ArgumentNullException.ThrowIfNull(joinStepId, nameof(joinStepId));

        if (paths.Count < 2)
        {
            throw new ArgumentException("Fork must have at least two paths.", nameof(paths));
        }

        return new ForkPointDefinition
        {
            ForkPointId = Guid.NewGuid().ToString("N"),
            FromStepId = fromStepId,
            Paths = paths,
            JoinStepId = joinStepId,
        };
    }
}
