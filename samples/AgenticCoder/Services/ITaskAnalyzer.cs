// =============================================================================
// <copyright file="ITaskAnalyzer.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace AgenticCoder.Services;

/// <summary>
/// Contract for analyzing coding tasks.
/// </summary>
public interface ITaskAnalyzer
{
    /// <summary>
    /// Analyzes a task description and validates it can be implemented.
    /// </summary>
    /// <param name="taskDescription">The task to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Analysis result with validation and complexity assessment.</returns>
    Task<TaskAnalysisResult> AnalyzeTaskAsync(
        string taskDescription,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of task analysis.
/// </summary>
/// <param name="IsValid">Whether the task can be implemented.</param>
/// <param name="Complexity">Estimated complexity (Low, Medium, High).</param>
/// <param name="Requirements">Extracted requirements from the task.</param>
public sealed record TaskAnalysisResult(
    bool IsValid,
    string Complexity,
    IReadOnlyList<string> Requirements);
