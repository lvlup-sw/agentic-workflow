// =============================================================================
// <copyright file="GenerateResponseTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Steps;
using MultiModelRouter.Services;
using MultiModelRouter.State;
using MultiModelRouter.Steps;
using NSubstitute;

namespace MultiModelRouter.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="GenerateResponse"/> step.
/// </summary>
[Property("Category", "Unit")]
public class GenerateResponseTests
{
    /// <summary>
    /// Verifies that the step generates a response using the selected model.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_GeneratesResponseUsingSelectedModel()
    {
        // Arrange
        var modelProvider = Substitute.For<IModelProvider>();
        modelProvider.GenerateAsync("gpt-4", "What is AI?", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ModelResponse("AI is artificial intelligence.", 0.95m)));

        var step = new GenerateResponse(modelProvider);
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "What is AI?",
            Category = QueryCategory.Factual,
            SelectedModel = "gpt-4",
            Confidence = 0.9m,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(GenerateResponse), "Generate");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.Response).IsEqualTo("AI is artificial intelligence.");
    }

    /// <summary>
    /// Verifies that the step updates confidence from model response.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_UpdatesConfidenceFromModelResponse()
    {
        // Arrange
        var modelProvider = Substitute.For<IModelProvider>();
        modelProvider.GenerateAsync("claude-3", "Write a poem", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ModelResponse("Roses are red...", 0.88m)));

        var step = new GenerateResponse(modelProvider);
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "Write a poem",
            Category = QueryCategory.Creative,
            SelectedModel = "claude-3",
            Confidence = 0.8m,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(GenerateResponse), "Generate");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.Confidence).IsEqualTo(0.88m);
    }

    /// <summary>
    /// Verifies that the step preserves original state properties.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_PreservesOriginalState()
    {
        // Arrange
        var modelProvider = Substitute.For<IModelProvider>();
        modelProvider.GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ModelResponse("Test response", 0.9m)));

        var step = new GenerateResponse(modelProvider);
        var workflowId = Guid.NewGuid();
        var state = new RouterState
        {
            WorkflowId = workflowId,
            UserQuery = "Test query",
            Category = QueryCategory.Technical,
            SelectedModel = "gpt-4",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(GenerateResponse), "Generate");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.WorkflowId).IsEqualTo(workflowId);
        await Assert.That(result.UpdatedState.UserQuery).IsEqualTo("Test query");
        await Assert.That(result.UpdatedState.Category).IsEqualTo(QueryCategory.Technical);
        await Assert.That(result.UpdatedState.SelectedModel).IsEqualTo("gpt-4");
    }

    /// <summary>
    /// Verifies that the step calls the model provider with correct arguments.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_CallsModelProviderWithCorrectArguments()
    {
        // Arrange
        var modelProvider = Substitute.For<IModelProvider>();
        modelProvider.GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ModelResponse("Response", 0.9m)));

        var step = new GenerateResponse(modelProvider);
        var state = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "What is the meaning of life?",
            SelectedModel = "local-model",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(GenerateResponse), "Generate");

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await modelProvider.Received(1).GenerateAsync(
            "local-model",
            "What is the meaning of life?",
            Arg.Any<CancellationToken>());
    }
}
