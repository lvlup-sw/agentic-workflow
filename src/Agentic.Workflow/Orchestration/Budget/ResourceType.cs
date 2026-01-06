// =============================================================================
// <copyright file="ResourceType.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Agentic.Workflow.Orchestration.Budget;

/// <summary>
/// Defines the types of resources tracked in workflow budgets.
/// </summary>
/// <remarks>
/// <para>
/// Workflow budgets track consumption of multiple resource types. Each resource
/// has an allocated limit and consumed amount, enabling scarcity-aware action
/// scoring and early termination when resources are exhausted.
/// </para>
/// <para>
/// The budget algebra ensures that workflows gracefully degrade as resources
/// become scarce, rather than failing abruptly.
/// </para>
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResourceType
{
    /// <summary>
    /// The number of workflow steps or iterations.
    /// </summary>
    /// <remarks>
    /// Prevents infinite loops by limiting total step count.
    /// </remarks>
    [JsonStringEnumMemberName("steps")]
    Steps,

    /// <summary>
    /// The number of LLM tokens consumed.
    /// </summary>
    /// <remarks>
    /// Tracks API costs and enables cost-aware action selection.
    /// </remarks>
    [JsonStringEnumMemberName("tokens")]
    Tokens,

    /// <summary>
    /// The number of code or task executions.
    /// </summary>
    /// <remarks>
    /// Limits sandbox invocations for security and resource management.
    /// </remarks>
    [JsonStringEnumMemberName("executions")]
    Executions,

    /// <summary>
    /// The number of tool calls made.
    /// </summary>
    /// <remarks>
    /// Tracks tool usage across all tool types.
    /// </remarks>
    [JsonStringEnumMemberName("tool_calls")]
    ToolCalls,

    /// <summary>
    /// Wall-clock time elapsed.
    /// </summary>
    /// <remarks>
    /// Enforces time limits for SLA compliance.
    /// </remarks>
    [JsonStringEnumMemberName("wall_time")]
    WallTime
}
