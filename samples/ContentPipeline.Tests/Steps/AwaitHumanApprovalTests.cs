// =============================================================================
// <copyright file="AwaitHumanApprovalTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Steps;
using ContentPipeline.Services;
using ContentPipeline.State;
using ContentPipeline.Steps;
using NSubstitute;

namespace ContentPipeline.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="AwaitHumanApproval"/> step.
/// </summary>
[Property("Category", "Unit")]
public class AwaitHumanApprovalTests
{
    private readonly IApprovalService _mockApprovalService = Substitute.For<IApprovalService>();
    private readonly TimeProvider _mockTimeProvider = Substitute.For<TimeProvider>();

    /// <summary>
    /// Verifies that AwaitHumanApproval implements IWorkflowStep interface.
    /// </summary>
    [Test]
    public async Task AwaitHumanApproval_ImplementsIWorkflowStep()
    {
        // Arrange & Act
        var step = new AwaitHumanApproval(_mockApprovalService, _mockTimeProvider);

        // Assert
        await Assert.That(step).IsAssignableTo<IWorkflowStep<ContentState>>();
    }

    /// <summary>
    /// Verifies that ExecuteAsync calls approval service.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_CallsApprovalService()
    {
        // Arrange
        var step = new AwaitHumanApproval(_mockApprovalService, _mockTimeProvider);
        var workflowId = Guid.NewGuid();
        var state = new ContentState
        {
            WorkflowId = workflowId,
            Title = "Test Article",
            Draft = "Content to approve",
            AiQualityScore = 0.85m,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(AwaitHumanApproval), "AwaitHumanApproval");
        var timestamp = DateTimeOffset.UtcNow;
        var decision = new ApprovalDecision(
            Approved: true,
            Feedback: null,
            ReviewerId: "editor-1",
            DecisionTime: timestamp);

        _mockApprovalService.GetApprovalAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(decision);
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await _mockApprovalService.Received(1).GetApprovalAsync(workflowId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that ExecuteAsync updates state with approval decision.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_Approved_UpdatesStateWithDecision()
    {
        // Arrange
        var step = new AwaitHumanApproval(_mockApprovalService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            Draft = "Content to approve",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(AwaitHumanApproval), "AwaitHumanApproval");
        var decision = new ApprovalDecision(
            Approved: true,
            Feedback: "Looks great!",
            ReviewerId: "editor-123",
            DecisionTime: timestamp);

        _mockApprovalService.GetApprovalAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(decision);
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.HumanDecision).IsNotNull();
        await Assert.That(result.UpdatedState.HumanDecision!.Approved).IsTrue();
        await Assert.That(result.UpdatedState.HumanDecision!.ReviewerId).IsEqualTo("editor-123");
        await Assert.That(result.UpdatedState.HumanDecision!.Feedback).IsEqualTo("Looks great!");
    }

    /// <summary>
    /// Verifies that ExecuteAsync adds audit entry for approval.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_Approved_AddsAuditEntry()
    {
        // Arrange
        var step = new AwaitHumanApproval(_mockApprovalService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            Draft = "Content",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(AwaitHumanApproval), "AwaitHumanApproval");
        var decision = new ApprovalDecision(
            Approved: true,
            Feedback: null,
            ReviewerId: "editor-1",
            DecisionTime: timestamp);

        _mockApprovalService.GetApprovalAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(decision);
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.AuditEntries).HasCount().EqualTo(1);
        await Assert.That(result.UpdatedState.AuditEntries[0].Action).IsEqualTo("Human Approval Received");
        await Assert.That(result.UpdatedState.AuditEntries[0].Actor).IsEqualTo("editor-1");
    }

    /// <summary>
    /// Verifies that ExecuteAsync handles rejection.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_Rejected_SetsDecisionApprovedFalse()
    {
        // Arrange
        var step = new AwaitHumanApproval(_mockApprovalService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            Draft = "Content",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(AwaitHumanApproval), "AwaitHumanApproval");
        var decision = new ApprovalDecision(
            Approved: false,
            Feedback: "Needs more work",
            ReviewerId: "editor-1",
            DecisionTime: timestamp);

        _mockApprovalService.GetApprovalAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(decision);
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.HumanDecision!.Approved).IsFalse();
        await Assert.That(result.UpdatedState.HumanDecision!.Feedback).IsEqualTo("Needs more work");
    }
}
