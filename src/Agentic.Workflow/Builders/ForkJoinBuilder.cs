// =============================================================================
// <copyright file="ForkJoinBuilder.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Builders;

/// <summary>
/// Internal implementation of the fork/join builder.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
internal sealed class ForkJoinBuilder<TState> : IForkJoinBuilder<TState>
    where TState : class, IWorkflowState
{
    private readonly WorkflowBuilder<TState> _workflowBuilder;
    private readonly ForkPointDefinition _pendingForkPoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForkJoinBuilder{TState}"/> class.
    /// </summary>
    /// <param name="workflowBuilder">The parent workflow builder.</param>
    /// <param name="pendingForkPoint">The fork point awaiting a join step.</param>
    internal ForkJoinBuilder(
        WorkflowBuilder<TState> workflowBuilder,
        ForkPointDefinition pendingForkPoint)
    {
        _workflowBuilder = workflowBuilder;
        _pendingForkPoint = pendingForkPoint;
    }

    /// <inheritdoc/>
    public IWorkflowBuilder<TState> Join<TJoinStep>()
        where TJoinStep : class, IWorkflowStep<TState>
    {
        // Create the join step
        var joinStep = StepDefinition.Create(typeof(TJoinStep));

        // Complete the fork point with the join step ID
        var completedForkPoint = _pendingForkPoint with { JoinStepId = joinStep.StepId };

        // Register the fork point and join step with the workflow builder
        _workflowBuilder.CompleteForkJoin(completedForkPoint, joinStep);

        return _workflowBuilder;
    }
}