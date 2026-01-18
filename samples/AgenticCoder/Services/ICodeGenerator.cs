// =============================================================================
// <copyright file="ICodeGenerator.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace AgenticCoder.Services;

/// <summary>
/// Contract for AI-powered code generation.
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Generates code based on a task description and optional feedback.
    /// </summary>
    /// <param name="taskDescription">The coding task to implement.</param>
    /// <param name="plan">The implementation plan.</param>
    /// <param name="previousAttempt">Optional previous code attempt for refinement.</param>
    /// <param name="feedback">Optional feedback from failed tests.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated code and reasoning.</returns>
    Task<(string Code, string Reasoning)> GenerateCodeAsync(
        string taskDescription,
        string plan,
        string? previousAttempt = null,
        string? feedback = null,
        CancellationToken cancellationToken = default);
}
