// =============================================================================
// <copyright file="BudgetGuardAllocationTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Infrastructure.Budget;
using Agentic.Workflow.Orchestration.Budget;

namespace Agentic.Workflow.Infrastructure.Tests.Budget;

/// <summary>
/// Unit tests for <see cref="BudgetGuard"/> allocation optimizations.
/// </summary>
/// <remarks>
/// <para>
/// These tests verify that <see cref="BudgetGuard"/> minimizes heap allocations
/// when checking resource availability. The optimization uses stackalloc for
/// small fixed-size result sets to avoid List&lt;T&gt; allocations on the hot path.
/// </para>
/// </remarks>
[Property("Category", "Unit")]
public sealed class BudgetGuardAllocationTests
{
    // =============================================================================
    // A. Stackalloc Optimization Tests
    // =============================================================================

    /// <summary>
    /// Verifies that CanAffordReservation succeeds when all resources are sufficient.
    /// </summary>
    /// <remarks>
    /// When all resources are sufficient, CanAffordReservation should return Success.
    /// This test validates the happy-path result.
    /// </remarks>
    [Test]
    public async Task CanAffordReservation_AllSufficient_ReturnsSuccess()
    {
        // Arrange
        var budget = CreateSufficientBudget();
        var reservation = CreateSmallReservation();
        var guard = new BudgetGuard();

        // Act
        var result = guard.CanAffordReservation(budget, reservation);

        // Assert - Success means no insufficient resources were found
        await Assert.That(result.CanContinue).IsTrue();
        await Assert.That(result.Reason).IsNull();
    }

    /// <summary>
    /// Verifies that when resources are insufficient, the result correctly identifies them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Even with stackalloc optimization, insufficient resources must be correctly
    /// identified and reported in the blocked result.
    /// </para>
    /// </remarks>
    [Test]
    public async Task CanAffordReservation_SomeInsufficient_ReportsInsufficientResources()
    {
        // Arrange
        var budget = CreateInsufficientBudget();
        var reservation = CreateLargeReservation();
        var guard = new BudgetGuard();

        // Act
        var result = guard.CanAffordReservation(budget, reservation);

        // Assert - Should be blocked with reason mentioning insufficient resources
        await Assert.That(result.CanContinue).IsFalse();
        await Assert.That(result.Reason).IsNotNull();
        await Assert.That(result.Reason!).Contains("Insufficient resources");
    }

    /// <summary>
    /// Verifies that the guard can handle all five resource types being insufficient.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The stackalloc buffer must be sized to handle the maximum case where
    /// all five resource types (Steps, Tokens, Executions, ToolCalls, WallTime)
    /// are insufficient.
    /// </para>
    /// </remarks>
    [Test]
    public async Task CanAffordReservation_AllInsufficient_ReportsAllResourceTypes()
    {
        // Arrange - Create budget with minimal resources
        var budget = CreateMinimalBudget();
        var reservation = CreateMaxReservation();
        var guard = new BudgetGuard();

        // Act
        var result = guard.CanAffordReservation(budget, reservation);

        // Assert - Should report multiple insufficient resources
        await Assert.That(result.CanContinue).IsFalse();
        await Assert.That(result.Reason).IsNotNull();
        await Assert.That(result.Reason!).Contains("Steps");
        await Assert.That(result.Reason!).Contains("Tokens");
    }

    /// <summary>
    /// Verifies that null budget always allows the reservation.
    /// </summary>
    [Test]
    public async Task CanAffordReservation_NullBudget_ReturnsSuccess()
    {
        // Arrange
        var reservation = CreateSmallReservation();
        var guard = new BudgetGuard();

        // Act
        var result = guard.CanAffordReservation(null, reservation);

        // Assert
        await Assert.That(result.CanContinue).IsTrue();
    }

