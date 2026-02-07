// -----------------------------------------------------------------------
// <copyright file="LoopModel.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Polyfills;
using Agentic.Workflow.Generators.Utilities;

namespace Agentic.Workflow.Generators.Models;

/// <summary>
/// Represents a loop construct within a workflow for code generation.
/// </summary>
/// <remarks>
/// <para>
/// Loop models capture the structure of RepeatUntil/While constructs in the workflow DSL.
/// The source generator uses this model to emit:
/// - Loop iteration tracking properties on saga state
/// - Loop condition evaluation handlers
/// - Loop exit/continue transition logic
/// </para>
/// <para>
/// Nested loops are supported via ParentLoopName, which enables hierarchical naming
/// for generated code (e.g., "OuterLoop_InnerLoop_StepName").
/// </para>
/// </remarks>
/// <param name="LoopName">The name of the loop (e.g., "Refinement").</param>
/// <param name="ConditionId">The unique identifier for registry lookup (e.g., "ProcessClaim-Refinement").</param>
/// <param name="MaxIterations">The maximum allowed iterations before forced exit.</param>
/// <param name="FirstBodyStepName">The prefixed name of the first step in the loop body.</param>
/// <param name="LastBodyStepName">The prefixed name of the last step in the loop body.</param>
/// <param name="ContinuationStepName">The step to execute after loop exit, or null if terminal.</param>
/// <param name="ParentLoopName">The name of the parent loop for nested loops, or null for top-level.</param>
/// <param name="BranchOnExitId">The ID of the branch that should be evaluated when the loop exits, or null if no branch follows.</param>
/// <param name="BranchOnExit">The full branch model to evaluate on loop exit, or null if no branch follows.</param>
internal sealed record LoopModel(
    string LoopName,
    string ConditionId,
    int MaxIterations,
    string FirstBodyStepName,
    string LastBodyStepName,
    string? ContinuationStepName,
    string? ParentLoopName,
    string? BranchOnExitId = null,
    BranchModel? BranchOnExit = null)
{
    /// <summary>
    /// Gets the full hierarchical prefix for steps in this loop.
    /// </summary>
    /// <remarks>
    /// For top-level loops, returns the loop name (e.g., "Refinement").
    /// For nested loops, returns the hierarchy (e.g., "Outer_Inner").
    /// </remarks>
    public string FullPrefix => ParentLoopName is null
        ? LoopName
        : $"{ParentLoopName}_{LoopName}";

    /// <summary>
    /// Gets the property name for tracking loop iterations on saga state.
    /// </summary>
    /// <remarks>
    /// Removes underscores for valid C# property names.
    /// E.g., "Outer_Inner" becomes "OuterInnerIterationCount".
    /// </remarks>
    public string IterationPropertyName => $"{FullPrefix.Replace("_", string.Empty)}IterationCount";

    /// <summary>
    /// Gets the method name for the loop exit condition check.
    /// </summary>
    /// <remarks>
    /// E.g., "Refinement" becomes "ShouldExitRefinementLoop".
    /// For nested loops, "Outer_Inner" becomes "ShouldExitOuterInnerLoop".
    /// </remarks>
    public string ConditionMethodName => $"ShouldExit{FullPrefix.Replace("_", string.Empty)}Loop";

    /// <summary>
    /// Gets whether this loop has a branch that should be evaluated on exit.
    /// </summary>
    /// <remarks>
    /// When true, the loop exit handler should route through the branch evaluation
    /// instead of directly to the continuation step.
    /// </remarks>
    public bool HasBranchOnExit => BranchOnExit is not null;

    /// <summary>
    /// Creates a new <see cref="LoopModel"/> with validation of all parameters.
    /// </summary>
    /// <param name="loopName">The name of the loop (e.g., "Refinement"). Must be a valid C# identifier.</param>
    /// <param name="conditionId">The unique identifier for registry lookup. Cannot be null or whitespace.</param>
    /// <param name="maxIterations">The maximum allowed iterations before forced exit. Must be >= 1.</param>
    /// <param name="firstBodyStepName">The prefixed name of the first step in the loop body. Cannot be null or whitespace.</param>
    /// <param name="lastBodyStepName">The prefixed name of the last step in the loop body. Cannot be null or whitespace.</param>
    /// <param name="continuationStepName">The optional step to execute after loop exit.</param>
    /// <param name="parentLoopName">The optional name of the parent loop. If provided, must be a valid C# identifier.</param>
    /// <param name="branchOnExitId">The optional ID of the branch to evaluate on loop exit (deprecated, use branchOnExit).</param>
    /// <param name="branchOnExit">The optional full branch model to evaluate on loop exit.</param>
    /// <returns>A validated <see cref="LoopModel"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxIterations"/> is less than 1.</exception>
    public static LoopModel Create(
        string loopName,
        string conditionId,
        int maxIterations,
        string firstBodyStepName,
        string lastBodyStepName,
        string? continuationStepName = null,
        string? parentLoopName = null,
        string? branchOnExitId = null,
        BranchModel? branchOnExit = null)
    {
        // Validate required parameters
        ThrowHelper.ThrowIfNull(loopName, nameof(loopName));
        IdentifierValidator.ValidateIdentifier(loopName, nameof(loopName));
        ThrowHelper.ThrowIfNullOrWhiteSpace(conditionId, nameof(conditionId));
        ThrowHelper.ThrowIfLessThan(maxIterations, 1, nameof(maxIterations));
        ThrowHelper.ThrowIfNullOrWhiteSpace(firstBodyStepName, nameof(firstBodyStepName));
        ThrowHelper.ThrowIfNullOrWhiteSpace(lastBodyStepName, nameof(lastBodyStepName));

        // Validate optional parent loop name if provided
        if (parentLoopName is not null && !IdentifierValidator.IsValidIdentifier(parentLoopName))
        {
            throw new ArgumentException(
                $"Parent loop name '{parentLoopName}' is not a valid C# identifier.",
                nameof(parentLoopName));
        }

        return new LoopModel(
            LoopName: loopName,
            ConditionId: conditionId,
            MaxIterations: maxIterations,
            FirstBodyStepName: firstBodyStepName,
            LastBodyStepName: lastBodyStepName,
            ContinuationStepName: continuationStepName,
            ParentLoopName: parentLoopName,
            BranchOnExitId: branchOnExitId,
            BranchOnExit: branchOnExit);
    }
}