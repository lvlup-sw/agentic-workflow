// =============================================================================
// <copyright file="RunTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Steps;
using AgenticCoder.Services;
using AgenticCoder.State;

namespace AgenticCoder.Steps;

/// <summary>
/// Runs tests against the latest generated code.
/// </summary>
public sealed class RunTests : IWorkflowStep<CoderState>
{
    private readonly ITestRunner _runner;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunTests"/> class.
    /// </summary>
    /// <param name="runner">The test runner service.</param>
    public RunTests(ITestRunner runner)
    {
        ArgumentNullException.ThrowIfNull(runner, nameof(runner));
        _runner = runner;
    }

    /// <inheritdoc/>
    public async Task<StepResult<CoderState>> ExecuteAsync(
        CoderState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        if (state.Attempts.Count == 0)
        {
            throw new InvalidOperationException("Cannot run tests - no code attempts exist.");
        }

        // Get the latest code attempt
        var latestCode = state.Attempts[^1].Code;

        // Run tests
        var results = await _runner.RunTestsAsync(latestCode, state.TaskDescription, cancellationToken);

        var updatedState = state with { LatestTestResults = results };
        return StepResult<CoderState>.FromState(updatedState);
    }
}
