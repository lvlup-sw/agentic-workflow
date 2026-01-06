// =============================================================================
// <copyright file="IBudgetReservation.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Contract for a budget reservation that estimates resource consumption.
/// </summary>
/// <remarks>
/// <para>
/// Budget reservations are used to check if a workflow can afford an operation
/// before executing it. This prevents partial completion of expensive operations
/// when resources are insufficient.
/// </para>
/// </remarks>
public interface IBudgetReservation
{
    /// <summary>
    /// Gets the estimated number of steps this operation will consume.
    /// </summary>
    int Steps { get; }

    /// <summary>
    /// Gets the estimated number of tokens this operation will consume.
    /// </summary>
    int Tokens { get; }

    /// <summary>
    /// Gets the estimated number of code executions this operation will consume.
    /// </summary>
    int Executions { get; }

    /// <summary>
    /// Gets the estimated number of tool calls this operation will consume.
    /// </summary>
    int ToolCalls { get; }

    /// <summary>
    /// Gets the estimated wall time this operation will consume.
    /// </summary>
    TimeSpan WallTime { get; }
}
