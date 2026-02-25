// =============================================================================
// <copyright file="CoderWorkflowTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Definitions;
using AgenticCoder.State;

namespace AgenticCoder.Tests.Workflow;

/// <summary>
/// Unit tests for the CoderWorkflow definition.
/// </summary>
[Property("Category", "Unit")]
public class CoderWorkflowTests
{
    /// <summary>
    /// Verifies that Create returns a valid workflow definition.
    /// </summary>
    [Test]
    public async Task Create_ReturnsValidDefinition()
    {
        // Arrange & Act
        var workflow = CoderWorkflow.Create();

        // Assert
        await Assert.That(workflow).IsNotNull();
        await Assert.That(workflow).IsTypeOf<WorkflowDefinition<CoderState>>();
        await Assert.That(workflow.Name).IsEqualTo("agentic-coder");
    }

    /// <summary>
    /// Verifies that the workflow has an entry step.
    /// </summary>
    [Test]
    public async Task Create_HasEntryStep()
    {
        // Arrange & Act
        var workflow = CoderWorkflow.Create();

        // Assert
        await Assert.That(workflow.EntryStep).IsNotNull();
    }

    /// <summary>
    /// Verifies that the workflow has a terminal step.
    /// </summary>
    [Test]
    public async Task Create_HasTerminalStep()
    {
        // Arrange & Act
        var workflow = CoderWorkflow.Create();

        // Assert
        await Assert.That(workflow.TerminalStep).IsNotNull();
        await Assert.That(workflow.TerminalStep!.IsTerminal).IsTrue();
    }

    /// <summary>
    /// Verifies that the workflow has a refinement loop.
    /// </summary>
    [Test]
    public async Task Create_HasRefinementLoop()
    {
        // Arrange & Act
        var workflow = CoderWorkflow.Create();

        // Assert
        await Assert.That(workflow.Loops).IsNotEmpty();
        await Assert.That(workflow.Loops[0].LoopName).IsEqualTo("Refinement");
    }

    /// <summary>
    /// Verifies that the workflow loop has max iterations set to 3.
    /// </summary>
    [Test]
    public async Task Create_RefinementLoop_HasMaxIterations()
    {
        // Arrange & Act
        var workflow = CoderWorkflow.Create();

        // Assert
        await Assert.That(workflow.Loops[0].MaxIterations).IsEqualTo(3);
    }

    /// <summary>
    /// Verifies that the workflow has an approval point.
    /// </summary>
    [Test]
    public async Task Create_HasApprovalPoint()
    {
        // Arrange & Act
        var workflow = CoderWorkflow.Create();

        // Assert
        await Assert.That(workflow.ApprovalPoints).IsNotEmpty();
    }
}
