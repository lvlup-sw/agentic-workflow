// =============================================================================
// <copyright file="PlanImplementationTests.cs" company="Levelup Software">
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
/// Unit tests for <see cref="PlanImplementation"/> step.
/// </summary>
[Property("Category", "Unit")]
public class PlanImplementationTests
{
    /// <summary>
    /// Verifies that PlanImplementation creates a plan.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_CreatesImplementationPlan()
    {
        // Arrange
        var planner = Substitute.For<IPlanner>();
        var analyzer = Substitute.For<ITaskAnalyzer>();

        analyzer.AnalyzeTaskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new TaskAnalysisResult(true, "Low", ["Req1", "Req2"]));

        planner.CreatePlanAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns("## Implementation Plan\n\n### Steps:\n1. Create function\n2. Add tests");

        var step = new PlanImplementation(planner, analyzer);
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Implement FizzBuzz",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(PlanImplementation), "Planning");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.Plan).IsNotNull();
        await Assert.That(result.UpdatedState.Plan).Contains("Implementation Plan");
    }

    /// <summary>
    /// Verifies that PlanImplementation updates state with plan.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_UpdatesStateWithPlan()
    {
        // Arrange
        var expectedPlan = "Test Plan Content";
        var planner = Substitute.For<IPlanner>();
        var analyzer = Substitute.For<ITaskAnalyzer>();

        analyzer.AnalyzeTaskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new TaskAnalysisResult(true, "Medium", []));

        planner.CreatePlanAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(expectedPlan);

        var step = new PlanImplementation(planner, analyzer);
        var state = new CoderState
        {
            WorkflowId = Guid.NewGuid(),
            TaskDescription = "Test task",
        };
        var context = StepContext.Create(state.WorkflowId, nameof(PlanImplementation), "Planning");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(result.UpdatedState.Plan).IsEqualTo(expectedPlan);
    }
}
