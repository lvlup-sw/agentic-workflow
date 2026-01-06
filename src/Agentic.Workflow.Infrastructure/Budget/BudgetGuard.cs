// =============================================================================
// <copyright file="BudgetGuard.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Orchestration.Budget;

namespace Agentic.Workflow.Infrastructure.Budget;

/// <summary>
/// Guards workflow execution against resource exhaustion and critical scarcity.
/// </summary>
/// <remarks>
/// <para>
/// The BudgetGuard implements the early termination policy from the budget algebra.
/// It checks resource availability before each delegation and recommends graceful
/// termination when resources become critically scarce.
/// </para>
/// <para>
/// This prevents workflows from partially completing tasks when insufficient
/// resources remain, ensuring predictable behavior under resource constraints.
/// </para>
/// </remarks>
public sealed class BudgetGuard : IBudgetGuard
{
    /// <inheritdoc />
    public BudgetGuardResult CanProceed(IWorkflowBudget? budget)
    {
        // No budget tracking - always allow
        if (budget is null)
        {
            return BudgetGuardResult.Success();
        }

        // Empty resources - always allow (no constraints)
        if (budget.Resources.Count == 0)
        {
            return BudgetGuardResult.Success();
        }

        return budget.OverallScarcity switch
        {
            ScarcityLevel.Abundant or ScarcityLevel.Normal => BudgetGuardResult.Success(),
            ScarcityLevel.Scarce => BudgetGuardResult.Warning(
                "Resources are running low. Consider prioritizing remaining work."),
            ScarcityLevel.Critical => BudgetGuardResult.Blocked(
                "Critical resource scarcity detected. Workflow should terminate gracefully."),
            _ => BudgetGuardResult.Success()
        };
    }

    /// <inheritdoc />
    public BudgetGuardResult CanAffordReservation(IWorkflowBudget? budget, IBudgetReservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation, nameof(reservation));

        // No budget tracking - always allow
        if (budget is null)
        {
            return BudgetGuardResult.Success();
        }

        // Check each resource type against the reservation
        var insufficientResources = GetInsufficientResources(budget, reservation);

        if (insufficientResources.Count == 0)
        {
            return BudgetGuardResult.Success();
        }

        // Build reason string listing insufficient resources
        var resourceNames = string.Join(", ", insufficientResources.Select(r => r.ToString()));
        return BudgetGuardResult.Blocked($"Insufficient resources: {resourceNames}");
    }

    /// <summary>
    /// Gets the list of resources that cannot afford the reservation.
    /// </summary>
    private static List<ResourceType> GetInsufficientResources(
        IWorkflowBudget budget,
        IBudgetReservation reservation)
    {
        var insufficient = new List<ResourceType>();

        // Check Steps
        if (reservation.Steps > 0 && !budget.HasSufficientBudget(ResourceType.Steps, reservation.Steps))
        {
            insufficient.Add(ResourceType.Steps);
        }

        // Check Tokens
        if (reservation.Tokens > 0 && !budget.HasSufficientBudget(ResourceType.Tokens, reservation.Tokens))
        {
            insufficient.Add(ResourceType.Tokens);
        }

        // Check Executions
        if (reservation.Executions > 0 && !budget.HasSufficientBudget(ResourceType.Executions, reservation.Executions))
        {
            insufficient.Add(ResourceType.Executions);
        }

        // Check ToolCalls
        if (reservation.ToolCalls > 0 && !budget.HasSufficientBudget(ResourceType.ToolCalls, reservation.ToolCalls))
        {
            insufficient.Add(ResourceType.ToolCalls);
        }

        // Check WallTime
        if (reservation.WallTime > TimeSpan.Zero &&
            !budget.HasSufficientBudget(ResourceType.WallTime, reservation.WallTime.TotalSeconds))
        {
            insufficient.Add(ResourceType.WallTime);
        }

        return insufficient;
    }
}
