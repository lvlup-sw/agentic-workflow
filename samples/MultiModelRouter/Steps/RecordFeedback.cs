// =============================================================================
// <copyright file="RecordFeedback.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Selection;
using Agentic.Workflow.Steps;
using MultiModelRouter.State;

namespace MultiModelRouter.Steps;

/// <summary>
/// Records user feedback to update Thompson Sampling beliefs.
/// </summary>
/// <remarks>
/// <para>
/// This step records the outcome of a model generation to the agent selector,
/// enabling Thompson Sampling to learn from user feedback:
/// <list type="bullet">
///   <item><description>Rating >= 3: Success outcome (increments alpha)</description></item>
///   <item><description>Rating less than 3: Failure outcome (increments beta)</description></item>
/// </list>
/// </para>
/// <para>
/// This feedback loop enables the system to improve model selection over time.
/// </para>
/// </remarks>
public sealed class RecordFeedback : IWorkflowStep<RouterState>
{
    private const int SuccessThreshold = 3;

    private readonly IAgentSelector _agentSelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordFeedback"/> class.
    /// </summary>
    /// <param name="agentSelector">The agent selector for recording outcomes.</param>
    public RecordFeedback(IAgentSelector agentSelector)
    {
        ArgumentNullException.ThrowIfNull(agentSelector);
        _agentSelector = agentSelector;
    }

    /// <inheritdoc/>
    public async Task<StepResult<RouterState>> ExecuteAsync(
        RouterState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        // If no feedback provided, skip recording
        if (state.Feedback is null)
        {
            return StepResult<RouterState>.FromState(state);
        }

        var isSuccess = state.Feedback.Rating >= SuccessThreshold;
        var outcome = isSuccess
            ? AgentOutcome.Succeeded(confidence: (double)state.Confidence)
            : AgentOutcome.Failed(confidence: (double)state.Confidence);

        // Record outcome to update Thompson Sampling beliefs
        await _agentSelector.RecordOutcomeAsync(
            state.SelectedModel,
            state.Category.ToString(),
            outcome,
            cancellationToken);

        return StepResult<RouterState>.FromState(state);
    }
}
