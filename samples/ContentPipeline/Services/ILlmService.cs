// =============================================================================
// <copyright file="ILlmService.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace ContentPipeline.Services;

/// <summary>
/// Interface for LLM-based content generation and review services.
/// </summary>
public interface ILlmService
{
    /// <summary>
    /// Generates a draft based on the given prompt.
    /// </summary>
    /// <param name="prompt">The prompt describing what to generate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated draft content.</returns>
    Task<string> GenerateDraftAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reviews content and returns feedback with a quality score.
    /// </summary>
    /// <param name="content">The content to review.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple of feedback text and quality score (0.0 to 1.0).</returns>
    Task<(string Feedback, decimal QualityScore)> ReviewContentAsync(
        string content,
        CancellationToken cancellationToken = default);
}
