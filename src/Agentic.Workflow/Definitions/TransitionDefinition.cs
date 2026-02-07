// =============================================================================
// <copyright file="TransitionDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Immutable definition of a transition between workflow steps.
/// </summary>
/// <remarks>
/// <para>
/// Transition definitions capture edges in the workflow graph for:
/// <list type="bullet">
///   <item><description>Linear step progression (automatic transitions)</description></item>
///   <item><description>Conditional branching (predicate-based routing)</description></item>
///   <item><description>Source generation of transition tables</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record TransitionDefinition
{
    /// <summary>
    /// Gets the unique identifier for this transition.
    /// </summary>
    public required string TransitionId { get; init; }

    /// <summary>
    /// Gets the step ID this transition originates from.
    /// </summary>
    public required string FromStepId { get; init; }

    /// <summary>
    /// Gets the step ID this transition targets.
    /// </summary>
    public required string ToStepId { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is the default (fallthrough) transition.
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// Creates a new transition between two steps.
    /// </summary>
    /// <param name="fromStepId">The source step ID.</param>
    /// <param name="toStepId">The target step ID.</param>
    /// <param name="isDefault">Whether this is a default transition.</param>
    /// <returns>A new transition definition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fromStepId"/> or <paramref name="toStepId"/> is null.</exception>
    public static TransitionDefinition Create(string fromStepId, string toStepId, bool isDefault = true)
    {
        ArgumentNullException.ThrowIfNull(fromStepId, nameof(fromStepId));
        ArgumentNullException.ThrowIfNull(toStepId, nameof(toStepId));

        return new TransitionDefinition
        {
            TransitionId = Guid.NewGuid().ToString("N"),
            FromStepId = fromStepId,
            ToStepId = toStepId,
            IsDefault = isDefault,
        };
    }
}