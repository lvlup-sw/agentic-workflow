// =============================================================================
// <copyright file="ResourceBudget.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Orchestration.Budget;

namespace Agentic.Workflow.Infrastructure.Budget;

/// <summary>
/// Represents the budget state for a single resource type.
/// </summary>
/// <remarks>
/// <para>
/// Budget lifecycle follows the pattern: allocate → reserve → commit/release.
/// </para>
/// <para>
/// Formula: Remaining = Allocated - Consumed.
/// </para>
/// </remarks>
public sealed record ResourceBudget : IResourceBudget
{
    /// <inheritdoc />
    public required ResourceType Type { get; init; }

    /// <inheritdoc />
    public required double Allocated { get; init; }

    /// <inheritdoc />
    public double Consumed { get; init; }

    /// <inheritdoc />
    public double Remaining => Allocated - Consumed;

    /// <inheritdoc />
    public double RemainingPercentage => Allocated > 0 ? Remaining / Allocated : 0.0;

    /// <inheritdoc />
    public ScarcityLevel Scarcity => RemainingPercentage switch
    {
        > 0.7 => ScarcityLevel.Abundant,
        > 0.3 => ScarcityLevel.Normal,
        > 0.1 => ScarcityLevel.Scarce,
        _ => ScarcityLevel.Critical
    };

    /// <summary>
    /// Creates a new budget with the specified allocation.
    /// </summary>
    /// <param name="resourceType">The type of resource.</param>
    /// <param name="allocated">The total allocated budget.</param>
    /// <returns>A new resource budget with zero consumed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when allocated is negative.</exception>
    public static ResourceBudget Create(ResourceType resourceType, double allocated)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(allocated, nameof(allocated));

        return new ResourceBudget
        {
            Type = resourceType,
            Allocated = allocated,
            Consumed = 0
        };
    }

    /// <inheritdoc />
    public IResourceBudget WithConsumption(double amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount, nameof(amount));

        return this with { Consumed = Consumed + amount };
    }

    /// <inheritdoc />
    public bool HasSufficient(double amount)
    {
        return Remaining >= amount;
    }
}
