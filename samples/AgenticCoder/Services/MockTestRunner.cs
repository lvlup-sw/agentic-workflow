// =============================================================================
// <copyright file="MockTestRunner.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using AgenticCoder.State;

namespace AgenticCoder.Services;

/// <summary>
/// Mock implementation of test runner for demonstration.
/// </summary>
/// <remarks>
/// Simulates test execution by checking for known patterns in the code.
/// Used to demonstrate workflow iteration when tests fail.
/// </remarks>
public sealed class MockTestRunner : ITestRunner
{
    /// <inheritdoc/>
    public Task<TestResults> RunTestsAsync(
        string code,
        string taskDescription,
        CancellationToken cancellationToken = default)
    {
        var failures = new List<string>();

        // Check for common FizzBuzz implementation issues
        if (!code.Contains("% 15") && !code.Contains("%15"))
        {
            failures.Add("Test_FizzBuzz_MultipleOf15_ReturnsFizzBuzz: Expected 'FizzBuzz' but got 'Fizz'");
        }

        // Check that 15 is checked before 3 and 5
        var indexOf15 = code.IndexOf("% 15", StringComparison.Ordinal);
        if (indexOf15 < 0)
        {
            indexOf15 = code.IndexOf("%15", StringComparison.Ordinal);
        }

        var indexOf3 = code.IndexOf("% 3", StringComparison.Ordinal);
        if (indexOf3 < 0)
        {
            indexOf3 = code.IndexOf("%3", StringComparison.Ordinal);
        }

        if (indexOf15 >= 0 && indexOf3 >= 0 && indexOf15 > indexOf3)
        {
            failures.Add("Test_FizzBuzz_MultipleOf15_ReturnsFizzBuzz: Check order is wrong - 15 should be checked before 3");
        }

        var passed = failures.Count == 0;
        var result = new TestResults(passed, failures);

        return Task.FromResult(result);
    }
}
