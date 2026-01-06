// =============================================================================
// <copyright file="BranchPathDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Immutable definition of a single branch path within a workflow.
/// </summary>
/// <remarks>
/// <para>
/// A branch path represents one possible route through a conditional branch.
/// Each path contains a sequence of steps and may or may not rejoin the main flow.
/// </para>
/// </remarks>
public sealed record BranchPathDefinition
{
    /// <summary>
    /// Gets the unique identifier for this branch path.
    /// </summary>
    public required string PathId { get; init; }

    /// <summary>
    /// Gets the condition description for this path (for visualization/debugging).
    /// </summary>
    public required string ConditionDescription { get; init; }

    /// <summary>
    /// Gets the steps in this branch path.
    /// </summary>
    public IReadOnlyList<StepDefinition> Steps { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether this path terminates (does not rejoin).
    /// </summary>
    public bool IsTerminal { get; init; }

    /// <summary>
    /// Gets the optional approval definition for this path.
    /// </summary>
    /// <remarks>
    /// When set, this path includes an approval gate before its steps execute.
    /// </remarks>
    public ApprovalDefinition? Approval { get; init; }

    /// <summary>
    /// Creates a new branch path definition.
    /// </summary>
    /// <param name="conditionDescription">The condition description.</param>
    /// <param name="steps">The steps in this path.</param>
    /// <param name="isTerminal">Whether this path terminates without rejoining.</param>
    /// <param name="approval">Optional approval gate for this path.</param>
    /// <returns>A new branch path definition.</returns>
    public static BranchPathDefinition Create(
        string conditionDescription,
        IReadOnlyList<StepDefinition> steps,
        bool isTerminal = false,
        ApprovalDefinition? approval = null)
    {
        ArgumentNullException.ThrowIfNull(conditionDescription, nameof(conditionDescription));
        ArgumentNullException.ThrowIfNull(steps, nameof(steps));

        return new BranchPathDefinition
        {
            PathId = Guid.NewGuid().ToString("N"),
            ConditionDescription = conditionDescription,
            Steps = steps,
            IsTerminal = isTerminal,
            Approval = approval,
        };
    }
}
