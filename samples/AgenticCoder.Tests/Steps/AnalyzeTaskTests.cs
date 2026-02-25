// =============================================================================
// <copyright file="AnalyzeTaskTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Steps;
using AgenticCoder.Services;
using AgenticCoder.State;
using AgenticCoder.Steps;
using NSubstitute;

namespace AgenticCoder.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="AnalyzeTask"/> step.
/// </summary>
[Property("Category", "Unit")]
public class AnalyzeTaskTests
{
    /// <summary>
    /// Verifies that AnalyzeTask validates a valid task.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_ValidTask_ReturnsSuccessfulResult()
    {
        // Arrange
        var analyzer = Substitute.For<ITaskAnalyzer>();
        analyzer.AnalyzeTaskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new TaskAnalysisResult(true, "Low", ["Requirement 1"]));

        var step = new AnalyzeTask(analyzer);
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Implement FizzBuzz",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(AnalyzeTask), "Analyzing");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.UpdatedState).IsNotNull();
    }

    /// <summary>
    /// Verifies that AnalyzeTask calls the analyzer with correct task description.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_CallsAnalyzer_WithTaskDescription()
    {
        // Arrange
        var analyzer = Substitute.For<ITaskAnalyzer>();
        analyzer.AnalyzeTaskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new TaskAnalysisResult(true, "Medium", []));

        var step = new AnalyzeTask(analyzer);
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Implement a calculator",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(AnalyzeTask), "Analyzing");

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await analyzer.Received(1).AnalyzeTaskAsync("Implement a calculator", Arg.Any<CancellationToken>());
    }
}
