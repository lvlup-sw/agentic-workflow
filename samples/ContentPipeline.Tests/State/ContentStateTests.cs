// =============================================================================
// <copyright file="ContentStateTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using ContentPipeline.State;

namespace ContentPipeline.Tests.State;

/// <summary>
/// Unit tests for <see cref="ContentState"/> record.
/// </summary>
[Property("Category", "Unit")]
public class ContentStateTests
{
    /// <summary>
    /// Verifies that ContentState implements IWorkflowState interface.
    /// </summary>
    [Test]
    public async Task ContentState_ImplementsIWorkflowState()
    {
        // Arrange
        var workflowId = Guid.NewGuid();

        // Act
        var state = new ContentState { WorkflowId = workflowId };

        // Assert
        await Assert.That(state).IsAssignableTo<IWorkflowState>();
        await Assert.That(state.WorkflowId).IsEqualTo(workflowId);
    }

    /// <summary>
    /// Verifies that ContentState has all required properties with default values.
    /// </summary>
    [Test]
    public async Task ContentState_HasRequiredProperties_WithDefaults()
    {
        // Arrange & Act
        var state = new ContentState { WorkflowId = Guid.NewGuid() };

        // Assert
        await Assert.That(state.Title).IsEqualTo(string.Empty);
        await Assert.That(state.Draft).IsEqualTo(string.Empty);
        await Assert.That(state.AiReviewFeedback).IsNull();
        await Assert.That(state.AiQualityScore).IsEqualTo(0m);
        await Assert.That(state.HumanDecision).IsNull();
        await Assert.That(state.PublishedAt).IsNull();
        await Assert.That(state.PublishedUrl).IsNull();
    }

    /// <summary>
    /// Verifies that ApprovalDecision record is properly initialized.
    /// </summary>
    [Test]
    public async Task ApprovalDecision_Initializes_Correctly()
    {
        // Arrange
        var decisionTime = DateTimeOffset.UtcNow;

        // Act
        var decision = new ApprovalDecision(
            Approved: true,
            Feedback: "Looks good!",
            ReviewerId: "editor-1",
            DecisionTime: decisionTime);

        // Assert
        await Assert.That(decision.Approved).IsTrue();
        await Assert.That(decision.Feedback).IsEqualTo("Looks good!");
        await Assert.That(decision.ReviewerId).IsEqualTo("editor-1");
        await Assert.That(decision.DecisionTime).IsEqualTo(decisionTime);
    }

    /// <summary>
    /// Verifies that ContentState can be updated immutably via with expression.
    /// </summary>
    [Test]
    public async Task ContentState_WithExpression_CreatesNewInstance()
    {
        // Arrange
        var originalState = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Original Title",
            Draft = "Original draft content.",
        };

        // Act
        var updatedState = originalState with { Title = "Updated Title" };

        // Assert
        await Assert.That(updatedState.Title).IsEqualTo("Updated Title");
        await Assert.That(updatedState.Draft).IsEqualTo("Original draft content.");
        await Assert.That(originalState.Title).IsEqualTo("Original Title");
    }

    /// <summary>
    /// Verifies that ContentState can store approval decision.
    /// </summary>
    [Test]
    public async Task ContentState_WithApprovalDecision_StoresCorrectly()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var decisionTime = DateTimeOffset.UtcNow;
        var decision = new ApprovalDecision(
            Approved: true,
            Feedback: "Approved for publication",
            ReviewerId: "editor-123",
            DecisionTime: decisionTime);

        // Act
        var state = new ContentState
        {
            WorkflowId = workflowId,
            Title = "Test Article",
            HumanDecision = decision,
        };

        // Assert
        await Assert.That(state.HumanDecision).IsNotNull();
        await Assert.That(state.HumanDecision!.Approved).IsTrue();
        await Assert.That(state.HumanDecision!.ReviewerId).IsEqualTo("editor-123");
    }

    /// <summary>
    /// Verifies that ContentState tracks audit trail via AuditEntries.
    /// </summary>
    [Test]
    public async Task ContentState_AuditEntries_TracksHistory()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var entry1 = new AuditEntry(
            Timestamp: DateTimeOffset.UtcNow.AddMinutes(-10),
            Action: "Draft Generated",
            Actor: "AI",
            Details: "Generated initial draft");
        var entry2 = new AuditEntry(
            Timestamp: DateTimeOffset.UtcNow,
            Action: "Review Completed",
            Actor: "editor-1",
            Details: "Approved with minor edits");

        // Act
        var state = new ContentState
        {
            WorkflowId = workflowId,
            AuditEntries = [entry1, entry2],
        };

        // Assert
        await Assert.That(state.AuditEntries).HasCount().EqualTo(2);
        await Assert.That(state.AuditEntries[0].Action).IsEqualTo("Draft Generated");
        await Assert.That(state.AuditEntries[1].Actor).IsEqualTo("editor-1");
    }
}
