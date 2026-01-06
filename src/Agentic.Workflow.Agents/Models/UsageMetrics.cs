// =============================================================================
// <copyright file="UsageMetrics.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Agents.Models;

/// <summary>
/// Represents resource usage metrics collected during specialist task execution.
/// </summary>
/// <remarks>
/// <para>
/// UsageMetrics tracks actual resource consumption for budget enforcement.
/// These metrics are included in specialist signals to enable accurate
/// budget commit operations (estimated vs actual).
/// </para>
/// <para>
/// Metrics are immutable and support addition for aggregating usage
/// across multiple operations.
/// </para>
/// </remarks>
/// <param name="TokensConsumed">The number of LLM tokens consumed during execution.</param>
/// <param name="ExecutionsPerformed">The number of code executions performed (Sandbox calls).</param>
/// <param name="ToolCallsMade">The number of MCP tool calls made.</param>
/// <param name="Duration">The wall-clock duration of the operation.</param>
public sealed record UsageMetrics(
    long TokensConsumed,
    long ExecutionsPerformed,
    long ToolCallsMade,
    TimeSpan Duration)
{
    /// <summary>
    /// Gets a UsageMetrics instance with all values set to zero.
    /// </summary>
    /// <remarks>
    /// Used as a default when no usage metrics are available.
    /// </remarks>
    public static UsageMetrics Zero { get; } = new(0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Combines two UsageMetrics instances by summing all properties.
    /// </summary>
    /// <param name="left">The first metrics instance.</param>
    /// <param name="right">The second metrics instance.</param>
    /// <returns>A new UsageMetrics with combined values.</returns>
    public static UsageMetrics operator +(UsageMetrics left, UsageMetrics right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));

        return new UsageMetrics(
            left.TokensConsumed + right.TokensConsumed,
            left.ExecutionsPerformed + right.ExecutionsPerformed,
            left.ToolCallsMade + right.ToolCallsMade,
            left.Duration + right.Duration);
    }
}
