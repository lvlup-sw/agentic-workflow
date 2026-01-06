// =============================================================================
// <copyright file="IResourceBudget.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Orchestration.Budget;

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Contract for tracking a single resource within a workflow budget.
/// </summary>
/// <remarks>
/// <para>
/// Each resource type (tokens, steps, executions, etc.) is tracked independently
/// with an allocated limit and consumed amount. Scarcity level is computed
/// based on the percentage of remaining capacity.
/// </para>
/// </remarks>
public interface IResourceBudget
{
    /// <summary>
    /// Gets the type of resource being tracked.
    /// </summary>
    ResourceType Type { get; }

    /// <summary>
    /// Gets the total allocated budget for this resource.
    /// </summary>
    double Allocated { get; }

    /// <summary>
    /// Gets the amount of this resource that has been consumed.
    /// </summary>
    double Consumed { get; }

    /// <summary>
    /// Gets the remaining budget for this resource.
    /// </summary>
    double Remaining { get; }

    /// <summary>
    /// Gets the current scarcity level for this resource.
    /// </summary>
    ScarcityLevel Scarcity { get; }

    /// <summary>
    /// Gets the percentage of budget remaining (0.0 to 1.0).
    /// </summary>
    double RemainingPercentage { get; }

    /// <summary>
    /// Creates a new budget with the specified amount consumed.
    /// </summary>
    /// <param name="amount">The amount to consume.</param>
    /// <returns>A new budget with updated consumption.</returns>
    IResourceBudget WithConsumption(double amount);

    /// <summary>
    /// Checks if there is sufficient budget for the requested amount.
    /// </summary>
    /// <param name="amount">The amount to check.</param>
    /// <returns>True if sufficient budget remains; otherwise, false.</returns>
    bool HasSufficient(double amount);
}
