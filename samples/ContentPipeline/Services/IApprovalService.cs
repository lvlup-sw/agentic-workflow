// =============================================================================
// <copyright file="IApprovalService.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using ContentPipeline.State;

namespace ContentPipeline.Services;

/// <summary>
/// Interface for human approval services in the content workflow.
/// </summary>
public interface IApprovalService
{
    /// <summary>
    /// Gets the approval decision for a workflow.
    /// </summary>
    /// <param name="workflowId">The workflow identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The approval decision.</returns>
    Task<ApprovalDecision> GetApprovalAsync(Guid workflowId, CancellationToken cancellationToken = default);
}
