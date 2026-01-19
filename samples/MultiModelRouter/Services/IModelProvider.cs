// =============================================================================
// <copyright file="IModelProvider.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace MultiModelRouter.Services;

/// <summary>
/// Response from a model generation request.
/// </summary>
/// <param name="Content">The generated text content.</param>
/// <param name="Confidence">The model's confidence in the response (0.0 to 1.0).</param>
public sealed record ModelResponse(string Content, decimal Confidence);

/// <summary>
/// Contract for model providers that generate responses.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the LLM provider, allowing the workflow
/// to work with different models (GPT-4, Claude, local models) through
/// a unified interface.
/// </para>
/// </remarks>
public interface IModelProvider
{
    /// <summary>
    /// Generates a response for the given query using the specified model.
    /// </summary>
    /// <param name="modelId">The model identifier (e.g., "gpt-4", "claude-3").</param>
    /// <param name="query">The user query to respond to.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The model's response with confidence score.</returns>
    Task<ModelResponse> GenerateAsync(
        string modelId,
        string query,
        CancellationToken cancellationToken = default);
}
