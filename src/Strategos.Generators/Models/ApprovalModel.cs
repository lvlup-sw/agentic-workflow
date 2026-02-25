// -----------------------------------------------------------------------
// <copyright file="ApprovalModel.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Strategos.Generators.Polyfills;
using Strategos.Generators.Utilities;

namespace Strategos.Generators.Models;

/// <summary>
/// Represents an approval checkpoint in a workflow for code generation.
/// </summary>
/// <remarks>
/// This model captures:
/// <list type="bullet">
///   <item><description>ApprovalPointName: Identifier for the approval phase</description></item>
///   <item><description>ApproverTypeName: Fully qualified type name of the approver marker</description></item>
///   <item><description>PrecedingStepName: The step that leads into this approval</description></item>
///   <item><description>EscalationSteps: Optional steps to execute on timeout</description></item>
///   <item><description>RejectionSteps: Optional steps to execute on rejection</description></item>
///   <item><description>NestedEscalationApprovals: Optional chained approvals for escalation</description></item>
/// </list>
/// </remarks>
/// <param name="ApprovalPointName">The name of the approval point (e.g., "ManagerReview").</param>
/// <param name="ApproverTypeName">The fully qualified approver type name (e.g., "MyApp.Approvers.ManagerApprover").</param>
/// <param name="PrecedingStepName">The name of the step that precedes this approval.</param>
/// <param name="EscalationSteps">The optional steps in the escalation path.</param>
/// <param name="RejectionSteps">The optional steps in the rejection path.</param>
/// <param name="NestedEscalationApprovals">The optional nested approvals for chained escalation.</param>
/// <param name="IsEscalationTerminal">Whether escalation terminates the workflow.</param>
/// <param name="IsRejectionTerminal">Whether rejection terminates the workflow.</param>
internal sealed record ApprovalModel(
    string ApprovalPointName,
    string ApproverTypeName,
    string PrecedingStepName,
    IReadOnlyList<StepModel>? EscalationSteps = null,
    IReadOnlyList<StepModel>? RejectionSteps = null,
    IReadOnlyList<ApprovalModel>? NestedEscalationApprovals = null,
    bool IsEscalationTerminal = false,
    bool IsRejectionTerminal = false)
{
    /// <summary>
    /// Gets the phase name for this approval point.
    /// </summary>
    /// <remarks>
    /// Returns "AwaitApproval_{ApprovalPointName}" (e.g., "AwaitApproval_ManagerReview").
    /// </remarks>
    public string PhaseName => $"AwaitApproval_{ApprovalPointName}";

    /// <summary>
    /// Gets a value indicating whether this approval has escalation configured.
    /// </summary>
    public bool HasEscalation =>
        (EscalationSteps is not null && EscalationSteps.Count > 0)
        || (NestedEscalationApprovals is not null && NestedEscalationApprovals.Count > 0);

    /// <summary>
    /// Gets a value indicating whether this approval has rejection configured.
    /// </summary>
    public bool HasRejection => RejectionSteps is not null && RejectionSteps.Count > 0;

    /// <summary>
    /// Creates a new <see cref="ApprovalModel"/> with validation of all parameters.
    /// </summary>
    /// <param name="approvalPointName">The name of the approval point. Must be a valid C# identifier.</param>
    /// <param name="approverTypeName">The fully qualified approver type name. Cannot be null or whitespace.</param>
    /// <param name="precedingStepName">The name of the preceding step. Must be a valid C# identifier.</param>
    /// <param name="escalationSteps">The optional steps in the escalation path.</param>
    /// <param name="rejectionSteps">The optional steps in the rejection path.</param>
    /// <param name="nestedEscalationApprovals">The optional nested approvals for chained escalation.</param>
    /// <param name="isEscalationTerminal">Whether escalation terminates the workflow.</param>
    /// <param name="isRejectionTerminal">Whether rejection terminates the workflow.</param>
    /// <returns>A validated <see cref="ApprovalModel"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="approvalPointName"/>, <paramref name="approverTypeName"/>,
    /// or <paramref name="precedingStepName"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when validation fails (e.g., invalid identifier).
    /// </exception>
    public static ApprovalModel Create(
        string approvalPointName,
        string approverTypeName,
        string precedingStepName,
        IReadOnlyList<StepModel>? escalationSteps = null,
        IReadOnlyList<StepModel>? rejectionSteps = null,
        IReadOnlyList<ApprovalModel>? nestedEscalationApprovals = null,
        bool isEscalationTerminal = false,
        bool isRejectionTerminal = false)
    {
        // Validate required parameters
        ThrowHelper.ThrowIfNull(approvalPointName, nameof(approvalPointName));
        IdentifierValidator.ValidateIdentifier(approvalPointName, nameof(approvalPointName));
        ThrowHelper.ThrowIfNullOrWhiteSpace(approverTypeName, nameof(approverTypeName));
        ThrowHelper.ThrowIfNull(precedingStepName, nameof(precedingStepName));
        IdentifierValidator.ValidateIdentifier(precedingStepName, nameof(precedingStepName));

        return new ApprovalModel(
            ApprovalPointName: approvalPointName,
            ApproverTypeName: approverTypeName,
            PrecedingStepName: precedingStepName,
            EscalationSteps: escalationSteps,
            RejectionSteps: rejectionSteps,
            NestedEscalationApprovals: nestedEscalationApprovals,
            IsEscalationTerminal: isEscalationTerminal,
            IsRejectionTerminal: isRejectionTerminal);
    }
}
