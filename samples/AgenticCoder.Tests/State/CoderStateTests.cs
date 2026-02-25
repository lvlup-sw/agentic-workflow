// =============================================================================
// <copyright file="CoderStateTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using AgenticCoder.State;

namespace AgenticCoder.Tests.State;

/// <summary>
/// Unit tests for <see cref="CoderState"/>.
/// </summary>
[Property("Category", "Unit")]
public class CoderStateTests
{
    /// <summary>
    /// Verifies that CoderState implements IWorkflowState.
    /// </summary>
    [Test]
    public async Task CoderState_Implements_IWorkflowState()
    {
        // Arrange
        var workflowId = Guid.NewGuid();

        // Act
        var state = new CoderState
        {
            WorkflowId = workflowId,
            TaskDescription = "Implement FizzBuzz",
        };

        // Assert
        await Assert.That(state).IsAssignableTo<IWorkflowState>();
        await Assert.That(state.WorkflowId).IsEqualTo(workflowId);
    }

    /// <summary>
    /// Verifies that CoderState has required properties.
    /// </summary>
    [Test]
    public async Task CoderState_HasRequiredProperties()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var plan = "Step 1: Write function. Step 2: Add tests.";

        // Act
        var state = new CoderState
        {
            WorkflowId = workflowId,
            TaskDescription = "Implement FizzBuzz",
            Plan = plan,
        };

        // Assert
        await Assert.That(state.TaskDescription).IsEqualTo("Implement FizzBuzz");
        await Assert.That(state.Plan).IsEqualTo(plan);
        await Assert.That(state.Attempts).IsEmpty();
        await Assert.That(state.LatestTestResults).IsNull();
        await Assert.That(state.AttemptCount).IsEqualTo(0);
        await Assert.That(state.HumanApproved).IsFalse();
    }

    /// <summary>
    /// Verifies that Attempts collection can be appended.
    /// </summary>
    [Test]
    public async Task CoderState_Attempts_CanBeAppended()
    {
        // Arrange
        var attempt = new CodeAttempt(
            "public int Add(int a, int b) => a + b;",
            "Simple implementation for addition",
            DateTimeOffset.UtcNow);

        // Act
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Implement Add function",
            Attempts = [attempt],
        };

        // Assert
        await Assert.That(state.Attempts).HasCount().EqualTo(1);
        await Assert.That(state.Attempts[0].Code).IsEqualTo("public int Add(int a, int b) => a + b;");
    }

    /// <summary>
    /// Verifies that TestResults can be set.
    /// </summary>
    [Test]
    public async Task CoderState_TestResults_CanBeSet()
    {
        // Arrange
        var testResults = new TestResults(false, ["Test_Add_ReturnsSum: Expected 5, got 4"]);

        // Act
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Implement Add function",
            LatestTestResults = testResults,
        };

        // Assert
        await Assert.That(state.LatestTestResults).IsNotNull();
        await Assert.That(state.LatestTestResults!.Passed).IsFalse();
        await Assert.That(state.LatestTestResults.Failures).HasCount().EqualTo(1);
    }

    /// <summary>
    /// Verifies that CoderState is immutable.
    /// </summary>
    [Test]
    public async Task CoderState_IsImmutable_WithExpression()
    {
        // Arrange
        var originalState = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Original task",
            AttemptCount = 0,
        };

        // Act
        var updatedState = originalState with { AttemptCount = 1 };

        // Assert
        await Assert.That(originalState.AttemptCount).IsEqualTo(0);
        await Assert.That(updatedState.AttemptCount).IsEqualTo(1);
        await Assert.That(originalState.TaskDescription).IsEqualTo(updatedState.TaskDescription);
    }
}
