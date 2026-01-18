// =============================================================================
// <copyright file="GenerateCodeTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Steps;
using AgenticCoder.Services;
using AgenticCoder.State;
using AgenticCoder.Steps;
using NSubstitute;

namespace AgenticCoder.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="GenerateCode"/> step.
/// </summary>
[Property("Category", "Unit")]
public class GenerateCodeTests
{
    /// <summary>
    /// Verifies that GenerateCode creates a code attempt.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_CreatesCodeAttempt()
    {
        // Arrange
        var generator = Substitute.For<ICodeGenerator>();
        generator.GenerateCodeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(("public void Test() { }", "Simple implementation"));

        var step = new GenerateCode(generator);
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Implement test function",
            Plan = "Step 1: Create function",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(GenerateCode), "Generating");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.Attempts).HasCount().EqualTo(1);
        await Assert.That(result.UpdatedState.Attempts[0].Code).Contains("public void Test()");
    }

    /// <summary>
    /// Verifies that GenerateCode increments attempt count.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_IncrementsAttemptCount()
    {
        // Arrange
        var generator = Substitute.For<ICodeGenerator>();
        generator.GenerateCodeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(("code", "reasoning"));

        var step = new GenerateCode(generator);
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Task",
            Plan = "Plan",
            AttemptCount = 1,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(GenerateCode), "Generating");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.AttemptCount).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that GenerateCode passes feedback from failed tests.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_WithFailedTests_PassesFeedback()
    {
        // Arrange
        var generator = Substitute.For<ICodeGenerator>();
        generator.GenerateCodeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(("improved code", "fixed based on feedback"));

        var previousAttempt = new CodeAttempt("old code", "old reasoning", DateTimeOffset.UtcNow.AddMinutes(-1));
        var step = new GenerateCode(generator);
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Task",
            Plan = "Plan",
            Attempts = [previousAttempt],
            LatestTestResults = new TestResults(false, ["Test failed: expected X got Y"]),
        };
        var context = StepContext.Create(state.WorkflowId, nameof(GenerateCode), "Generating");

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert - verify generator was called with previous attempt and feedback
        await generator.Received(1).GenerateCodeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            "old code",
            Arg.Is<string>(s => s.Contains("Test failed")),
            Arg.Any<CancellationToken>());
    }
}
