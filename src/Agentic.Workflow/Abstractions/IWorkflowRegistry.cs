// =============================================================================
// <copyright file="IWorkflowRegistry.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Registry for workflow conditions that enables runtime lookup and auditability.
/// </summary>
/// <remarks>
/// <para>
/// The workflow registry implements the Registry Pattern for handling lambda expressions
/// in source-generated sagas. This pattern:
/// <list type="bullet">
///   <item><description>Keeps lambda expressions in memory for runtime evaluation</description></item>
///   <item><description>Allows generated saga code to look up conditions by deterministic ID</description></item>
///   <item><description>Enables auditability by preserving the expression text</description></item>
///   <item><description>Supports closures that cannot be transpiled to generated code</description></item>
/// </list>
/// </para>
/// <para>
/// The registry should be registered as a singleton in the DI container and populated
/// during workflow builder setup via the fluent DSL.
/// </para>
/// </remarks>
public interface IWorkflowRegistry
{
    /// <summary>
    /// Registers a condition expression with a deterministic ID.
    /// </summary>
    /// <typeparam name="TState">The workflow state type.</typeparam>
    /// <param name="conditionId">The unique condition identifier.</param>
    /// <param name="condition">The condition expression.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="conditionId"/> or <paramref name="condition"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a condition with the same ID is already registered.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The condition ID should follow the format "{WorkflowName}-{LoopOrBranchName}"
    /// (e.g., "ProcessClaim-Refinement"). The expression is compiled to a delegate
    /// for efficient runtime evaluation.
    /// </para>
    /// <para>
    /// Using <see cref="Expression{TDelegate}"/> instead of <see cref="Func{T, TResult}"/>
    /// enables:
    /// <list type="bullet">
    ///   <item><description>Auditability via Expression.ToString()</description></item>
    ///   <item><description>Runtime compilation for efficient execution</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>NativeAOT:</strong> This method uses <see cref="Expression{TDelegate}.Compile()"/>
    /// which is not compatible with NativeAOT. For AOT scenarios, use
    /// <see cref="RegisterConditionAot{TState}"/> instead.
    /// </para>
    /// </remarks>
    [RequiresDynamicCode("Expression.Compile() generates code at runtime. Use RegisterConditionAot for NativeAOT.")]
    [RequiresUnreferencedCode("Expression.Compile() requires unreferenced code. Use RegisterConditionAot for NativeAOT.")]
    void RegisterCondition<TState>(string conditionId, Expression<Func<TState, bool>> condition)
        where TState : class, IWorkflowState;

    /// <summary>
    /// Registers a pre-compiled condition delegate with a deterministic ID (NativeAOT compatible).
    /// </summary>
    /// <typeparam name="TState">The workflow state type.</typeparam>
    /// <param name="conditionId">The unique condition identifier.</param>
    /// <param name="condition">The pre-compiled condition delegate.</param>
    /// <param name="expressionText">Optional human-readable expression text for auditability.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="conditionId"/> or <paramref name="condition"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a condition with the same ID is already registered.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is NativeAOT compatible as it accepts a pre-compiled delegate
    /// rather than an expression that requires runtime compilation.
    /// </para>
    /// <para>
    /// For auditability, pass the <paramref name="expressionText"/> parameter with
    /// a human-readable representation of the condition (e.g., "s => s.Quality >= 0.9").
    /// </para>
    /// </remarks>
    void RegisterConditionAot<TState>(
        string conditionId,
        Func<TState, bool> condition,
        string? expressionText = null)
        where TState : class, IWorkflowState;

    /// <summary>
    /// Retrieves a registered condition by ID.
    /// </summary>
    /// <typeparam name="TState">The workflow state type.</typeparam>
    /// <param name="conditionId">The condition identifier.</param>
    /// <returns>The workflow condition wrapper.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no condition with the specified ID exists.
    /// </exception>
    IWorkflowCondition<TState> GetCondition<TState>(string conditionId)
        where TState : class, IWorkflowState;

    /// <summary>
    /// Attempts to retrieve a registered condition by ID.
    /// </summary>
    /// <typeparam name="TState">The workflow state type.</typeparam>
    /// <param name="conditionId">The condition identifier.</param>
    /// <param name="condition">
    /// When this method returns, contains the condition if found; otherwise, null.
    /// </param>
    /// <returns>True if the condition was found; otherwise, false.</returns>
    bool TryGetCondition<TState>(string conditionId, out IWorkflowCondition<TState>? condition)
        where TState : class, IWorkflowState;
}