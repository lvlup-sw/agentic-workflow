// =============================================================================
// <copyright file="ApprovalDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Immutable definition of an approval checkpoint within a workflow.
/// </summary>
/// <remarks>
/// <para>
/// Approval definitions capture human-in-the-loop checkpoints:
/// <list type="bullet">
///   <item><description>ApprovalPointId: Unique identifier for this approval point</description></item>
///   <item><description>ApproverType: Marker type identifying the approver role</description></item>
///   <item><description>Configuration: Approval settings (type, timeout, context)</description></item>
///   <item><description>PrecedingStepId: The step that leads into this approval</description></item>
///   <item><description>EscalationHandler: What happens on timeout</description></item>
///   <item><description>RejectionHandler: What happens on rejection</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record ApprovalDefinition
{
    /// <summary>
    /// Gets the unique identifier for this approval point.
    /// </summary>
    public required string ApprovalPointId { get; init; }

    /// <summary>
    /// Gets the marker type identifying the approver role or group.
    /// </summary>
    /// <remarks>
    /// This type serves as a marker for:
    /// <list type="bullet">
    ///   <item><description>Type-safe routing of approvals</description></item>
    ///   <item><description>Dependency injection of approver-specific handlers</description></item>
    ///   <item><description>Configuration binding per approver type</description></item>
    /// </list>
    /// </remarks>
    public required Type ApproverType { get; init; }

    /// <summary>
    /// Gets the approval configuration.
    /// </summary>
    public required ApprovalConfiguration Configuration { get; init; }

    /// <summary>
    /// Gets the preceding step ID (the step before this approval point).
    /// </summary>
    public required string PrecedingStepId { get; init; }

    /// <summary>
    /// Gets the escalation handler definition (null if no escalation configured).
    /// </summary>
    public ApprovalEscalationDefinition? EscalationHandler { get; init; }

    /// <summary>
    /// Gets the rejection handler definition (null if no rejection handler configured).
    /// </summary>
    public ApprovalRejectionDefinition? RejectionHandler { get; init; }

    /// <summary>
    /// Creates a new approval definition.
    /// </summary>
    /// <param name="approverType">The marker type identifying the approver role.</param>
    /// <param name="configuration">The approval configuration.</param>
    /// <param name="precedingStepId">The step ID that leads into this approval.</param>
    /// <returns>A new approval definition.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="approverType"/>, <paramref name="configuration"/>,
    /// or <paramref name="precedingStepId"/> is null.
    /// </exception>
    public static ApprovalDefinition Create(
        Type approverType,
        ApprovalConfiguration configuration,
        string precedingStepId)
    {
        ArgumentNullException.ThrowIfNull(approverType, nameof(approverType));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        ArgumentNullException.ThrowIfNull(precedingStepId, nameof(precedingStepId));

        return new ApprovalDefinition
        {
            ApprovalPointId = Guid.NewGuid().ToString("N"),
            ApproverType = approverType,
            Configuration = configuration,
            PrecedingStepId = precedingStepId,
        };
    }

    /// <summary>
    /// Creates a new definition with escalation handler.
    /// </summary>
    /// <param name="escalation">The escalation handler definition.</param>
    /// <returns>A new approval definition with the escalation handler set.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="escalation"/> is null.
    /// </exception>
    public ApprovalDefinition WithEscalation(ApprovalEscalationDefinition escalation)
    {
        ArgumentNullException.ThrowIfNull(escalation, nameof(escalation));

        return this with { EscalationHandler = escalation };
    }

    /// <summary>
    /// Creates a new definition with rejection handler.
    /// </summary>
    /// <param name="rejection">The rejection handler definition.</param>
    /// <returns>A new approval definition with the rejection handler set.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="rejection"/> is null.
    /// </exception>
    public ApprovalDefinition WithRejection(ApprovalRejectionDefinition rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection, nameof(rejection));

        return this with { RejectionHandler = rejection };
    }
}