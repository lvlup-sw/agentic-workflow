// =============================================================================
// <copyright file="ApprovalEscalationDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Immutable definition of an approval timeout escalation handler.
/// </summary>
/// <remarks>
/// <para>
/// Escalation handlers define what happens when an approval times out:
/// <list type="bullet">
///   <item><description>Steps: Additional workflow steps to execute</description></item>
///   <item><description>NestedApprovals: Chained approval requests (e.g., escalate to supervisor)</description></item>
///   <item><description>IsTerminal: Whether the escalation terminates the workflow</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record ApprovalEscalationDefinition
{
    /// <summary>
    /// Gets the unique identifier for this escalation handler.
    /// </summary>
    public required string EscalationId { get; init; }

    /// <summary>
    /// Gets the steps in the escalation path.
    /// </summary>
    public IReadOnlyList<StepDefinition> Steps { get; init; } = [];

    /// <summary>
    /// Gets the nested approval definitions in the escalation path.
    /// </summary>
    /// <remarks>
    /// Enables chained approvals like: timeout → notify → escalate to supervisor.
    /// </remarks>
    public IReadOnlyList<ApprovalDefinition> NestedApprovals { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether this escalation terminates the workflow.
    /// </summary>
    /// <remarks>
    /// When true, the workflow fails on timeout. When false, the escalation path
    /// rejoins the main workflow after handling.
    /// </remarks>
    public bool IsTerminal { get; init; }

    /// <summary>
    /// Creates a new escalation definition.
    /// </summary>
    /// <param name="steps">The steps in the escalation path.</param>
    /// <param name="nestedApprovals">The nested approval definitions.</param>
    /// <param name="isTerminal">Whether this escalation terminates the workflow.</param>
    /// <returns>A new escalation definition.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="steps"/> or <paramref name="nestedApprovals"/> is null.
    /// </exception>
    public static ApprovalEscalationDefinition Create(
        IReadOnlyList<StepDefinition> steps,
        IReadOnlyList<ApprovalDefinition> nestedApprovals,
        bool isTerminal)
    {
        ArgumentNullException.ThrowIfNull(steps, nameof(steps));
        ArgumentNullException.ThrowIfNull(nestedApprovals, nameof(nestedApprovals));

        return new ApprovalEscalationDefinition
        {
            EscalationId = Guid.NewGuid().ToString("N"),
            Steps = steps,
            NestedApprovals = nestedApprovals,
            IsTerminal = isTerminal,
        };
    }
}