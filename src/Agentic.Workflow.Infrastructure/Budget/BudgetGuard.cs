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
    /// <summary>
    /// Maximum number of resource types that can be insufficient.
    /// </summary>
    /// <remarks>
    /// This matches the number of resource types in <see cref="ResourceType"/>:
    /// Steps, Tokens, Executions, ToolCalls, and WallTime.
    /// </remarks>
    private const int MaxResourceTypes = 5;

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

        // Use stackalloc to avoid heap allocation for the common case (all sufficient)
        Span<ResourceType> insufficientBuffer = stackalloc ResourceType[MaxResourceTypes];
        var insufficientCount = GetInsufficientResources(budget, reservation, insufficientBuffer);

        if (insufficientCount == 0)
        {
            return BudgetGuardResult.Success();
        }

        // Only allocate when we need to build the error message
        var insufficientResources = insufficientBuffer[..insufficientCount];
        var resourceNames = BuildResourceNamesList(insufficientResources);
        return BudgetGuardResult.Blocked($"Insufficient resources: {resourceNames}");
    }

    /// <summary>
    /// Gets the count of resources that cannot afford the reservation, writing them to the buffer.
    /// </summary>
    /// <param name="budget">The workflow budget to check against.</param>
    /// <param name="reservation">The reservation to validate.</param>
    /// <param name="buffer">The stack-allocated buffer to write insufficient resource types to.</param>
    /// <returns>The number of insufficient resources written to the buffer.</returns>
    private static int GetInsufficientResources(
        IWorkflowBudget budget,
        IBudgetReservation reservation,
        Span<ResourceType> buffer)
    {
        var count = 0;

        // Check Steps
        if (reservation.Steps > 0 && !budget.HasSufficientBudget(ResourceType.Steps, reservation.Steps))
        {
            buffer[count++] = ResourceType.Steps;
        }

        // Check Tokens
        if (reservation.Tokens > 0 && !budget.HasSufficientBudget(ResourceType.Tokens, reservation.Tokens))
        {
            buffer[count++] = ResourceType.Tokens;
        }

        // Check Executions
        if (reservation.Executions > 0 && !budget.HasSufficientBudget(ResourceType.Executions, reservation.Executions))
        {
            buffer[count++] = ResourceType.Executions;
        }

        // Check ToolCalls
        if (reservation.ToolCalls > 0 && !budget.HasSufficientBudget(ResourceType.ToolCalls, reservation.ToolCalls))
        {
            buffer[count++] = ResourceType.ToolCalls;
        }

        // Check WallTime
        if (reservation.WallTime > TimeSpan.Zero &&
            !budget.HasSufficientBudget(ResourceType.WallTime, reservation.WallTime.TotalSeconds))
        {
            buffer[count++] = ResourceType.WallTime;
        }

        return count;
    }

    /// <summary>
    /// Builds a comma-separated list of resource type names from a span.
    /// </summary>
    /// <param name="resources">The span of insufficient resource types.</param>
    /// <returns>A comma-separated string of resource type names.</returns>
    private static string BuildResourceNamesList(ReadOnlySpan<ResourceType> resources)
    {
        if (resources.Length == 0)
        {
            return string.Empty;
        }

        if (resources.Length == 1)
        {
            return resources[0].ToString();
        }

        // For multiple resources, use a StringBuilder to avoid multiple string allocations
        var builder = new System.Text.StringBuilder();
        builder.Append(resources[0].ToString());

        for (var i = 1; i < resources.Length; i++)
        {
            builder.Append(", ");
            builder.Append(resources[i].ToString());
        }

        return builder.ToString();
    }
}