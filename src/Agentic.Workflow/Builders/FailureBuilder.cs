// =============================================================================
// <copyright file="FailureBuilder.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Builders;

/// <summary>
/// Internal implementation of the failure handler path builder.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
internal sealed class FailureBuilder<TState> : IFailureBuilder<TState>
    where TState : class, IWorkflowState
{
    private readonly List<StepDefinition> _steps = [];

    /// <summary>
    /// Gets the steps in this failure handler path.
    /// </summary>
    internal IReadOnlyList<StepDefinition> Steps => _steps;

    /// <summary>
    /// Gets a value indicating whether this failure handler terminates without rejoining.
    /// </summary>
    internal bool IsTerminal { get; private set; }

    /// <inheritdoc/>
    public IFailureBuilder<TState> Then<TStep>()
        where TStep : class, IWorkflowStep<TState>
    {
        var step = StepDefinition.Create(typeof(TStep));
        _steps.Add(step);
        return this;
    }

    /// <inheritdoc/>
    public IFailureBuilder<TState> Then<TStep>(string instanceName)
        where TStep : class, IWorkflowStep<TState>
    {
        ArgumentNullException.ThrowIfNull(instanceName, nameof(instanceName));

        var step = StepDefinition.Create(typeof(TStep), customName: null, instanceName: instanceName);
        _steps.Add(step);
        return this;
    }

    /// <inheritdoc/>
    public void Complete()
    {
        IsTerminal = true;
    }
}
