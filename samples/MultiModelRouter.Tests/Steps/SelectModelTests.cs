// =============================================================================
// <copyright file="SelectModelTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Primitives;
using Strategos.Selection;
using Strategos.Steps;
using MultiModelRouter.State;
using MultiModelRouter.Steps;
using NSubstitute;

namespace MultiModelRouter.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="SelectModel"/> step.
/// </summary>
[Property("Category", "Unit")]
public class SelectModelTests
{
    /// <summary>
    /// Verifies that the step selects a model using the agent selector.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_SelectsModelUsingAgentSelector()
    {
        // Arrange
        var selector = Substitute.For<IAgentSelector>();
        selector.SelectAgentAsync(Arg.Any<AgentSelectionContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<AgentSelection>.Success(new AgentSelection
            {
                SelectedAgentId = "gpt-4",
                TaskCategory = TaskCategory.General,
                SampledTheta = 0.85,
                SelectionConfidence = 0.9,
            })));

        var step = new SelectModel(selector);
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "What is AI?",
            Category = QueryCategory.Factual,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(SelectModel), "Select");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.SelectedModel).IsEqualTo("gpt-4");
    }

    /// <summary>
    /// Verifies that the step includes confidence from the selection.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_IncludesSelectionConfidence()
    {
        // Arrange
        var selector = Substitute.For<IAgentSelector>();
        selector.SelectAgentAsync(Arg.Any<AgentSelectionContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<AgentSelection>.Success(new AgentSelection
            {
                SelectedAgentId = "claude-3",
                TaskCategory = TaskCategory.General,
                SampledTheta = 0.75,
                SelectionConfidence = 0.85,
            })));

        var step = new SelectModel(selector);
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "Write a story",
            Category = QueryCategory.Creative,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(SelectModel), "Select");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.Confidence).IsEqualTo(0.85m);
    }

    /// <summary>
    /// Verifies that the step falls back to expensive model on low confidence.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_LowConfidence_FallsBackToExpensiveModel()
    {
        // Arrange
        var selector = Substitute.For<IAgentSelector>();
        selector.SelectAgentAsync(Arg.Any<AgentSelectionContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<AgentSelection>.Success(new AgentSelection
            {
                SelectedAgentId = "local-model",
                TaskCategory = TaskCategory.General,
                SampledTheta = 0.3,
                SelectionConfidence = 0.2, // Low confidence triggers fallback
            })));

        var step = new SelectModel(selector, confidenceThreshold: 0.5m);
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "Complex question",
            Category = QueryCategory.Technical,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(SelectModel), "Select");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert - should fall back to the expensive model (gpt-4)
        await Assert.That(result.UpdatedState.SelectedModel).IsEqualTo(SelectModel.FallbackModel);
    }

    /// <summary>
    /// Verifies that the step preserves original state properties.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_PreservesOriginalState()
    {
        // Arrange
        var selector = Substitute.For<IAgentSelector>();
        selector.SelectAgentAsync(Arg.Any<AgentSelectionContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<AgentSelection>.Success(new AgentSelection
            {
                SelectedAgentId = "gpt-4",
                TaskCategory = TaskCategory.General,
                SampledTheta = 0.9,
                SelectionConfidence = 0.95,
            })));

        var step = new SelectModel(selector);
        var workflowId = Guid.NewGuid();
        var state = new RouterState
        {
            WorkflowId = workflowId,
            UserQuery = "Test query",
            Category = QueryCategory.Technical,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(SelectModel), "Select");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.WorkflowId).IsEqualTo(workflowId);
        await Assert.That(result.UpdatedState.UserQuery).IsEqualTo("Test query");
        await Assert.That(result.UpdatedState.Category).IsEqualTo(QueryCategory.Technical);
    }
}

