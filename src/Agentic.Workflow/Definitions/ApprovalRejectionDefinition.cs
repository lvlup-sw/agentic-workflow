// =============================================================================
// <copyright file="ApprovalRejectionDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Immutable definition of an approval rejection handler.
/// </summary>
/// <remarks>
/// <para>
/// Rejection handlers define what happens when an approver rejects:
/// <list type="bullet">
///   <item><description>Steps: Workflow steps to execute on rejection (e.g., notify, cleanup)</description></item>
///   <item><description>IsTerminal: Whether rejection terminates the workflow</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record ApprovalRejectionDefinition
{
    /// <summary>
    /// Gets the unique identifier for this rejection handler.
    /// </summary>
    public required string RejectionHandlerId { get; init; }

    /// <summary>
    /// Gets the steps in the rejection handling path.
    /// </summary>
    public IReadOnlyList<StepDefinition> Steps { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether this handler terminates the workflow.
    /// </summary>
    /// <remarks>
    /// When true, the workflow fails on rejection. When false, the rejection path
    /// rejoins the main workflow after handling.
    /// </remarks>
    public bool IsTerminal { get; init; }

    /// <summary>
    /// Creates a new rejection handler definition.
    /// </summary>
    /// <param name="steps">The steps in the rejection handling path.</param>
    /// <param name="isTerminal">Whether this handler terminates the workflow.</param>
    /// <returns>A new rejection handler definition.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="steps"/> is null.
    /// </exception>
    public static ApprovalRejectionDefinition Create(
        IReadOnlyList<StepDefinition> steps,
        bool isTerminal)
    {
        ArgumentNullException.ThrowIfNull(steps, nameof(steps));

        return new ApprovalRejectionDefinition
        {
            RejectionHandlerId = Guid.NewGuid().ToString("N"),
            Steps = steps,
            IsTerminal = isTerminal,
        };
    }
}
