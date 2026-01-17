// =============================================================================
// <copyright file="WorkflowBudgetTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Orchestration.Budget;

namespace Agentic.Workflow.Infrastructure.Tests.Budget;

/// <summary>
/// Unit tests for <see cref="WorkflowBudget"/> verifying budget management
/// and scarcity calculation caching.
/// </summary>
[Property("Category", "Unit")]
public sealed class WorkflowBudgetTests
{
    // =============================================================================
    // A. OverallScarcity Caching Tests
    // =============================================================================

    /// <summary>
    /// Verifies that OverallScarcity is computed once and cached for subsequent accesses.
    /// </summary>
    /// <remarks>
    /// This test verifies the optimization where the OverallScarcity property
    /// is computed lazily and cached rather than iterating over all resources
    /// on every access.
    /// </remarks>
    [Test]
    public async Task OverallScarcity_AccessedMultipleTimes_ComputesOnce()
    {
        // Arrange
        var accessCount = 0;
        var mockBudget = Substitute.For<IResourceBudget>();
        mockBudget.Scarcity.Returns(_ =>
        {
            accessCount++;
            return ScarcityLevel.Normal;
        });

        var budget = new WorkflowBudget
        {
            BudgetId = "test-budget",
            WorkflowId = "test-workflow",
            Resources = new Dictionary<ResourceType, IResourceBudget>
            {
                [ResourceType.Steps] = mockBudget
            }
        };

        // Act - Access OverallScarcity multiple times
        var scarcity1 = budget.OverallScarcity;
        var scarcity2 = budget.OverallScarcity;
        var scarcity3 = budget.OverallScarcity;

        // Assert - All accesses should return the same value
        await Assert.That(scarcity1).IsEqualTo(ScarcityLevel.Normal);
        await Assert.That(scarcity2).IsEqualTo(ScarcityLevel.Normal);
        await Assert.That(scarcity3).IsEqualTo(ScarcityLevel.Normal);

        // Assert - Scarcity should only be computed once (mock accessed once per resource)
        await Assert.That(accessCount).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that OverallScarcity returns the maximum scarcity level from all resources.
    /// </summary>
    [Test]
    public async Task OverallScarcity_WithMultipleResources_ReturnsMaxScarcity()
    {
        // Arrange
        var abundantBudget = Substitute.For<IResourceBudget>();
        abundantBudget.Scarcity.Returns(ScarcityLevel.Abundant);

        var criticalBudget = Substitute.For<IResourceBudget>();
        criticalBudget.Scarcity.Returns(ScarcityLevel.Critical);

        var normalBudget = Substitute.For<IResourceBudget>();
        normalBudget.Scarcity.Returns(ScarcityLevel.Normal);

        var budget = new WorkflowBudget
        {
            BudgetId = "test-budget",
            WorkflowId = "test-workflow",
            Resources = new Dictionary<ResourceType, IResourceBudget>
            {
                [ResourceType.Steps] = abundantBudget,
                [ResourceType.Tokens] = criticalBudget,
                [ResourceType.Executions] = normalBudget
            }
        };

        // Act
        var scarcity = budget.OverallScarcity;

        // Assert - Should return Critical (highest ordinal)
        await Assert.That(scarcity).IsEqualTo(ScarcityLevel.Critical);
    }

    /// <summary>
    /// Verifies that OverallScarcity returns Abundant when no resources are defined.
    /// </summary>
    [Test]
    public async Task OverallScarcity_WithNoResources_ReturnsAbundant()
    {
        // Arrange
        var budget = new WorkflowBudget
        {
            BudgetId = "test-budget",
            WorkflowId = "test-workflow",
            Resources = new Dictionary<ResourceType, IResourceBudget>()
        };

        // Act
        var scarcity = budget.OverallScarcity;

        // Assert
        await Assert.That(scarcity).IsEqualTo(ScarcityLevel.Abundant);
    }

    /// <summary>
    /// Verifies that ScarcityMultiplier is correctly calculated based on cached OverallScarcity.
    /// </summary>
    [Test]
    public async Task ScarcityMultiplier_WithCachedOverallScarcity_ReturnsCorrectValue()
    {
        // Arrange
        var scarceBudget = Substitute.For<IResourceBudget>();
        scarceBudget.Scarcity.Returns(ScarcityLevel.Scarce);

        var budget = new WorkflowBudget
        {
            BudgetId = "test-budget",
            WorkflowId = "test-workflow",
            Resources = new Dictionary<ResourceType, IResourceBudget>
            {
                [ResourceType.Steps] = scarceBudget
            }
        };

        // Act
        var multiplier = budget.ScarcityMultiplier;

        // Assert - Scarce level should have multiplier of 3.0
        await Assert.That(multiplier).IsEqualTo(3.0);
    }
}
