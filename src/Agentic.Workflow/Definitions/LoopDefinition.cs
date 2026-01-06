// =============================================================================
// <copyright file="LoopDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Immutable definition of a loop (RepeatUntil) within a workflow.
/// </summary>
/// <remarks>
/// <para>
/// Loop definitions capture repeat-until loop constructs:
/// <list type="bullet">
///   <item><description>LoopId: Unique identifier for this loop</description></item>
///   <item><description>LoopName: Name used for phase enum prefixing</description></item>
///   <item><description>FromStepId: The step where this loop originates</description></item>
///   <item><description>MaxIterations: Safety limit to prevent infinite loops</description></item>
///   <item><description>BodySteps: The steps executed in each iteration</description></item>
///   <item><description>ContinuationStepId: The step to continue to after loop exits</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record LoopDefinition
{
    /// <summary>
    /// Gets the unique identifier for this loop.
    /// </summary>
    public required string LoopId { get; init; }

    /// <summary>
    /// Gets the loop name (used for Phase enum prefixing).
    /// </summary>
    public required string LoopName { get; init; }

    /// <summary>
    /// Gets the step ID where this loop originates from.
    /// </summary>
    public required string FromStepId { get; init; }

    /// <summary>
    /// Gets the maximum iterations allowed (prevents infinite loops).
    /// </summary>
    public required int MaxIterations { get; init; }

    /// <summary>
    /// Gets the steps in the loop body.
    /// </summary>
    public IReadOnlyList<StepDefinition> BodySteps { get; init; } = [];

    /// <summary>
    /// Gets the step ID to continue to after loop completes.
    /// </summary>
    public string? ContinuationStepId { get; init; }

    /// <summary>
    /// Creates a new loop definition.
    /// </summary>
    /// <param name="loopName">The loop name (used for phase enum prefixing).</param>
    /// <param name="fromStepId">The step ID where this loop originates.</param>
    /// <param name="maxIterations">Maximum iterations allowed.</param>
    /// <param name="bodySteps">The steps in the loop body.</param>
    /// <returns>A new loop definition.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="loopName"/>, <paramref name="fromStepId"/>, or
    /// <paramref name="bodySteps"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxIterations"/> is less than 1.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="bodySteps"/> is empty.
    /// </exception>
    public static LoopDefinition Create(
        string loopName,
        string fromStepId,
        int maxIterations,
        IReadOnlyList<StepDefinition> bodySteps)
    {
        ArgumentNullException.ThrowIfNull(loopName, nameof(loopName));
        ArgumentNullException.ThrowIfNull(fromStepId, nameof(fromStepId));
        ArgumentNullException.ThrowIfNull(bodySteps, nameof(bodySteps));
        ArgumentOutOfRangeException.ThrowIfLessThan(maxIterations, 1, nameof(maxIterations));

        if (bodySteps.Count == 0)
        {
            throw new ArgumentException("Loop body must contain at least one step.", nameof(bodySteps));
        }

        return new LoopDefinition
        {
            LoopId = Guid.NewGuid().ToString("N"),
            LoopName = loopName,
            FromStepId = fromStepId,
            MaxIterations = maxIterations,
            BodySteps = bodySteps,
        };
    }

    /// <summary>
    /// Creates a new loop definition with the specified continuation step.
    /// </summary>
    /// <param name="continuationStepId">The step ID to continue to after loop exits.</param>
    /// <returns>A new loop definition with the continuation step set.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="continuationStepId"/> is null.
    /// </exception>
    public LoopDefinition WithContinuation(string continuationStepId)
    {
        ArgumentNullException.ThrowIfNull(continuationStepId, nameof(continuationStepId));

        return this with { ContinuationStepId = continuationStepId };
    }
}
