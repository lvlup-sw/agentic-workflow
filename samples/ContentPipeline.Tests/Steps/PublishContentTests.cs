// =============================================================================
// <copyright file="PublishContentTests.cs" company="Levelup Software">
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
/// Unit tests for <see cref="PublishContent"/> step.
/// </summary>
[Property("Category", "Unit")]
public class PublishContentTests
{
    private readonly IPublishingService _mockPublishingService = Substitute.For<IPublishingService>();
    private readonly TimeProvider _mockTimeProvider = Substitute.For<TimeProvider>();

    /// <summary>
    /// Verifies that PublishContent implements IWorkflowStep interface.
    /// </summary>
    [Test]
    public async Task PublishContent_ImplementsIWorkflowStep()
    {
        // Arrange & Act
        var step = new PublishContent(_mockPublishingService, _mockTimeProvider);

        // Assert
        await Assert.That(step).IsAssignableTo<IWorkflowStep<ContentState>>();
    }

    /// <summary>
    /// Verifies that ExecuteAsync calls publishing service.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_WithApprovedContent_CallsPublishingService()
    {
        // Arrange
        var step = new PublishContent(_mockPublishingService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var decision = new ApprovalDecision(true, null, "editor-1", timestamp);
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            Draft = "Content to publish",
            HumanDecision = decision,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(PublishContent), "PublishContent");

        _mockPublishingService.PublishAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("https://example.com/articles/test-article");
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await _mockPublishingService.Received(1).PublishAsync(
            state.Title,
            state.Draft,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that ExecuteAsync updates state with published URL.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_Success_UpdatesStateWithPublishedUrl()
    {
        // Arrange
        var step = new PublishContent(_mockPublishingService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var decision = new ApprovalDecision(true, null, "editor-1", timestamp);
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            Draft = "Content to publish",
            HumanDecision = decision,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(PublishContent), "PublishContent");
        var expectedUrl = "https://example.com/articles/test-article";

        _mockPublishingService.PublishAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedUrl);
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.PublishedUrl).IsEqualTo(expectedUrl);
        await Assert.That(result.UpdatedState.PublishedAt).IsNotNull();
    }

    /// <summary>
    /// Verifies that ExecuteAsync adds audit entry.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_Success_AddsAuditEntry()
    {
        // Arrange
        var step = new PublishContent(_mockPublishingService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var decision = new ApprovalDecision(true, null, "editor-1", timestamp);
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            Draft = "Content",
            HumanDecision = decision,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(PublishContent), "PublishContent");

        _mockPublishingService.PublishAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("https://example.com/article");
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.AuditEntries).HasCount().EqualTo(1);
        await Assert.That(result.UpdatedState.AuditEntries[0].Action).IsEqualTo("Content Published");
        await Assert.That(result.UpdatedState.AuditEntries[0].Actor).IsEqualTo("System");
    }

    /// <summary>
    /// Verifies that ExecuteAsync skips publishing if not approved.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_NotApproved_SkipsPublishing()
    {
        // Arrange
        var step = new PublishContent(_mockPublishingService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var decision = new ApprovalDecision(false, "Rejected", "editor-1", timestamp);
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            Draft = "Content",
            HumanDecision = decision,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(PublishContent), "PublishContent");

        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await _mockPublishingService.DidNotReceive().PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await Assert.That(result.UpdatedState.PublishedUrl).IsNull();
    }
}
