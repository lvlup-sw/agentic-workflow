// =============================================================================
// <copyright file="MockTaskAnalyzer.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace AgenticCoder.Services;

/// <summary>
/// Mock implementation of task analyzer for demonstration.
/// </summary>
public sealed class MockTaskAnalyzer : ITaskAnalyzer
{
    /// <inheritdoc/>
    public Task<TaskAnalysisResult> AnalyzeTaskAsync(
        string taskDescription,
        CancellationToken cancellationToken = default)
    {
        // Normalize input to avoid NRE on null taskDescription
        var normalizedDescription = taskDescription ?? string.Empty;

        // Simple mock analysis based on task description content
        var isValid = !string.IsNullOrWhiteSpace(normalizedDescription);
        var complexity = normalizedDescription.Length > 100 ? "High" : "Low";

        var requirements = new List<string>
        {
            "Function must be public and static",
            "Must handle edge cases",
            "Must return expected type",
        };

        if (normalizedDescription.Contains("FizzBuzz", StringComparison.OrdinalIgnoreCase))
        {
            requirements.Add("Return 'Fizz' for multiples of 3");
            requirements.Add("Return 'Buzz' for multiples of 5");
            requirements.Add("Return 'FizzBuzz' for multiples of both 3 and 5");
            requirements.Add("Return the number as string for other values");
        }

        var result = new TaskAnalysisResult(isValid, complexity, requirements);
        return Task.FromResult(result);
    }
}
