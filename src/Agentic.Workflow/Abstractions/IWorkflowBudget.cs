// =============================================================================
// <copyright file="IWorkflowBudget.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Orchestration.Budget;

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Contract for aggregate budget tracking across multiple resource types.
/// </summary>
/// <remarks>
/// <para>
/// A workflow budget tracks consumption of multiple resource types. Each resource
/// has an allocated limit and consumed amount, enabling scarcity-aware action
/// scoring and early termination when resources are exhausted.
/// </para>
/// <para>
/// The budget algebra ensures that workflows gracefully degrade as resources
/// become scarce, rather than failing abruptly.
/// </para>
/// </remarks>
public interface IWorkflowBudget
{
    /// <summary>
    /// Gets the unique identifier for this budget.
    /// </summary>
    string BudgetId { get; }

    /// <summary>
    /// Gets the workflow ID this budget is associated with.
    /// </summary>
    string WorkflowId { get; }

    /// <summary>
    /// Gets the collection of resource budgets tracked by this workflow budget.
    /// </summary>
    IReadOnlyDictionary<ResourceType, IResourceBudget> Resources { get; }

    /// <summary>
    /// Gets the overall scarcity level across all resources.
    /// </summary>
    /// <remarks>
    /// Returns the maximum scarcity level from any individual resource.
    /// </remarks>
    ScarcityLevel OverallScarcity { get; }

    /// <summary>
    /// Gets the scarcity multiplier for action scoring.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The multiplier increases as resources become scarce:
    /// <list type="bullet">
    ///   <item><description>Abundant/Normal: 1.0</description></item>
    ///   <item><description>Scarce: 1.5</description></item>
    ///   <item><description>Critical: 2.0</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    double ScarcityMultiplier { get; }

    /// <summary>
    /// Checks if there is sufficient budget for a specific resource type.
    /// </summary>
    /// <param name="resourceType">The type of resource to check.</param>
    /// <param name="amount">The amount required.</param>
    /// <returns>True if sufficient budget remains; otherwise, false.</returns>
    bool HasSufficientBudget(ResourceType resourceType, double amount);

    /// <summary>
    /// Creates a new budget with updated resource consumption.
    /// </summary>
    /// <param name="resourceType">The type of resource consumed.</param>
    /// <param name="amount">The amount consumed.</param>
    /// <returns>A new budget with updated consumption.</returns>
    IWorkflowBudget WithConsumption(ResourceType resourceType, double amount);

    /// <summary>
    /// Creates a new budget with an additional or updated resource.
    /// </summary>
    /// <param name="resourceType">The type of resource.</param>
    /// <param name="resource">The resource budget to add or update.</param>
    /// <returns>A new budget with the resource added or updated.</returns>
    IWorkflowBudget WithResource(ResourceType resourceType, IResourceBudget resource);
}