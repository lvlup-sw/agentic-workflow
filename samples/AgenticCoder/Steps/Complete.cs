// =============================================================================
// <copyright file="Complete.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Steps;
using AgenticCoder.State;

namespace AgenticCoder.Steps;

/// <summary>
/// Terminal step that completes the workflow.
/// </summary>
public sealed class Complete : IWorkflowStep<CoderState>
{
    /// <inheritdoc/>
    public Task<StepResult<CoderState>> ExecuteAsync(
        CoderState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        // Terminal step - workflow is complete
        return Task.FromResult(StepResult<CoderState>.FromState(state));
    }
}
