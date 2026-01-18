// =============================================================================
// <copyright file="MockApprovalService.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using ContentPipeline.State;

namespace ContentPipeline.Services;

/// <summary>
/// Mock implementation of <see cref="IApprovalService"/> for testing purposes.
/// </summary>
/// <remarks>
/// This service simulates human approval by returning a preconfigured decision.
/// It can be configured to reject content for testing rejection scenarios.
/// </remarks>
public sealed class MockApprovalService : IApprovalService
{
    private readonly bool _shouldApprove;
    private readonly string _reviewerId;
    private readonly string? _feedback;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockApprovalService"/> class.
    /// </summary>
    /// <param name="shouldApprove">Whether to approve content.</param>
    /// <param name="reviewerId">The reviewer identifier.</param>
    /// <param name="feedback">Optional feedback for rejection.</param>
    /// <param name="timeProvider">The time provider for timestamps.</param>
    public MockApprovalService(
        bool shouldApprove = true,
        string reviewerId = "mock-editor",
        string? feedback = null,
        TimeProvider? timeProvider = null)
    {
        _shouldApprove = shouldApprove;
        _reviewerId = reviewerId;
        _feedback = feedback;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc/>
    public Task<ApprovalDecision> GetApprovalAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        var decision = new ApprovalDecision(
            Approved: _shouldApprove,
            Feedback: _shouldApprove ? _feedback : _feedback ?? "Content needs revision",
            ReviewerId: _reviewerId,
            DecisionTime: _timeProvider.GetUtcNow());

        return Task.FromResult(decision);
    }
}
