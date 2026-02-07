// =============================================================================
// <copyright file="IBudgetGuard.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Orchestration.Budget;

namespace Agentic.Workflow.Abstractions;

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
public interface IBudgetGuard
{
    /// <summary>
    /// Checks if the workflow can proceed with the next delegation.
    /// </summary>
    /// <param name="budget">The current workflow budget, or null if not tracking.</param>
    /// <returns>A result indicating whether to proceed and any warnings/blocks.</returns>
    /// <remarks>
    /// <para>
    /// The guard checks overall scarcity level:
    /// <list type="bullet">
    ///   <item><description>Abundant/Normal: Proceed without warning</description></item>
    ///   <item><description>Scarce: Proceed with warning about low resources</description></item>
    ///   <item><description>Critical: Block execution and recommend termination</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    BudgetGuardResult CanProceed(IWorkflowBudget? budget);

    /// <summary>
    /// Checks if the budget can afford a specific reservation.
    /// </summary>
    /// <param name="budget">The current workflow budget, or null if not tracking.</param>
    /// <param name="reservation">The reservation to check against available resources.</param>
    /// <returns>A result indicating whether the reservation is affordable.</returns>
    /// <exception cref="ArgumentNullException">Thrown when reservation is null.</exception>
    BudgetGuardResult CanAffordReservation(IWorkflowBudget? budget, IBudgetReservation reservation);
}