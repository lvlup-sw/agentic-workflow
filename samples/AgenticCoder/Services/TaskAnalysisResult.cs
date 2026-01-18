// =============================================================================
// <copyright file="TaskAnalysisResult.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace AgenticCoder.Services;

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
