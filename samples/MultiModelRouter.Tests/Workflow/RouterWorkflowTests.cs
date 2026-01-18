// =============================================================================
// <copyright file="RouterWorkflowTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using MultiModelRouter.State;
using MultiModelRouter.Steps;

namespace MultiModelRouter.Tests.Workflow;

/// <summary>
/// Unit tests for <see cref="RouterWorkflow"/>.
/// </summary>
[Property("Category", "Unit")]
public class RouterWorkflowTests
{
    /// <summary>
    /// Verifies that RouterWorkflow.Create returns a valid definition.
    /// </summary>
    [Test]
    public async Task Create_ReturnsValidDefinition()
    {
        // Act
        var workflow = RouterWorkflow.Create();

        // Assert
        await Assert.That(workflow).IsNotNull();
        await Assert.That(workflow.Name).IsEqualTo("multi-model-router");
    }

    /// <summary>
    /// Verifies that RouterWorkflow has the correct entry step.
    /// </summary>
    [Test]
    public async Task Create_HasCorrectEntryStep()
    {
        // Act
        var workflow = RouterWorkflow.Create();

        // Assert
        await Assert.That(workflow.EntryStep).IsNotNull();
        await Assert.That(workflow.EntryStep!.StepType).IsEqualTo(typeof(ClassifyQuery));
    }

    /// <summary>
    /// Verifies that RouterWorkflow has the correct terminal step.
    /// </summary>
    [Test]
    public async Task Create_HasCorrectTerminalStep()
    {
        // Act
        var workflow = RouterWorkflow.Create();

        // Assert
        await Assert.That(workflow.TerminalStep).IsNotNull();
        await Assert.That(workflow.TerminalStep!.StepType).IsEqualTo(typeof(RecordFeedback));
    }

    /// <summary>
    /// Verifies that RouterWorkflow has four steps.
    /// </summary>
    [Test]
    public async Task Create_HasFourSteps()
    {
        // Act
        var workflow = RouterWorkflow.Create();

        // Assert - ClassifyQuery, SelectModel, GenerateResponse, RecordFeedback
        await Assert.That(workflow.Steps).HasCount().EqualTo(4);
    }

    /// <summary>
    /// Verifies that RouterWorkflow has correct step sequence.
    /// </summary>
    [Test]
    public async Task Create_HasCorrectStepSequence()
    {
        // Act
        var workflow = RouterWorkflow.Create();

        // Assert
        var stepTypes = workflow.Steps.Select(s => s.StepType).ToList();
        await Assert.That(stepTypes).Contains(typeof(ClassifyQuery));
        await Assert.That(stepTypes).Contains(typeof(SelectModel));
        await Assert.That(stepTypes).Contains(typeof(GenerateResponse));
        await Assert.That(stepTypes).Contains(typeof(RecordFeedback));
    }
}
