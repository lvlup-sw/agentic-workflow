// =============================================================================
// <copyright file="ContentWorkflowTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Definitions;
using ContentPipeline.State;
using ContentPipeline.Steps;

namespace ContentPipeline.Tests;

/// <summary>
/// Unit tests for <see cref="ContentWorkflow"/> definition.
/// </summary>
[Property("Category", "Unit")]
public class ContentWorkflowTests
{
    /// <summary>
    /// Verifies that Create returns a valid workflow definition.
    /// </summary>
    [Test]
    public async Task Create_ReturnsValidDefinition()
    {
        // Act
        var workflow = ContentWorkflow.Create();

        // Assert
        await Assert.That(workflow).IsNotNull();
        await Assert.That(workflow).IsTypeOf<WorkflowDefinition<ContentState>>();
    }

    /// <summary>
    /// Verifies that the workflow has the expected name.
    /// </summary>
    [Test]
    public async Task Create_SetsWorkflowName()
    {
        // Act
        var workflow = ContentWorkflow.Create();

        // Assert
        await Assert.That(workflow.Name).IsEqualTo("content-pipeline");
    }

    /// <summary>
    /// Verifies that the workflow starts with GenerateDraft step.
    /// </summary>
    [Test]
    public async Task Create_StartsWithGenerateDraft()
    {
        // Act
        var workflow = ContentWorkflow.Create();

        // Assert
        await Assert.That(workflow.EntryStep).IsNotNull();
        await Assert.That(workflow.EntryStep!.StepType).IsEqualTo(typeof(GenerateDraft));
    }

    /// <summary>
    /// Verifies that the workflow ends with PublishContent step.
    /// </summary>
    [Test]
    public async Task Create_EndsWithPublishContent()
    {
        // Act
        var workflow = ContentWorkflow.Create();

        // Assert
        await Assert.That(workflow.TerminalStep).IsNotNull();
        await Assert.That(workflow.TerminalStep!.StepType).IsEqualTo(typeof(PublishContent));
    }

    /// <summary>
    /// Verifies that the workflow includes all expected steps.
    /// </summary>
    [Test]
    public async Task Create_IncludesAllSteps()
    {
        // Act
        var workflow = ContentWorkflow.Create();

        // Assert - 4 steps: GenerateDraft, AiReviewContent, AwaitHumanApproval, PublishContent
        await Assert.That(workflow.Steps).HasCount().EqualTo(4);

        var stepTypes = workflow.Steps.Select(s => s.StepType).ToList();
        await Assert.That(stepTypes).Contains(typeof(GenerateDraft));
        await Assert.That(stepTypes).Contains(typeof(AiReviewContent));
        await Assert.That(stepTypes).Contains(typeof(AwaitHumanApproval));
        await Assert.That(stepTypes).Contains(typeof(PublishContent));
    }

    /// <summary>
    /// Verifies that the workflow has correct transition count.
    /// </summary>
    [Test]
    public async Task Create_HasCorrectTransitionCount()
    {
        // Act
        var workflow = ContentWorkflow.Create();

        // Assert - 3 transitions: GenerateDraft->AiReviewContent, AiReviewContent->AwaitHumanApproval, AwaitHumanApproval->PublishContent
        await Assert.That(workflow.Transitions).HasCount().EqualTo(3);
    }
}
