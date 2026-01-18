// =============================================================================
// <copyright file="ITestRunner.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using AgenticCoder.State;

namespace AgenticCoder.Services;

/// <summary>
/// Contract for running tests against generated code.
/// </summary>
public interface ITestRunner
{
    /// <summary>
    /// Runs tests against the provided code.
    /// </summary>
    /// <param name="code">The code to test.</param>
    /// <param name="taskDescription">The original task description for context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Test execution results.</returns>
    Task<TestResults> RunTestsAsync(
        string code,
        string taskDescription,
        CancellationToken cancellationToken = default);
}
