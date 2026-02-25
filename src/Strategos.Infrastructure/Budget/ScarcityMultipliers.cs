// =============================================================================
// <copyright file="ScarcityMultipliers.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Orchestration.Budget;

namespace Strategos.Infrastructure.Budget;

/// <summary>
/// Provides scarcity multipliers for budget-aware action scoring.
/// </summary>
/// <remarks>
/// <para>
/// Multipliers are applied to action costs during specialist selection.
/// Lower multipliers (scarce/critical) make expensive actions less attractive,
/// encouraging the orchestrator to choose more economical operations when
/// resources are running low.
/// </para>
/// <para>
/// The multiplier values follow the budget algebra from the system architecture:
/// <list type="bullet">
///   <item><description>Abundant (>70% remaining): 1.0 - no penalty</description></item>
///   <item><description>Normal (30-70% remaining): 0.8 - slight penalty</description></item>
///   <item><description>Scarce (10-30% remaining): 0.5 - significant penalty</description></item>
///   <item><description>Critical (less than or equal to 10% remaining): 0.2 - severe penalty</description></item>
/// </list>
/// </para>
/// </remarks>
public static class ScarcityMultipliers
{
    /// <summary>
    /// Multiplier for abundant resources (>70% remaining). No penalty applied.
    /// </summary>
    public const decimal Abundant = 1.0m;

    /// <summary>
    /// Multiplier for normal resource levels (30-70% remaining). Slight penalty.
    /// </summary>
    public const decimal Normal = 0.8m;

    /// <summary>
    /// Multiplier for scarce resources (10-30% remaining). Significant penalty.
    /// </summary>
    public const decimal Scarce = 0.5m;

    /// <summary>
    /// Multiplier for critical resource levels (less than or equal to 10% remaining). Severe penalty.
    /// </summary>
    public const decimal Critical = 0.2m;

    /// <summary>
    /// Gets the multiplier for the specified scarcity level.
    /// </summary>
    /// <param name="level">The scarcity level to get the multiplier for.</param>
    /// <returns>The multiplier value for the specified scarcity level.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="level"/> is not a valid <see cref="ScarcityLevel"/> value.
    /// </exception>
    public static decimal For(ScarcityLevel level) => level switch
    {
        ScarcityLevel.Abundant => Abundant,
        ScarcityLevel.Normal => Normal,
        ScarcityLevel.Scarce => Scarce,
        ScarcityLevel.Critical => Critical,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Invalid scarcity level.")
    };
}
