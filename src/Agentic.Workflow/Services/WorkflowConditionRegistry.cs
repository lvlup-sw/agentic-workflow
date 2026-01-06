// =============================================================================
// <copyright file="WorkflowConditionRegistry.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Collections.Concurrent;

namespace Agentic.Workflow.Services;

/// <summary>
/// Static registry for workflow loop conditions that enables runtime lookup by generated sagas.
/// </summary>
/// <remarks>
/// <para>
/// This registry implements the Registry Pattern for handling lambda expressions that cannot
/// be transpiled to generated code at compile time. When <see cref="WorkflowBuilder{TState}.RepeatUntil"/>
/// is called, the condition delegate is stored here with a deterministic ID.
/// </para>
/// <para>
/// The generated saga code then calls <see cref="Evaluate{TState}"/> with the same condition ID
/// to retrieve and execute the original condition delegate against the current workflow state.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> This class uses <see cref="ConcurrentDictionary{TKey, TValue}"/>
/// for thread-safe registration and retrieval. Multiple workflows can register conditions concurrently.
/// </para>
/// <para>
/// <strong>Lifetime:</strong> Conditions are stored for the lifetime of the application.
/// This is intentional as workflow definitions are typically static and registered once at startup.
/// </para>
/// </remarks>
public static class WorkflowConditionRegistry
{
    private static readonly ConcurrentDictionary<string, object> Conditions = new();

    /// <summary>
    /// Registers a condition delegate with a deterministic ID.
    /// </summary>
    /// <typeparam name="TState">The workflow state type.</typeparam>
    /// <param name="conditionId">
    /// The unique condition identifier, typically in the format "{WorkflowName}-{LoopName}".
    /// </param>
    /// <param name="condition">The condition delegate to register.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="conditionId"/> or <paramref name="condition"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// If a condition with the same ID already exists, it will be replaced. This allows
    /// workflow definitions to be re-evaluated (e.g., during hot reload scenarios).
    /// </para>
    /// <para>
    /// The condition ID follows the format: "{WorkflowName}-{LoopName}".
    /// For example: "orchestrator-SpecialistExecution".
    /// </para>
    /// </remarks>
    public static void Register<TState>(string conditionId, Func<TState, bool> condition)
        where TState : class
    {
        ArgumentNullException.ThrowIfNull(conditionId, nameof(conditionId));
        ArgumentNullException.ThrowIfNull(condition, nameof(condition));

        // Use indexer to allow re-registration (e.g., hot reload)
        Conditions[conditionId] = condition;
    }

    /// <summary>
    /// Evaluates a registered condition against the current workflow state.
    /// </summary>
    /// <typeparam name="TState">The workflow state type.</typeparam>
    /// <param name="conditionId">The condition identifier to look up.</param>
    /// <param name="state">The current workflow state to evaluate.</param>
    /// <returns><c>true</c> if the loop should exit; <c>false</c> to continue iterating.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="conditionId"/> or <paramref name="state"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no condition with the specified ID is registered. This typically indicates
    /// that the workflow definition was not evaluated before the saga tried to check the condition.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is called by source-generated saga code to evaluate loop exit conditions.
    /// The generated <c>ShouldExit{LoopName}Loop()</c> methods delegate to this registry.
    /// </para>
    /// </remarks>
    public static bool Evaluate<TState>(string conditionId, TState state)
        where TState : class
    {
        ArgumentNullException.ThrowIfNull(conditionId, nameof(conditionId));
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        if (Conditions.TryGetValue(conditionId, out var condition))
        {
            return ((Func<TState, bool>)condition)(state);
        }

        throw new InvalidOperationException(
            $"Condition '{conditionId}' is not registered. " +
            "Ensure the workflow definition is evaluated before starting the saga. " +
            "This typically happens when accessing the Definition property triggers registration.");
    }

    /// <summary>
    /// Checks whether a condition with the specified ID is registered.
    /// </summary>
    /// <param name="conditionId">The condition identifier to check.</param>
    /// <returns><c>true</c> if the condition is registered; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="conditionId"/> is null.
    /// </exception>
    public static bool IsRegistered(string conditionId)
    {
        ArgumentNullException.ThrowIfNull(conditionId, nameof(conditionId));
        return Conditions.ContainsKey(conditionId);
    }

    /// <summary>
    /// Clears all registered conditions. Intended for testing purposes only.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Warning:</strong> Do not call this method in production code.
    /// It is provided for test isolation to reset state between test runs.
    /// </para>
    /// </remarks>
    internal static void ClearForTesting()
    {
        Conditions.Clear();
    }
}
