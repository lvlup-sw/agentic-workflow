// -----------------------------------------------------------------------
// <copyright file="ForkPathModel.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Polyfills;

namespace Agentic.Workflow.Generators.Models;

/// <summary>
/// Represents a single path within a fork construct for code generation.
/// </summary>
/// <remarks>
/// <para>
/// Fork path models capture the structure of parallel execution paths.
/// The source generator uses this model to emit:
/// - Path-specific phase enum values with path index prefix
/// - Path status tracking properties in saga state
/// - Path step handlers with join readiness checks
/// </para>
/// </remarks>
/// <param name="PathIndex">The zero-based index of this path within the fork.</param>
/// <param name="StepNames">The ordered list of step names in this path.</param>
/// <param name="HasFailureHandler">Whether this path has a failure handler defined.</param>
/// <param name="IsTerminalOnFailure">Whether the failure handler terminates without recovery.</param>
/// <param name="FailureHandlerStepNames">The optional list of failure handler step names.</param>
internal sealed record ForkPathModel(
    int PathIndex,
    IReadOnlyList<string> StepNames,
    bool HasFailureHandler,
    bool IsTerminalOnFailure,
    IReadOnlyList<string>? FailureHandlerStepNames = null)
{
    /// <summary>
    /// Gets the first step name in this path.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="StepNames"/> is empty.</exception>
    public string FirstStepName => StepNames.Count > 0
        ? StepNames[0]
        : throw new InvalidOperationException("Cannot access FirstStepName: StepNames is empty.");

    /// <summary>
    /// Gets the last step name in this path.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="StepNames"/> is empty.</exception>
    public string LastStepName => StepNames.Count > 0
        ? StepNames[StepNames.Count - 1]
        : throw new InvalidOperationException("Cannot access LastStepName: StepNames is empty.");

    /// <summary>
    /// Gets the property name for tracking this path's status.
    /// </summary>
    /// <remarks>
    /// Returns "Path{N}Status" where N is the path index (e.g., "Path0Status", "Path1Status").
    /// </remarks>
    public string StatusPropertyName => $"Path{PathIndex}Status";

    /// <summary>
    /// Gets the property name for storing this path's final state.
    /// </summary>
    /// <remarks>
    /// Returns "Path{N}State" where N is the path index (e.g., "Path0State", "Path1State").
    /// </remarks>
    public string StatePropertyName => $"Path{PathIndex}State";

    /// <summary>
    /// Creates a new <see cref="ForkPathModel"/> with validation.
    /// </summary>
    /// <param name="pathIndex">The zero-based index of this path. Must be non-negative.</param>
    /// <param name="stepNames">The ordered list of step names. Must have at least one step.</param>
    /// <param name="hasFailureHandler">Whether this path has a failure handler defined.</param>
    /// <param name="isTerminalOnFailure">Whether the failure handler terminates without recovery.</param>
    /// <param name="failureHandlerStepNames">The optional list of failure handler step names.</param>
    /// <returns>A validated <see cref="ForkPathModel"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepNames"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stepNames"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pathIndex"/> is negative.</exception>
    public static ForkPathModel Create(
        int pathIndex,
        IReadOnlyList<string> stepNames,
        bool hasFailureHandler,
        bool isTerminalOnFailure,
        IReadOnlyList<string>? failureHandlerStepNames = null)
    {
        ThrowHelper.ThrowIfNull(stepNames, nameof(stepNames));
        ThrowHelper.ThrowIfLessThan(pathIndex, 0, nameof(pathIndex));

        if (stepNames.Count == 0)
        {
            throw new ArgumentException("Fork path must have at least one step.", nameof(stepNames));
        }

        return new ForkPathModel(
            PathIndex: pathIndex,
            StepNames: stepNames,
            HasFailureHandler: hasFailureHandler,
            IsTerminalOnFailure: isTerminalOnFailure,
            FailureHandlerStepNames: failureHandlerStepNames);
    }
}