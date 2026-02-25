// =============================================================================
// <copyright file="StepResult.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Steps;

/// <summary>
/// Immutable result of a workflow step execution.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <param name="UpdatedState">The updated workflow state after step execution.</param>
/// <param name="Confidence">Optional confidence score (0.0 to 1.0) for agent steps.</param>
/// <param name="Metadata">Optional execution metadata.</param>
/// <remarks>
/// <para>
/// Step results capture the outcome of a single step execution:
/// <list type="bullet">
///   <item><description>UpdatedState: The mutated workflow state (required)</description></item>
///   <item><description>Confidence: Agent step confidence score for review decisions</description></item>
///   <item><description>Metadata: Execution metrics, model info, and diagnostic data</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record StepResult<TState>(
    TState UpdatedState,
    double? Confidence = null,
    IReadOnlyDictionary<string, object>? Metadata = null)
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Creates a step result from the provided state with no additional metadata.
    /// </summary>
    /// <param name="state">The updated workflow state.</param>
    /// <returns>A new step result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    public static StepResult<TState> FromState(TState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        return new StepResult<TState>(state);
    }

    /// <summary>
    /// Creates a step result with a confidence score.
    /// </summary>
    /// <param name="state">The updated workflow state.</param>
    /// <param name="confidence">The confidence score (0.0 to 1.0).</param>
    /// <returns>A new step result with confidence.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="confidence"/> is outside [0, 1].</exception>
    public static StepResult<TState> WithConfidence(TState state, double confidence)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentOutOfRangeException.ThrowIfLessThan(confidence, 0.0, nameof(confidence));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(confidence, 1.0, nameof(confidence));

        return new StepResult<TState>(state, confidence);
    }

    /// <summary>
    /// Creates a new step result with the specified metadata.
    /// </summary>
    /// <param name="metadata">The execution metadata.</param>
    /// <returns>A new step result with metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is null.</exception>
    public StepResult<TState> WithMetadata(IReadOnlyDictionary<string, object> metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));

        return this with { Metadata = metadata };
    }
}
