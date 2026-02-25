// =============================================================================
// <copyright file="PlanImplementation.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Steps;
using AgenticCoder.Services;
using AgenticCoder.State;

namespace AgenticCoder.Steps;

/// <summary>
/// Creates an implementation plan for the coding task.
/// </summary>
public sealed class PlanImplementation : IWorkflowStep<CoderState>
{
    private readonly IPlanner _planner;
    private readonly ITaskAnalyzer _analyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanImplementation"/> class.
    /// </summary>
    /// <param name="planner">The planner service.</param>
    /// <param name="analyzer">The task analyzer service.</param>
    public PlanImplementation(IPlanner planner, ITaskAnalyzer analyzer)
    {
        ArgumentNullException.ThrowIfNull(planner, nameof(planner));
        ArgumentNullException.ThrowIfNull(analyzer, nameof(analyzer));
        _planner = planner;
        _analyzer = analyzer;
    }

    /// <inheritdoc/>
    public async Task<StepResult<CoderState>> ExecuteAsync(
        CoderState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        // Re-analyze to get requirements
        var analysis = await _analyzer.AnalyzeTaskAsync(state.TaskDescription, cancellationToken);

        // Create the implementation plan
        var plan = await _planner.CreatePlanAsync(
            state.TaskDescription,
            analysis.Requirements,
            cancellationToken);

        var updatedState = state with { Plan = plan };
        return StepResult<CoderState>.FromState(updatedState);
    }
}
