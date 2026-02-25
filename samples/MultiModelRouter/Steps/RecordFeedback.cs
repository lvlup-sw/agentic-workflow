// =============================================================================
// <copyright file="RecordFeedback.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Selection;
using Strategos.Steps;
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

        if (string.IsNullOrWhiteSpace(state.SelectedModel))
        {
            throw new ArgumentException("Selected model is required to record feedback.", nameof(state));
        }

        var isSuccess = state.Feedback.Rating >= SuccessThreshold;
        var outcome = isSuccess
            ? AgentOutcome.Succeeded(confidence: (double)state.Confidence)
            : AgentOutcome.Failed(confidence: (double)state.Confidence);

        // Map QueryCategory to TaskCategory for consistent belief keys
        var taskCategory = MapToTaskCategory(state.Category);

        // Record outcome to update Thompson Sampling beliefs
        await _agentSelector.RecordOutcomeAsync(
            state.SelectedModel,
            taskCategory.ToString(),
            outcome,
            cancellationToken);

        return StepResult<RouterState>.FromState(state);
    }

    /// <summary>
    /// Maps a query category to the corresponding task category for Thompson Sampling.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This mapping ensures belief key alignment between model selection and feedback recording.
    /// The MockAgentSelector uses TaskCategory for belief keys, so feedback must use the same
    /// taxonomy to ensure Thompson Sampling learns correctly.
    /// </para>
    /// </remarks>
    /// <param name="queryCategory">The query category from classification.</param>
    /// <returns>The corresponding task category for belief tracking.</returns>
    private static TaskCategory MapToTaskCategory(QueryCategory queryCategory)
    {
        return queryCategory switch
        {
            QueryCategory.Technical => TaskCategory.CodeGeneration,
            QueryCategory.Creative => TaskCategory.TextGeneration,
            QueryCategory.Factual => TaskCategory.General,
            QueryCategory.Conversational => TaskCategory.General,
            _ => TaskCategory.General,
        };
    }
}
