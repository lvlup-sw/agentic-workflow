// =============================================================================
// <copyright file="AiReviewContentTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Steps;
using ContentPipeline.Services;
using ContentPipeline.State;
using ContentPipeline.Steps;
using NSubstitute;

namespace ContentPipeline.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="AiReviewContent"/> step.
/// </summary>
[Property("Category", "Unit")]
public class AiReviewContentTests
{
    private readonly ILlmService _mockLlmService = Substitute.For<ILlmService>();
    private readonly TimeProvider _mockTimeProvider = Substitute.For<TimeProvider>();

    /// <summary>
    /// Verifies that AiReviewContent implements IWorkflowStep interface.
    /// </summary>
    [Test]
    public async Task AiReviewContent_ImplementsIWorkflowStep()
    {
        // Arrange & Act
        var step = new AiReviewContent(_mockLlmService, _mockTimeProvider);

        // Assert
        await Assert.That(step).IsAssignableTo<IWorkflowStep<ContentState>>();
    }

    /// <summary>
    /// Verifies that ExecuteAsync calls LLM service to review content.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_WithDraft_CallsLlmService()
    {
        // Arrange
        var step = new AiReviewContent(_mockLlmService, _mockTimeProvider);
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            Draft = "This is the draft content to review.",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(AiReviewContent), "AiReviewContent");

        _mockLlmService.ReviewContentAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(("Good content", 0.85m));
        _mockTimeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await _mockLlmService.Received(1).ReviewContentAsync(
            Arg.Is<string>(s => s == state.Draft),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that ExecuteAsync updates state with feedback and score.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_Success_UpdatesStateWithFeedbackAndScore()
    {
        // Arrange
        var step = new AiReviewContent(_mockLlmService, _mockTimeProvider);
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            Draft = "This is the draft content.",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(AiReviewContent), "AiReviewContent");
        var expectedFeedback = "Excellent article with comprehensive coverage.";
        var expectedScore = 0.92m;

        _mockLlmService.ReviewContentAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((expectedFeedback, expectedScore));
        _mockTimeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.AiReviewFeedback).IsEqualTo(expectedFeedback);
        await Assert.That(result.UpdatedState.AiQualityScore).IsEqualTo(expectedScore);
    }

    /// <summary>
    /// Verifies that ExecuteAsync adds audit entry.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_Success_AddsAuditEntry()
    {
        // Arrange
        var step = new AiReviewContent(_mockLlmService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            Draft = "Content to review",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(AiReviewContent), "AiReviewContent");

        _mockLlmService.ReviewContentAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(("Feedback", 0.85m));
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.AuditEntries).HasCount().EqualTo(1);
        await Assert.That(result.UpdatedState.AuditEntries[0].Action).IsEqualTo("AI Review Completed");
        await Assert.That(result.UpdatedState.AuditEntries[0].Actor).IsEqualTo("AI");
    }
}
