// =============================================================================
// <copyright file="RecordFeedbackTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Primitives;
using Agentic.Workflow.Selection;
using Agentic.Workflow.Steps;
using MultiModelRouter.State;
using MultiModelRouter.Steps;
using NSubstitute;

namespace MultiModelRouter.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="RecordFeedback"/> step.
/// </summary>
[Property("Category", "Unit")]
public class RecordFeedbackTests
{
    /// <summary>
    /// Verifies that positive feedback records success outcome.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_PositiveFeedback_RecordsSuccessOutcome()
    {
        // Arrange
        var agentSelector = Substitute.For<IAgentSelector>();
        agentSelector.RecordOutcomeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AgentOutcome>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Unit>.Success(Unit.Value)));

        var step = new RecordFeedback(agentSelector);
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "What is AI?",
            Category = QueryCategory.Factual,
            SelectedModel = "gpt-4",
            Response = "AI is artificial intelligence.",
            Confidence = 0.9m,
            Feedback = new UserFeedback(5, "Great answer!", DateTimeOffset.UtcNow),
        };
        var context = StepContext.Create(state.WorkflowId, nameof(RecordFeedback), "Record");

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await agentSelector.Received(1).RecordOutcomeAsync(
            "gpt-4",
            Arg.Any<string>(),
            Arg.Is<AgentOutcome>(o => o.Success),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that negative feedback records failure outcome.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_NegativeFeedback_RecordsFailureOutcome()
    {
        // Arrange
        var agentSelector = Substitute.For<IAgentSelector>();
        agentSelector.RecordOutcomeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AgentOutcome>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Unit>.Success(Unit.Value)));

        var step = new RecordFeedback(agentSelector);
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "Explain quantum physics",
            Category = QueryCategory.Technical,
            SelectedModel = "local-model",
            Response = "Quantum is... uhh...",
            Confidence = 0.3m,
            Feedback = new UserFeedback(1, "Terrible answer!", DateTimeOffset.UtcNow),
        };
        var context = StepContext.Create(state.WorkflowId, nameof(RecordFeedback), "Record");

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await agentSelector.Received(1).RecordOutcomeAsync(
            "local-model",
            Arg.Any<string>(),
            Arg.Is<AgentOutcome>(o => !o.Success),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that neutral feedback (rating 3) records success outcome.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_NeutralFeedback_RecordsSuccessOutcome()
    {
        // Arrange
        var agentSelector = Substitute.For<IAgentSelector>();
        agentSelector.RecordOutcomeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AgentOutcome>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Unit>.Success(Unit.Value)));

        var step = new RecordFeedback(agentSelector);
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "Hello",
            Category = QueryCategory.Conversational,
            SelectedModel = "claude-3",
            Response = "Hello! How can I help?",
            Confidence = 0.7m,
            Feedback = new UserFeedback(3, null, DateTimeOffset.UtcNow),
        };
        var context = StepContext.Create(state.WorkflowId, nameof(RecordFeedback), "Record");

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert - rating 3 or above is considered success
        await agentSelector.Received(1).RecordOutcomeAsync(
            "claude-3",
            Arg.Any<string>(),
            Arg.Is<AgentOutcome>(o => o.Success),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that no feedback still returns state.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_NoFeedback_ReturnsStateUnchanged()
    {
        // Arrange
        var agentSelector = Substitute.For<IAgentSelector>();

        var step = new RecordFeedback(agentSelector);
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "Test",
            SelectedModel = "gpt-4",
            Response = "Response",
            Feedback = null, // No feedback
        };
        var context = StepContext.Create(state.WorkflowId, nameof(RecordFeedback), "Record");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert - no recording should happen
        await agentSelector.DidNotReceive().RecordOutcomeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AgentOutcome>(),
            Arg.Any<CancellationToken>());
        await Assert.That(result.UpdatedState).IsEqualTo(state);
    }

    /// <summary>
    /// Verifies that the step preserves all state properties.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_PreservesOriginalState()
    {
        // Arrange
        var agentSelector = Substitute.For<IAgentSelector>();
        agentSelector.RecordOutcomeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AgentOutcome>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Unit>.Success(Unit.Value)));

        var step = new RecordFeedback(agentSelector);
        var workflowId = Guid.NewGuid();
        var feedback = new UserFeedback(4, "Good", DateTimeOffset.UtcNow);
        var state = new RouterState
        {
            WorkflowId = workflowId,
            UserQuery = "Test query",
            Category = QueryCategory.Factual,
            SelectedModel = "gpt-4",
            Response = "Test response",
            Confidence = 0.85m,
            Feedback = feedback,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(RecordFeedback), "Record");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.WorkflowId).IsEqualTo(workflowId);
        await Assert.That(result.UpdatedState.UserQuery).IsEqualTo("Test query");
        await Assert.That(result.UpdatedState.Category).IsEqualTo(QueryCategory.Factual);
        await Assert.That(result.UpdatedState.SelectedModel).IsEqualTo("gpt-4");
        await Assert.That(result.UpdatedState.Response).IsEqualTo("Test response");
        await Assert.That(result.UpdatedState.Confidence).IsEqualTo(0.85m);
        await Assert.That(result.UpdatedState.Feedback).IsEqualTo(feedback);
    }
}
