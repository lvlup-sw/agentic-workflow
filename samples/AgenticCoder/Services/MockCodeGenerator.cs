// =============================================================================
// <copyright file="MockCodeGenerator.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace AgenticCoder.Services;

/// <summary>
/// Mock implementation of code generator for demonstration.
/// </summary>
/// <remarks>
/// Simulates AI code generation with configurable behavior for testing
/// the workflow's iteration and error handling capabilities.
/// </remarks>
public sealed class MockCodeGenerator : ICodeGenerator
{
    private int _attemptNumber;

    /// <summary>
    /// Gets or sets the number of attempts before generating correct code.
    /// </summary>
    public int AttemptsBeforeSuccess { get; set; } = 2;

    /// <inheritdoc/>
    public Task<(string Code, string Reasoning)> GenerateCodeAsync(
        string taskDescription,
        string plan,
        string? previousAttempt = null,
        string? feedback = null,
        CancellationToken cancellationToken = default)
    {
        _attemptNumber++;

        var (code, reasoning) = _attemptNumber >= AttemptsBeforeSuccess
            ? GenerateCorrectCode(taskDescription)
            : GenerateFlawedCode(taskDescription, _attemptNumber);

        return Task.FromResult((code, reasoning));
    }

    /// <summary>
    /// Resets the attempt counter for fresh test runs.
    /// </summary>
    public void Reset()
    {
        _attemptNumber = 0;
    }

    private static (string Code, string Reasoning) GenerateCorrectCode(string taskDescription)
    {
        // Generate a simple FizzBuzz-style implementation
        var code = """
            public static class FizzBuzz
            {
                public static string GetResult(int number)
                {
                    if (number % 15 == 0) return "FizzBuzz";
                    if (number % 3 == 0) return "Fizz";
                    if (number % 5 == 0) return "Buzz";
                    return number.ToString();
                }
            }
            """;

        var reasoning = """
            Implemented FizzBuzz with correct divisibility checks:
            1. Check for 15 first (both 3 and 5)
            2. Then check for 3 (Fizz)
            3. Then check for 5 (Buzz)
            4. Default to the number itself
            """;

        return (code, reasoning);
    }

    private static (string Code, string Reasoning) GenerateFlawedCode(string taskDescription, int attempt)
    {
        // Generate intentionally flawed code to demonstrate iteration
        var code = attempt switch
        {
            1 => """
                public static class FizzBuzz
                {
                    public static string GetResult(int number)
                    {
                        // Bug: Missing FizzBuzz check for multiples of 15
                        if (number % 3 == 0) return "Fizz";
                        if (number % 5 == 0) return "Buzz";
                        return number.ToString();
                    }
                }
                """,
            _ => """
                public static class FizzBuzz
                {
                    public static string GetResult(int number)
                    {
                        // Bug: Wrong order of checks
                        if (number % 3 == 0) return "Fizz";
                        if (number % 15 == 0) return "FizzBuzz";
                        if (number % 5 == 0) return "Buzz";
                        return number.ToString();
                    }
                }
                """,
        };

        var reasoning = $"Attempt {attempt}: Generated initial implementation based on requirements.";

        return (code, reasoning);
    }
}
