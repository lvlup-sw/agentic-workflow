// =============================================================================
// <copyright file="AnalyzeTask.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Steps;
using AgenticCoder.Services;
using AgenticCoder.State;

namespace AgenticCoder.Steps;

/// <summary>
/// Analyzes the coding task to validate and understand requirements.
/// </summary>
public sealed class AnalyzeTask : IWorkflowStep<CoderState>
{
    private readonly ITaskAnalyzer _analyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeTask"/> class.
    /// </summary>
    /// <param name="analyzer">The task analyzer service.</param>
    public AnalyzeTask(ITaskAnalyzer analyzer)
    {
        ArgumentNullException.ThrowIfNull(analyzer, nameof(analyzer));
        _analyzer = analyzer;
    }

    /// <inheritdoc/>
    public async Task<StepResult<CoderState>> ExecuteAsync(
        CoderState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        var analysis = await _analyzer.AnalyzeTaskAsync(state.TaskDescription, cancellationToken);

        if (!analysis.IsValid)
        {
            throw new InvalidOperationException($"Task is not valid: {state.TaskDescription}");
        }

        // State remains unchanged - analysis is used in the next step
        return StepResult<CoderState>.FromState(state);
    }
}
