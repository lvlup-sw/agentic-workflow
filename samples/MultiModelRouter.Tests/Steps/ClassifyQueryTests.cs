// =============================================================================
// <copyright file="ClassifyQueryTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Steps;
using MultiModelRouter.State;
using MultiModelRouter.Steps;

namespace MultiModelRouter.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="ClassifyQuery"/> step.
/// </summary>
[Property("Category", "Unit")]
public class ClassifyQueryTests
{
    /// <summary>
    /// Verifies that a factual query is classified correctly.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_FactualQuery_ReturnsFactualCategory()
    {
        // Arrange
        var step = new ClassifyQuery();
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "What is the capital of France?",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(ClassifyQuery), "Classify");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.Category).IsEqualTo(QueryCategory.Factual);
    }

    /// <summary>
    /// Verifies that a creative query is classified correctly.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_CreativeQuery_ReturnsCreativeCategory()
    {
        // Arrange
        var step = new ClassifyQuery();
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "Write a poem about the ocean",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(ClassifyQuery), "Classify");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.Category).IsEqualTo(QueryCategory.Creative);
    }

    /// <summary>
    /// Verifies that a technical query is classified correctly.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_TechnicalQuery_ReturnsTechnicalCategory()
    {
        // Arrange
        var step = new ClassifyQuery();
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "Explain how to implement a binary search algorithm",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(ClassifyQuery), "Classify");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.Category).IsEqualTo(QueryCategory.Technical);
    }

    /// <summary>
    /// Verifies that a conversational query is classified correctly.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_ConversationalQuery_ReturnsConversationalCategory()
    {
        // Arrange
        var step = new ClassifyQuery();
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "How are you doing today?",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(ClassifyQuery), "Classify");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.Category).IsEqualTo(QueryCategory.Conversational);
    }

    /// <summary>
    /// Verifies that the original state is preserved after classification.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_PreservesOriginalState()
    {
        // Arrange
        var step = new ClassifyQuery();
        var workflowId = Guid.NewGuid();
        var state = new RouterState
        {
            WorkflowId = workflowId,
            UserQuery = "What is the capital of France?",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(ClassifyQuery), "Classify");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.WorkflowId).IsEqualTo(workflowId);
        await Assert.That(result.UpdatedState.UserQuery).IsEqualTo("What is the capital of France?");
    }
}
