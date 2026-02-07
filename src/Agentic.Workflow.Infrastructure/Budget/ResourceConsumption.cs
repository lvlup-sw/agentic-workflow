// =============================================================================
// <copyright file="ResourceConsumption.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Infrastructure.Budget;

/// <summary>
/// Represents resources consumed by a workflow operation.
/// </summary>
/// <remarks>
/// <para>
/// ResourceConsumption is an immutable record that tracks three types of resources:
/// <list type="bullet">
///   <item><description>Tokens: LLM tokens consumed for completions</description></item>
///   <item><description>Steps: Number of workflow steps executed</description></item>
///   <item><description>WallTime: Real-world elapsed time</description></item>
/// </list>
/// </para>
/// <para>
/// Instances are created using static factory methods and combined via the Add method.
/// The BudgetGuard uses this type to track cumulative consumption against budget limits.
/// </para>
/// </remarks>
public sealed record ResourceConsumption
{
    /// <summary>
    /// Gets the number of LLM tokens consumed.
    /// </summary>
    public int Tokens { get; init; }

    /// <summary>
    /// Gets the number of workflow steps executed.
    /// </summary>
    public int Steps { get; init; }

    /// <summary>
    /// Gets the wall-clock time elapsed.
    /// </summary>
    public TimeSpan WallTime { get; init; }

    /// <summary>
    /// Gets a ResourceConsumption with all values set to zero.
    /// </summary>
    public static ResourceConsumption None => new();

    /// <summary>
    /// Creates a ResourceConsumption with only tokens set.
    /// </summary>
    /// <param name="tokens">The number of tokens consumed.</param>
    /// <returns>A new ResourceConsumption with the specified tokens.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="tokens"/> is negative.
    /// </exception>
    public static ResourceConsumption FromTokens(int tokens)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tokens);
        return new() { Tokens = tokens };
    }

    /// <summary>
    /// Creates a ResourceConsumption representing a single step.
    /// </summary>
    /// <returns>A new ResourceConsumption with Steps set to 1.</returns>
    public static ResourceConsumption FromStep()
        => new() { Steps = 1 };

    /// <summary>
    /// Creates a ResourceConsumption with only wall time set.
    /// </summary>
    /// <param name="wallTime">The wall-clock time elapsed.</param>
    /// <returns>A new ResourceConsumption with the specified wall time.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="wallTime"/> is negative.
    /// </exception>
    public static ResourceConsumption FromWallTime(TimeSpan wallTime)
    {
        if (wallTime < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(wallTime), wallTime, "Wall time cannot be negative.");
        }

        return new() { WallTime = wallTime };
    }

    /// <summary>
    /// Combines this consumption with another, summing all resource values.
    /// </summary>
    /// <param name="other">The consumption to add.</param>
    /// <returns>A new ResourceConsumption with combined values.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="other"/> is null.
    /// </exception>
    public ResourceConsumption Add(ResourceConsumption other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new()
        {
            Tokens = Tokens + other.Tokens,
            Steps = Steps + other.Steps,
            WallTime = WallTime + other.WallTime
        };
    }
}