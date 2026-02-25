// =============================================================================
// <copyright file="SelectModel.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Selection;
using Strategos.Steps;
using MultiModelRouter.State;

namespace MultiModelRouter.Steps;

/// <summary>
/// Selects the optimal model using Thompson Sampling agent selection.
/// </summary>
/// <remarks>
/// <para>
/// This step uses the <see cref="IAgentSelector"/> to select the best model
/// for the given query category. Thompson Sampling balances exploration
/// (trying different models) with exploitation (using known-good models).
/// </para>
/// <para>
/// If the selection confidence is below the threshold, the step falls back
/// to the expensive model (GPT-4) to ensure quality responses.
/// </para>
/// </remarks>
public sealed class SelectModel : IWorkflowStep<RouterState>
{
    /// <summary>
    /// The fallback model used when confidence is too low.
    /// </summary>
    public const string FallbackModel = "gpt-4";

    private static readonly string[] AvailableModels = ["gpt-4", "claude-3", "local-model"];

    private readonly IAgentSelector _agentSelector;
    private readonly decimal _confidenceThreshold;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectModel"/> class.
    /// </summary>
    /// <param name="agentSelector">The agent selector for Thompson Sampling.</param>
    /// <param name="confidenceThreshold">Minimum confidence to accept selection (default 0.5).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when confidenceThreshold is not between 0.0 and 1.0.</exception>
    public SelectModel(IAgentSelector agentSelector, decimal confidenceThreshold = 0.5m)
    {
        ArgumentNullException.ThrowIfNull(agentSelector);

        if (confidenceThreshold is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(confidenceThreshold), "Must be between 0.0 and 1.0.");
        }

        _agentSelector = agentSelector;
        _confidenceThreshold = confidenceThreshold;
    }

    /// <inheritdoc/>
    public async Task<StepResult<RouterState>> ExecuteAsync(
        RouterState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(state.UserQuery))
        {
            throw new ArgumentException("UserQuery must not be null or whitespace for SelectModel.ExecuteAsync", nameof(state));
        }

        var selectionContext = new AgentSelectionContext
        {
            WorkflowId = state.WorkflowId,
            StepName = context.StepName,
            TaskDescription = state.UserQuery,
            AvailableAgents = AvailableModels,
        };

        var selectionResult = await _agentSelector.SelectAgentAsync(selectionContext, cancellationToken);

        if (!selectionResult.IsSuccess)
        {
            // Fallback to expensive model on selection failure
            var fallbackState = state with
            {
                SelectedModel = FallbackModel,
                Confidence = 0m,
            };
            return StepResult<RouterState>.FromState(fallbackState);
        }

        var selection = selectionResult.Value;
        var confidence = (decimal)selection.SelectionConfidence;

        // If confidence is too low, fall back to expensive model
        if (confidence < _confidenceThreshold)
        {
            var fallbackState = state with
            {
                SelectedModel = FallbackModel,
                Confidence = confidence,
            };
            return StepResult<RouterState>.FromState(fallbackState);
        }

        var updatedState = state with
        {
            SelectedModel = selection.SelectedAgentId,
            Confidence = confidence,
        };

        return StepResult<RouterState>.FromState(updatedState);
    }
}

