// =============================================================================
// <copyright file="GenerateCode.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Steps;
using AgenticCoder.Services;
using AgenticCoder.State;

namespace AgenticCoder.Steps;

/// <summary>
/// Generates code based on the task description and plan.
/// </summary>
/// <remarks>
/// On subsequent iterations, uses feedback from failed tests to improve the code.
/// </remarks>
public sealed class GenerateCode : IWorkflowStep<CoderState>
{
    private readonly ICodeGenerator _generator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateCode"/> class.
    /// </summary>
    /// <param name="generator">The code generator service.</param>
    public GenerateCode(ICodeGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(generator, nameof(generator));
        _generator = generator;
    }

    /// <inheritdoc/>
    public async Task<StepResult<CoderState>> ExecuteAsync(
        CoderState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        // Get previous attempt and feedback if available
        var previousAttempt = state.Attempts.Count > 0
            ? state.Attempts[^1].Code
            : null;

        var feedback = state.LatestTestResults is { Passed: false }
            ? string.Join("\n", state.LatestTestResults.Failures)
            : null;

        // Generate new code
        var (code, reasoning) = await _generator.GenerateCodeAsync(
            state.TaskDescription,
            state.Plan ?? string.Empty,
            previousAttempt,
            feedback,
            cancellationToken);

        // Create the new attempt
        var attempt = new CodeAttempt(code, reasoning, DateTimeOffset.UtcNow);

        // Update state with new attempt
        var updatedState = state with
        {
            Attempts = [.. state.Attempts, attempt],
            AttemptCount = state.AttemptCount + 1,
        };

        return StepResult<CoderState>.FromState(updatedState);
    }
}
