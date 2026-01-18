// =============================================================================
// <copyright file="UnpublishContentTests.cs" company="Levelup Software">
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
/// Unit tests for <see cref="UnpublishContent"/> compensation step.
/// </summary>
[Property("Category", "Unit")]
public class UnpublishContentTests
{
    private readonly IPublishingService _mockPublishingService = Substitute.For<IPublishingService>();
    private readonly TimeProvider _mockTimeProvider = Substitute.For<TimeProvider>();

    /// <summary>
    /// Verifies that UnpublishContent implements IWorkflowStep interface.
    /// </summary>
    [Test]
    public async Task UnpublishContent_ImplementsIWorkflowStep()
    {
        // Arrange & Act
        var step = new UnpublishContent(_mockPublishingService, _mockTimeProvider);

        // Assert
        await Assert.That(step).IsAssignableTo<IWorkflowStep<ContentState>>();
    }

    /// <summary>
    /// Verifies that ExecuteAsync calls unpublish when content is published.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_WhenPublished_CallsUnpublish()
    {
        // Arrange
        var step = new UnpublishContent(_mockPublishingService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var publishedUrl = "https://example.com/articles/test-article";
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            PublishedUrl = publishedUrl,
            PublishedAt = timestamp.AddMinutes(-10),
        };
        var context = StepContext.Create(state.WorkflowId, nameof(UnpublishContent), "UnpublishContent");

        _mockPublishingService.UnpublishAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await _mockPublishingService.Received(1).UnpublishAsync(publishedUrl, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that ExecuteAsync clears published state.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_Success_ClearsPublishedState()
    {
        // Arrange
        var step = new UnpublishContent(_mockPublishingService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            PublishedUrl = "https://example.com/article",
            PublishedAt = timestamp.AddMinutes(-10),
        };
        var context = StepContext.Create(state.WorkflowId, nameof(UnpublishContent), "UnpublishContent");

        _mockPublishingService.UnpublishAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.PublishedUrl).IsNull();
        await Assert.That(result.UpdatedState.PublishedAt).IsNull();
    }

    /// <summary>
    /// Verifies that ExecuteAsync adds audit entry.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_Success_AddsAuditEntry()
    {
        // Arrange
        var step = new UnpublishContent(_mockPublishingService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            PublishedUrl = "https://example.com/article",
            PublishedAt = timestamp.AddMinutes(-10),
        };
        var context = StepContext.Create(state.WorkflowId, nameof(UnpublishContent), "UnpublishContent");

        _mockPublishingService.UnpublishAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.AuditEntries).HasCount().EqualTo(1);
        await Assert.That(result.UpdatedState.AuditEntries[0].Action).IsEqualTo("Content Unpublished (Compensation)");
        await Assert.That(result.UpdatedState.AuditEntries[0].Actor).IsEqualTo("System");
    }

    /// <summary>
    /// Verifies that ExecuteAsync skips unpublishing if not published.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_NotPublished_SkipsUnpublishing()
    {
        // Arrange
        var step = new UnpublishContent(_mockPublishingService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            PublishedUrl = null,
            PublishedAt = null,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(UnpublishContent), "UnpublishContent");

        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await _mockPublishingService.DidNotReceive().UnpublishAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await Assert.That(result.UpdatedState.AuditEntries).HasCount().EqualTo(0);
    }

    /// <summary>
    /// Verifies that ExecuteAsync preserves state when UnpublishAsync returns false.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_UnpublishFails_PreservesPublishedState()
    {
        // Arrange
        var step = new UnpublishContent(_mockPublishingService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var publishedUrl = "https://example.com/articles/test-article";
        var publishedAt = timestamp.AddMinutes(-10);
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            PublishedUrl = publishedUrl,
            PublishedAt = publishedAt,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(UnpublishContent), "UnpublishContent");

        _mockPublishingService.UnpublishAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.PublishedUrl).IsEqualTo(publishedUrl);
        await Assert.That(result.UpdatedState.PublishedAt).IsEqualTo(publishedAt);
    }

    /// <summary>
    /// Verifies that ExecuteAsync adds failure audit entry when UnpublishAsync returns false.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_UnpublishFails_AddsFailureAuditEntry()
    {
        // Arrange
        var step = new UnpublishContent(_mockPublishingService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var publishedUrl = "https://example.com/articles/test-article";
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
            PublishedUrl = publishedUrl,
            PublishedAt = timestamp.AddMinutes(-10),
        };
        var context = StepContext.Create(state.WorkflowId, nameof(UnpublishContent), "UnpublishContent");

        _mockPublishingService.UnpublishAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.AuditEntries).HasCount().EqualTo(1);
        await Assert.That(result.UpdatedState.AuditEntries[0].Action).IsEqualTo("Content Unpublish Failed (Compensation)");
        await Assert.That(result.UpdatedState.AuditEntries[0].Actor).IsEqualTo("System");
        await Assert.That(result.UpdatedState.AuditEntries[0].Details).Contains(publishedUrl);
    }
}