    /// <summary>
    /// Verifies that checking a single insufficient resource type works correctly.
    /// </summary>
    [Test]
    public async Task CanAffordReservation_SingleInsufficient_ReportsSingleResource()
    {
        // Arrange - Budget with only Steps being insufficient
        var stepsBudget = Substitute.For<IResourceBudget>();
        stepsBudget.HasSufficient(Arg.Any<double>()).Returns(false);
        stepsBudget.Scarcity.Returns(ScarcityLevel.Normal);

        var tokensBudget = Substitute.For<IResourceBudget>();
        tokensBudget.HasSufficient(Arg.Any<double>()).Returns(true);
        tokensBudget.Scarcity.Returns(ScarcityLevel.Normal);

        var budget = new WorkflowBudget
        {
            BudgetId = "test-budget",
            WorkflowId = "test-workflow",
            Resources = new Dictionary<ResourceType, IResourceBudget>
            {
                [ResourceType.Steps] = stepsBudget,
                [ResourceType.Tokens] = tokensBudget
            }
        };

        var reservation = CreateSmallReservation();
        var guard = new BudgetGuard();

        // Act
        var result = guard.CanAffordReservation(budget, reservation);

        // Assert - Should report only Steps as insufficient
        await Assert.That(result.CanContinue).IsFalse();
        await Assert.That(result.Reason).IsNotNull();
        await Assert.That(result.Reason!).Contains("Steps");
        await Assert.That(result.Reason!).DoesNotContain("Tokens");
    }

    // =============================================================================
    // Helper Methods
    // =============================================================================

    /// <summary>
    /// Creates a budget where all resources are sufficient for any reservation.
    /// </summary>
    private static WorkflowBudget CreateSufficientBudget()
    {
        return WorkflowBudget.Create(
            workflowId: "sufficient-workflow",
            steps: 1000,
            tokens: 100000,
            executions: 500,
            toolCalls: 1000,
            wallTimeSeconds: 3600);
    }

    /// <summary>
    /// Creates a budget where some resources are insufficient.
    /// </summary>
    private static IWorkflowBudget CreateInsufficientBudget()
    {
        // Create budget and consume most resources
        var budget = WorkflowBudget.Create(
            workflowId: "insufficient-workflow",
            steps: 10,
            tokens: 1000,
            executions: 5,
            toolCalls: 10,
            wallTimeSeconds: 60);

        // Consume most steps to make them insufficient
        return budget.WithConsumption(ResourceType.Steps, 9);
    }

    /// <summary>
    /// Creates a minimal budget where most resources are near exhaustion.
    /// </summary>
    private static IWorkflowBudget CreateMinimalBudget()
    {
        return WorkflowBudget.Create(
            workflowId: "minimal-workflow",
            steps: 1,
            tokens: 10,
            executions: 1,
            toolCalls: 1,
            wallTimeSeconds: 1);
    }

    /// <summary>
    /// Creates a small reservation that should fit within sufficient budgets.
    /// </summary>
    private static IBudgetReservation CreateSmallReservation()
    {
        return new TestReservation(Steps: 1, Tokens: 100, Executions: 1, ToolCalls: 1, WallTime: TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Creates a large reservation that may exceed limited budgets.
    /// </summary>
    private static IBudgetReservation CreateLargeReservation()
    {
        return new TestReservation(Steps: 5, Tokens: 500, Executions: 3, ToolCalls: 5, WallTime: TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Creates a maximum reservation that exceeds minimal budgets.
    /// </summary>
    private static IBudgetReservation CreateMaxReservation()
    {
        return new TestReservation(Steps: 100, Tokens: 10000, Executions: 100, ToolCalls: 100, WallTime: TimeSpan.FromMinutes(10));
    }

    /// <summary>
    /// Test implementation of budget reservation for testing purposes.
    /// </summary>
    private sealed record TestReservation(
        int Steps,
        int Tokens,
        int Executions,
        int ToolCalls,
        TimeSpan WallTime) : IBudgetReservation;
}
