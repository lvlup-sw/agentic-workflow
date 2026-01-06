// =============================================================================
// <copyright file="ScarcityLevel.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Agentic.Workflow.Orchestration.Budget;

/// <summary>
/// Indicates the scarcity level for a resource within a workflow budget.
/// </summary>
/// <remarks>
/// <para>
/// Scarcity levels drive action scoring through the budget algebra. As resources
/// become scarcer, the scarcity multiplier increases, making expensive actions
/// less attractive to the task scorer.
/// </para>
/// <para>
/// Thresholds are calculated as percentage of remaining capacity:
/// <list type="bullet">
///   <item><description>Abundant: > 70% remaining</description></item>
///   <item><description>Normal: 30% - 70% remaining</description></item>
///   <item><description>Scarce: 10% - 30% remaining</description></item>
///   <item><description>Critical: ≤ 10% remaining</description></item>
/// </list>
/// </para>
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ScarcityLevel
{
    /// <summary>
    /// Resources are plentiful (> 70% remaining).
    /// </summary>
    /// <remarks>
    /// Normal operation. Scarcity multiplier = 1.0 (no penalty).
    /// </remarks>
    [JsonStringEnumMemberName("abundant")]
    Abundant,

    /// <summary>
    /// Resources are at normal levels (30% - 70% remaining).
    /// </summary>
    /// <remarks>
    /// Normal operation. Scarcity multiplier = 1.0 (no penalty).
    /// </remarks>
    [JsonStringEnumMemberName("normal")]
    Normal,

    /// <summary>
    /// Resources are running low (10% - 30% remaining).
    /// </summary>
    /// <remarks>
    /// Prioritize high-value, low-cost actions. Scarcity multiplier = 1.5.
    /// </remarks>
    [JsonStringEnumMemberName("scarce")]
    Scarce,

    /// <summary>
    /// Resources are critically low (≤ 10% remaining).
    /// </summary>
    /// <remarks>
    /// Consider graceful termination. Scarcity multiplier = 2.0.
    /// BudgetGuard may block further execution.
    /// </remarks>
    [JsonStringEnumMemberName("critical")]
    Critical
}
