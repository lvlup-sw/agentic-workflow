// =============================================================================
// <copyright file="BudgetReservation.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;

namespace Agentic.Workflow.Infrastructure.Budget;

/// <summary>
/// Represents a multi-resource budget reservation for an executor invocation.
/// </summary>
/// <remarks>
/// <para>
/// BudgetReservation encapsulates the estimated resource consumption for a single
/// executor delegation. Resources are reserved before invocation and committed
/// or released after completion.
/// </para>
/// </remarks>
public sealed record BudgetReservation : IBudgetReservation
{
    /// <inheritdoc />
    public required int Steps { get; init; }

    /// <inheritdoc />
    public required int Tokens { get; init; }

    /// <inheritdoc />
    public required int Executions { get; init; }

    /// <inheritdoc />
    public required int ToolCalls { get; init; }

    /// <inheritdoc />
    public TimeSpan WallTime { get; init; } = TimeSpan.Zero;

    /// <summary>
    /// Gets the default reservation for a standard task.
    /// </summary>
    /// <remarks>
    /// Default values: 1 step, 2000 tokens, 1 execution, 2 tool calls.
    /// </remarks>
    public static BudgetReservation Default { get; } = new()
    {
        Steps = 1,
        Tokens = 2000,
        Executions = 1,
        ToolCalls = 2
    };

    /// <summary>
    /// Creates a reservation with the specified resource estimates.
    /// </summary>
    /// <param name="steps">Number of orchestrator steps to reserve (typically 1).</param>
    /// <param name="tokens">Estimated LLM tokens to consume.</param>
    /// <param name="executions">Number of code executions (Sandbox calls).</param>
    /// <param name="toolCalls">Number of MCP tool calls.</param>
    /// <param name="wallTime">Estimated wall time for the operation.</param>
    /// <returns>A new budget reservation with the specified estimates.</returns>
    public static BudgetReservation Create(
        int steps = 1,
        int tokens = 2000,
        int executions = 1,
        int toolCalls = 2,
        TimeSpan? wallTime = null)
    {
        return new BudgetReservation
        {
            Steps = steps,
            Tokens = tokens,
            Executions = executions,
            ToolCalls = toolCalls,
            WallTime = wallTime ?? TimeSpan.Zero
        };
    }
}