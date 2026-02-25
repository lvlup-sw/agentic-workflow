// =============================================================================
// <copyright file="GenerateResponse.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Steps;
using MultiModelRouter.Services;
using MultiModelRouter.State;

namespace MultiModelRouter.Steps;

/// <summary>
/// Generates a response using the selected model.
/// </summary>
/// <remarks>
/// <para>
/// This step uses the <see cref="IModelProvider"/> to generate a response
/// for the user query using the model selected by Thompson Sampling.
/// </para>
/// </remarks>
public sealed class GenerateResponse : IWorkflowStep<RouterState>
{
    private readonly IModelProvider _modelProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateResponse"/> class.
    /// </summary>
    /// <param name="modelProvider">The model provider for generating responses.</param>
    public GenerateResponse(IModelProvider modelProvider)
    {
        ArgumentNullException.ThrowIfNull(modelProvider);
        _modelProvider = modelProvider;
    }

    /// <inheritdoc/>
    public async Task<StepResult<RouterState>> ExecuteAsync(
        RouterState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(state.SelectedModel))
        {
            throw new ArgumentException("Selected model is required.", nameof(state));
        }

        if (string.IsNullOrWhiteSpace(state.UserQuery))
        {
            throw new ArgumentException("User query is required.", nameof(state));
        }

        var response = await _modelProvider.GenerateAsync(
            state.SelectedModel,
            state.UserQuery,
            cancellationToken);

        var updatedState = state with
        {
            Response = response.Content,
            Confidence = response.Confidence,
        };

        return StepResult<RouterState>.FromState(updatedState);
    }
}
