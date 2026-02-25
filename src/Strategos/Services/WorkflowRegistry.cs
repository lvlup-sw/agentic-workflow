// =============================================================================
// <copyright file="WorkflowRegistry.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Strategos.Services;

/// <summary>
/// Thread-safe implementation of <see cref="IWorkflowRegistry"/>.
/// </summary>
/// <remarks>
/// <para>
/// This implementation uses a <see cref="ConcurrentDictionary{TKey, TValue}"/> for
/// thread-safe registration and retrieval of conditions. Conditions are stored as
/// compiled delegates with their original expression text for auditability.
/// </para>
/// <para>
/// The registry should be registered as a singleton in the DI container.
/// </para>
/// </remarks>
public sealed class WorkflowRegistry : IWorkflowRegistry
{
    private readonly ConcurrentDictionary<string, object> _conditions = new();

    /// <inheritdoc/>
    [RequiresDynamicCode("Expression.Compile() generates code at runtime. Use RegisterConditionAot for NativeAOT.")]
    [RequiresUnreferencedCode("Expression.Compile() requires unreferenced code. Use RegisterConditionAot for NativeAOT.")]
    public void RegisterCondition<TState>(string conditionId, Expression<Func<TState, bool>> condition)
        where TState : class, IWorkflowState
    {
        ArgumentNullException.ThrowIfNull(conditionId, nameof(conditionId));
        ArgumentNullException.ThrowIfNull(condition, nameof(condition));

        var wrapper = new WorkflowCondition<TState>(conditionId, condition);

        if (!_conditions.TryAdd(conditionId, wrapper))
        {
            throw new InvalidOperationException(
                $"A condition with ID '{conditionId}' is already registered.");
        }
    }

    /// <inheritdoc/>
    public void RegisterConditionAot<TState>(
        string conditionId,
        Func<TState, bool> condition,
        string? expressionText = null)
        where TState : class, IWorkflowState
    {
        ArgumentNullException.ThrowIfNull(conditionId, nameof(conditionId));
        ArgumentNullException.ThrowIfNull(condition, nameof(condition));

        var wrapper = new WorkflowConditionAot<TState>(conditionId, condition, expressionText);

        if (!_conditions.TryAdd(conditionId, wrapper))
        {
            throw new InvalidOperationException(
                $"A condition with ID '{conditionId}' is already registered.");
        }
    }

    /// <inheritdoc/>
    public IWorkflowCondition<TState> GetCondition<TState>(string conditionId)
        where TState : class, IWorkflowState
    {
        if (_conditions.TryGetValue(conditionId, out var wrapper))
        {
            return (IWorkflowCondition<TState>)wrapper;
        }

        throw new KeyNotFoundException(
            $"No condition with ID '{conditionId}' is registered.");
    }

    /// <inheritdoc/>
    public bool TryGetCondition<TState>(string conditionId, out IWorkflowCondition<TState>? condition)
        where TState : class, IWorkflowState
    {
        if (_conditions.TryGetValue(conditionId, out var wrapper))
        {
            condition = (IWorkflowCondition<TState>)wrapper;
            return true;
        }

        condition = null;
        return false;
    }
}

/// <summary>
/// Internal implementation of <see cref="IWorkflowCondition{TState}"/> using expression compilation.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <remarks>
/// <strong>NativeAOT:</strong> This class uses <see cref="Expression{TDelegate}.Compile()"/>
/// which is not compatible with NativeAOT. Use <see cref="WorkflowConditionAot{TState}"/> instead.
/// </remarks>
internal sealed class WorkflowCondition<TState> : IWorkflowCondition<TState>
    where TState : class, IWorkflowState
{
    private readonly Func<TState, bool> _compiledCondition;
    private readonly string _expressionText;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowCondition{TState}"/> class.
    /// </summary>
    /// <param name="conditionId">The unique condition identifier.</param>
    /// <param name="expression">The condition expression.</param>
    [RequiresDynamicCode("Expression.Compile() generates code at runtime.")]
    [RequiresUnreferencedCode("Expression.Compile() requires unreferenced code.")]
    public WorkflowCondition(string conditionId, Expression<Func<TState, bool>> expression)
    {
        ConditionId = conditionId;
        _compiledCondition = expression.Compile();
        _expressionText = expression.ToString();
    }

    /// <inheritdoc/>
    public string ConditionId { get; }

    /// <inheritdoc/>
    public bool Execute(TState state) => _compiledCondition(state);

    /// <summary>
    /// Returns the original expression text for auditability.
    /// </summary>
    /// <returns>The expression text (e.g., "s => s.QualityScore >= 0.9").</returns>
    public override string ToString() => _expressionText;
}

/// <summary>
/// NativeAOT-compatible implementation of <see cref="IWorkflowCondition{TState}"/>.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <remarks>
/// This implementation accepts a pre-compiled delegate rather than an expression,
/// making it compatible with NativeAOT compilation.
/// </remarks>
internal sealed class WorkflowConditionAot<TState> : IWorkflowCondition<TState>
    where TState : class, IWorkflowState
{
    private readonly Func<TState, bool> _condition;
    private readonly string _expressionText;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowConditionAot{TState}"/> class.
    /// </summary>
    /// <param name="conditionId">The unique condition identifier.</param>
    /// <param name="condition">The pre-compiled condition delegate.</param>
    /// <param name="expressionText">Optional human-readable expression text for auditability.</param>
    public WorkflowConditionAot(string conditionId, Func<TState, bool> condition, string? expressionText)
    {
        ConditionId = conditionId;
        _condition = condition;
        _expressionText = expressionText ?? "[AOT condition - no expression text provided]";
    }

    /// <inheritdoc/>
    public string ConditionId { get; }

    /// <inheritdoc/>
    public bool Execute(TState state) => _condition(state);

    /// <summary>
    /// Returns the expression text for auditability.
    /// </summary>
    /// <returns>The expression text if provided, or a placeholder message.</returns>
    public override string ToString() => _expressionText;
}
