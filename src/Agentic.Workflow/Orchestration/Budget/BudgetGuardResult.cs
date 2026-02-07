// =============================================================================
// <copyright file="BudgetGuardResult.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Orchestration.Budget;

/// <summary>
/// Represents the result of a budget guard check.
/// </summary>
/// <remarks>
/// <para>
/// Three possible outcomes:
/// <list type="bullet">
///   <item><description>Success: Proceed without concern</description></item>
///   <item><description>Warning: Proceed but be aware of resource constraints</description></item>
///   <item><description>Blocked: Cannot proceed, must terminate gracefully</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record BudgetGuardResult
{
    /// <summary>
    /// Gets a value indicating whether execution can continue.
    /// </summary>
    public required bool CanContinue { get; init; }

    /// <summary>
    /// Gets a value indicating whether a warning is present.
    /// </summary>
    /// <remarks>
    /// A warning indicates that execution can proceed but resources are constrained.
    /// The orchestrator may want to prioritize critical tasks.
    /// </remarks>
    public bool HasWarning { get; init; }

    /// <summary>
    /// Gets the reason for blocking or warning, if applicable.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Creates a success result allowing execution to proceed.
    /// </summary>
    /// <returns>A result indicating execution can proceed without concerns.</returns>
    public static BudgetGuardResult Success()
    {
        return new BudgetGuardResult
        {
            CanContinue = true,
            HasWarning = false,
            Reason = null
        };
    }

    /// <summary>
    /// Creates a warning result allowing execution with a caution.
    /// </summary>
    /// <param name="reason">The warning message describing the concern.</param>
    /// <returns>A result indicating execution can proceed with caution.</returns>
    public static BudgetGuardResult Warning(string reason)
    {
        ArgumentNullException.ThrowIfNull(reason, nameof(reason));

        return new BudgetGuardResult
        {
            CanContinue = true,
            HasWarning = true,
            Reason = reason
        };
    }

    /// <summary>
    /// Creates a blocked result preventing execution.
    /// </summary>
    /// <param name="reason">The reason execution cannot proceed.</param>
    /// <returns>A result indicating execution must stop.</returns>
    public static BudgetGuardResult Blocked(string reason)
    {
        ArgumentNullException.ThrowIfNull(reason, nameof(reason));

        return new BudgetGuardResult
        {
            CanContinue = false,
            HasWarning = false,
            Reason = reason
        };
    }
}