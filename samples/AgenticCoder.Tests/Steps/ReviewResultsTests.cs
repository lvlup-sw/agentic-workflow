// =============================================================================
// <copyright file="ReviewResultsTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Steps;
using AgenticCoder.State;
using AgenticCoder.Steps;

namespace AgenticCoder.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="ReviewResults"/> step.
/// </summary>
[Property("Category", "Unit")]
public class ReviewResultsTests
{
    /// <summary>
    /// Verifies that ReviewResults passes through state when tests pass.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_TestsPassed_ReturnsState()
    {
        // Arrange
        var step = new ReviewResults();
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Test task",
            LatestTestResults = new TestResults(true, []),
        };
        var context = StepContext.Create(state.WorkflowId, nameof(ReviewResults), "Reviewing");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState).IsNotNull();
        await Assert.That(result.UpdatedState.LatestTestResults!.Passed).IsTrue();
    }

    /// <summary>
    /// Verifies that ReviewResults passes through state when tests fail.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_TestsFailed_ReturnsStateWithFailures()
    {
        // Arrange
        var step = new ReviewResults();
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Test task",
            LatestTestResults = new TestResults(false, ["Error 1"]),
        };
        var context = StepContext.Create(state.WorkflowId, nameof(ReviewResults), "Reviewing");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.LatestTestResults!.Passed).IsFalse();
    }
}
