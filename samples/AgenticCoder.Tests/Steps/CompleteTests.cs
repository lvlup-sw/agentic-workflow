// =============================================================================
// <copyright file="CompleteTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Steps;
using AgenticCoder.State;
using AgenticCoder.Steps;

namespace AgenticCoder.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="Complete"/> step.
/// </summary>
[Property("Category", "Unit")]
public class CompleteTests
{
    /// <summary>
    /// Verifies that Complete returns the final state unchanged.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_ReturnsFinalState()
    {
        // Arrange
        var step = new Complete();
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Test task",
            HumanApproved = true,
            AttemptCount = 2,
        };
        var context = StepContext.Create(state.WorkflowId, nameof(Complete), "Completing");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState).IsNotNull();
        await Assert.That(result.UpdatedState.TaskDescription).IsEqualTo("Test task");
        await Assert.That(result.UpdatedState.HumanApproved).IsTrue();
    }
}
