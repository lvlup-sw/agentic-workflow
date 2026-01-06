// =============================================================================
// <copyright file="ForkPathDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Immutable definition of a single path within a fork.
/// </summary>
/// <remarks>
/// <para>
/// A fork path represents one parallel execution route within a fork construct.
/// Each path contains a sequence of steps and may optionally have a failure handler.
/// </para>
/// <para>
/// When a fork path fails:
/// <list type="bullet">
///   <item><description>If a failure handler exists, it executes and determines recovery behavior</description></item>
///   <item><description>If no failure handler exists, the failure propagates to the workflow level</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record ForkPathDefinition
{
    /// <summary>
    /// Gets the unique identifier for this fork path.
    /// </summary>
    public required string PathId { get; init; }

    /// <summary>
    /// Gets the zero-based index of this path within the fork.
    /// </summary>
    /// <remarks>
    /// Path indices are used to generate unique property names and track path order.
    /// </remarks>
    public required int PathIndex { get; init; }

    /// <summary>
    /// Gets the steps in this fork path.
    /// </summary>
    public IReadOnlyList<StepDefinition> Steps { get; init; } = [];

    /// <summary>
    /// Gets the failure handler for this path, if any.
    /// </summary>
    /// <remarks>
    /// When a step in this path fails, the failure handler executes.
    /// If the handler calls <c>Complete()</c>, the path is marked as <see cref="ForkPathStatus.Failed"/>.
    /// Otherwise, the path is marked as <see cref="ForkPathStatus.FailedWithRecovery"/>.
    /// </remarks>
    public FailureHandlerDefinition? FailureHandler { get; init; }

    /// <summary>
    /// Creates a new fork path definition.
    /// </summary>
    /// <param name="pathIndex">The zero-based index of this path.</param>
    /// <param name="steps">The steps in this path.</param>
    /// <param name="failureHandler">Optional failure handler for this path.</param>
    /// <returns>A new fork path definition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="steps"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="steps"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pathIndex"/> is negative.</exception>
    public static ForkPathDefinition Create(
        int pathIndex,
        IReadOnlyList<StepDefinition> steps,
        FailureHandlerDefinition? failureHandler = null)
    {
        ArgumentNullException.ThrowIfNull(steps, nameof(steps));
        ArgumentOutOfRangeException.ThrowIfNegative(pathIndex, nameof(pathIndex));

        if (steps.Count == 0)
        {
            throw new ArgumentException("Fork path must have at least one step.", nameof(steps));
        }

        return new ForkPathDefinition
        {
            PathId = Guid.NewGuid().ToString("N"),
            PathIndex = pathIndex,
            Steps = steps,
            FailureHandler = failureHandler,
        };
    }
}
