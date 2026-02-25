// =============================================================================
// <copyright file="RunTestsTests.cs" company="Levelup Software">
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
/// Unit tests for <see cref="RunTests"/> step.
/// </summary>
[Property("Category", "Unit")]
public class RunTestsTests
{
    /// <summary>
    /// Verifies that RunTests executes tests and updates state.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_RunsTestsAndUpdatesState()
    {
        // Arrange
        var runner = Substitute.For<ITestRunner>();
        runner.RunTestsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new TestResults(true, []));

        var attempt = new CodeAttempt("public void Test() { }", "reasoning", DateTimeOffset.UtcNow);
        var step = new RunTests(runner);
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Test task",
            Attempts = [attempt],
        };
        var context = StepContext.Create(state.WorkflowId, nameof(RunTests), "Testing");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.LatestTestResults).IsNotNull();
        await Assert.That(result.UpdatedState.LatestTestResults!.Passed).IsTrue();
    }

    /// <summary>
    /// Verifies that RunTests captures failures.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_WithFailures_CapturesFailures()
    {
        // Arrange
        var runner = Substitute.For<ITestRunner>();
        runner.RunTestsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new TestResults(false, ["Test1 failed", "Test2 failed"]));

        var attempt = new CodeAttempt("buggy code", "reasoning", DateTimeOffset.UtcNow);
        var step = new RunTests(runner);
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Test task",
            Attempts = [attempt],
        };
        var context = StepContext.Create(state.WorkflowId, nameof(RunTests), "Testing");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.LatestTestResults!.Passed).IsFalse();
        await Assert.That(result.UpdatedState.LatestTestResults.Failures).HasCount().EqualTo(2);
    }

    /// <summary>
    /// Verifies that RunTests uses latest attempt code.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_UsesLatestAttemptCode()
    {
        // Arrange
        var runner = Substitute.For<ITestRunner>();
        runner.RunTestsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new TestResults(true, []));

        var attempt1 = new CodeAttempt("old code", "v1", DateTimeOffset.UtcNow.AddMinutes(-5));
        var attempt2 = new CodeAttempt("latest code", "v2", DateTimeOffset.UtcNow);
        var step = new RunTests(runner);
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Test task",
            Attempts = [attempt1, attempt2],
        };
        var context = StepContext.Create(state.WorkflowId, nameof(RunTests), "Testing");

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await runner.Received(1).RunTestsAsync("latest code", "Test task", Arg.Any<CancellationToken>());
    }
}
