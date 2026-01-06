// =============================================================================
// <copyright file="IWorkflowCondition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Represents a registered condition that can be evaluated at runtime and audited.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <remarks>
/// <para>
/// Workflow conditions wrap Expression-based predicates to enable:
/// <list type="bullet">
///   <item><description>Runtime evaluation via compiled delegate for performance</description></item>
///   <item><description>Auditability via string representation of the original expression</description></item>
///   <item><description>Traceability via deterministic condition identifier</description></item>
/// </list>
/// </para>
/// <para>
/// This interface implements the Registry Pattern for lambda handling as described
/// in the workflow design documentation. The ToString() method is critical for
/// explaining agent behavior to humans (e.g., "s => s.QualityScore >= 0.9").
/// </para>
/// </remarks>
public interface IWorkflowCondition<in TState>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Gets the unique identifier for this condition.
    /// </summary>
    /// <remarks>
    /// The condition ID is deterministic and used for registry lookup.
    /// Format: "{WorkflowName}-{LoopOrBranchName}" (e.g., "ProcessClaim-Refinement").
    /// </remarks>
    string ConditionId { get; }

    /// <summary>
    /// Executes the condition against the given state.
    /// </summary>
    /// <param name="state">The workflow state to evaluate.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    /// <remarks>
    /// This method uses a pre-compiled delegate for efficient runtime evaluation.
    /// </remarks>
    bool Execute(TState state);
}
