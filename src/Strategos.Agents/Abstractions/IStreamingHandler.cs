// -----------------------------------------------------------------------
// <copyright file="IStreamingHandler.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Strategos.Agents.Abstractions;

/// <summary>
/// Handles streaming tokens from LLM agent responses.
/// </summary>
/// <remarks>
/// Implementations can use this interface to provide real-time feedback
/// during agent execution, such as displaying tokens as they arrive
/// or tracking generation progress.
/// </remarks>
public interface IStreamingHandler
{
    /// <summary>
    /// Called when a new token is received from the streaming response.
    /// </summary>
    /// <param name="token">The received token text.</param>
    /// <param name="workflowId">The workflow execution identifier.</param>
    /// <param name="stepName">The current step name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    Task OnTokenReceivedAsync(
        string token,
        Guid workflowId,
        string stepName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the streaming response is complete.
    /// </summary>
    /// <param name="fullResponse">The complete response text.</param>
    /// <param name="workflowId">The workflow execution identifier.</param>
    /// <param name="stepName">The current step name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    Task OnResponseCompletedAsync(
        string fullResponse,
        Guid workflowId,
        string stepName,
        CancellationToken cancellationToken = default);
}
