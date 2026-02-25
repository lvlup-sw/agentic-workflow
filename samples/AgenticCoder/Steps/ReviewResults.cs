// =============================================================================
// <copyright file="ReviewResults.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Steps;
using AgenticCoder.State;

namespace AgenticCoder.Steps;

/// <summary>
/// Reviews test results to determine next workflow action.
/// </summary>
/// <remarks>
/// This step acts as a decision point in the workflow loop.
/// The loop condition will check the test results to determine
/// whether to continue iterating or proceed to human approval.
/// </remarks>
public sealed class ReviewResults : IWorkflowStep<CoderState>
{
    /// <inheritdoc/>
    public Task<StepResult<CoderState>> ExecuteAsync(
        CoderState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        // This step serves as a checkpoint in the refinement loop.
        // The actual loop control is handled by the workflow's RepeatUntil condition.
        // Here we could add logging, metrics, or additional analysis.

        return Task.FromResult(StepResult<CoderState>.FromState(state));
    }
}
