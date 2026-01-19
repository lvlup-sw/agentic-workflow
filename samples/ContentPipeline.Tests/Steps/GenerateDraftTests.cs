// =============================================================================
// <copyright file="GenerateDraftTests.cs" company="Levelup Software">
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
/// Unit tests for <see cref="GenerateDraft"/> step.
/// </summary>
[Property("Category", "Unit")]
public class GenerateDraftTests
{
    private readonly ILlmService _mockLlmService = Substitute.For<ILlmService>();
    private readonly TimeProvider _mockTimeProvider = Substitute.For<TimeProvider>();

    /// <summary>
    /// Verifies that GenerateDraft implements IWorkflowStep interface.
    /// </summary>
    [Test]
    public async Task GenerateDraft_ImplementsIWorkflowStep()
    {
        // Arrange & Act
        var step = new GenerateDraft(_mockLlmService, _mockTimeProvider);

        // Assert
        await Assert.That(step).IsAssignableTo<IWorkflowStep<ContentState>>();
    }

    /// <summary>
    /// Verifies that ExecuteAsync calls LLM service with title.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_WithTitle_CallsLlmService()
    {
        // Arrange
        var step = new GenerateDraft(_mockLlmService, _mockTimeProvider);
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(GenerateDraft), "GenerateDraft");

        _mockLlmService.GenerateDraftAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Generated content");
        _mockTimeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await _mockLlmService.Received(1).GenerateDraftAsync(
            Arg.Is<string>(s => s.Contains("Test Article")),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that ExecuteAsync updates state with generated draft.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_Success_UpdatesStateWithDraft()
    {
        // Arrange
        var step = new GenerateDraft(_mockLlmService, _mockTimeProvider);
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(GenerateDraft), "GenerateDraft");
        var expectedDraft = "This is the generated draft content.";

        _mockLlmService.GenerateDraftAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedDraft);
        _mockTimeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.Draft).IsEqualTo(expectedDraft);
    }

    /// <summary>
    /// Verifies that ExecuteAsync adds audit entry.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_Success_AddsAuditEntry()
    {
        // Arrange
        var step = new GenerateDraft(_mockLlmService, _mockTimeProvider);
        var timestamp = DateTimeOffset.UtcNow;
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(GenerateDraft), "GenerateDraft");

        _mockLlmService.GenerateDraftAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Generated content");
        _mockTimeProvider.GetUtcNow().Returns(timestamp);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.AuditEntries).HasCount().EqualTo(1);
        await Assert.That(result.UpdatedState.AuditEntries[0].Action).IsEqualTo("Draft Generated");
        await Assert.That(result.UpdatedState.AuditEntries[0].Actor).IsEqualTo("AI");
        await Assert.That(result.UpdatedState.AuditEntries[0].Timestamp).IsEqualTo(timestamp);
    }

    /// <summary>
    /// Verifies that ExecuteAsync returns StepResult with updated state.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_ReturnsStepResult()
    {
        // Arrange
        var step = new GenerateDraft(_mockLlmService, _mockTimeProvider);
        var state = new ContentState
        {
            WorkflowId = Guid.NewGuid(),
            Title = "Test Article",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(GenerateDraft), "GenerateDraft");

        _mockLlmService.GenerateDraftAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Generated content");
        _mockTimeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsTypeOf<StepResult<ContentState>>();
    }
}
