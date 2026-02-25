// =============================================================================
// <copyright file="AwaitHumanApproval.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Steps;
using ContentPipeline.Services;
using ContentPipeline.State;

namespace ContentPipeline.Steps;

/// <summary>
/// Workflow step that waits for human approval of the content.
/// </summary>
/// <remarks>
/// This step represents a human-in-the-loop checkpoint. The workflow
/// pauses until a human reviewer approves or rejects the content.
/// The decision and reviewer identity are recorded in the audit trail.
/// </remarks>
public sealed class AwaitHumanApproval : IWorkflowStep<ContentState>
{
    private readonly IApprovalService _approvalService;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AwaitHumanApproval"/> class.
    /// </summary>
    /// <param name="approvalService">The approval service.</param>
    /// <param name="timeProvider">The time provider for timestamps.</param>
    public AwaitHumanApproval(IApprovalService approvalService, TimeProvider timeProvider)
    {
        _approvalService = approvalService ?? throw new ArgumentNullException(nameof(approvalService));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <inheritdoc/>
    public async Task<StepResult<ContentState>> ExecuteAsync(
        ContentState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        var decision = await _approvalService.GetApprovalAsync(state.WorkflowId, cancellationToken);

        var timestamp = _timeProvider.GetUtcNow();
        var auditEntry = new AuditEntry(
            Timestamp: timestamp,
            Action: "Human Approval Received",
            Actor: decision.ReviewerId,
            Details: decision.Approved
                ? "Content approved for publication"
                : $"Content rejected. Feedback: {decision.Feedback}");

        var updatedState = state with
        {
            HumanDecision = decision,
            AuditEntries = [.. state.AuditEntries, auditEntry],
        };

        return StepResult<ContentState>.FromState(updatedState);
    }
}
