// =============================================================================
// <copyright file="RouterStateTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using MultiModelRouter.State;

namespace MultiModelRouter.Tests.State;

/// <summary>
/// Unit tests for <see cref="RouterState"/>.
/// </summary>
[Property("Category", "Unit")]
public class RouterStateTests
{
    /// <summary>
    /// Verifies that RouterState implements IWorkflowState interface.
    /// </summary>
    [Test]
    public async Task RouterState_Implements_IWorkflowState()
    {
        // Arrange
        var state = new RouterState { WorkflowId = Guid.NewGuid() };

        // Act
        IWorkflowState workflowState = state;

        // Assert
        await Assert.That(workflowState).IsNotNull();
        await Assert.That(workflowState.WorkflowId).IsNotEqualTo(Guid.Empty);
    }

    /// <summary>
    /// Verifies that RouterState has required properties with correct defaults.
    /// </summary>
    [Test]
    public async Task RouterState_HasDefaultValues()
    {
        // Arrange & Act
        var state = new RouterState { WorkflowId = Guid.NewGuid() };

        // Assert
        await Assert.That(state.UserQuery).IsEqualTo(string.Empty);
        await Assert.That(state.Category).IsEqualTo(QueryCategory.Factual);
        await Assert.That(state.SelectedModel).IsEqualTo(string.Empty);
        await Assert.That(state.Response).IsEqualTo(string.Empty);
        await Assert.That(state.Confidence).IsEqualTo(0m);
        await Assert.That(state.Feedback).IsNull();
    }

    /// <summary>
    /// Verifies that RouterState is immutable via with-expression.
    /// </summary>
    [Test]
    public async Task RouterState_IsImmutable_WithExpression()
    {
        // Arrange
        var original = new RouterState
        {
            WorkflowId = Guid.NewGuid(),
            UserQuery = "What is the capital of France?",
        };

        // Act
        var modified = original with { SelectedModel = "gpt-4" };

        // Assert
        await Assert.That(modified.SelectedModel).IsEqualTo("gpt-4");
        await Assert.That(original.SelectedModel).IsEqualTo(string.Empty);
        await Assert.That(modified.UserQuery).IsEqualTo("What is the capital of France?");
    }
}

