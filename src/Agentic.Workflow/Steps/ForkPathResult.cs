// =============================================================================
// <copyright file="ForkPathResult.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Definitions;

namespace Agentic.Workflow.Steps;

/// <summary>
/// Immutable result from a single fork path execution.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <remarks>
/// <para>
/// Fork path results capture the outcome of parallel path execution:
/// <list type="bullet">
///   <item><description>PathIndex: Zero-based index identifying the path within the fork</description></item>
///   <item><description>Status: Terminal status (Success, Failed, or FailedWithRecovery)</description></item>
///   <item><description>State: The final state from the path (null for terminal failures)</description></item>
/// </list>
/// </para>
/// <para>
/// The join step receives a <see cref="ForkContext{TState}"/> containing all path results
/// for state merge and continuation logic.
/// </para>
/// </remarks>
public sealed record ForkPathResult<TState>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Gets the zero-based index of this path within the fork.
    /// </summary>
    public required int PathIndex { get; init; }

    /// <summary>
    /// Gets the terminal status of this path.
    /// </summary>
    public required ForkPathStatus Status { get; init; }

    /// <summary>
    /// Gets the final state from this path, or null if the path failed terminally.
    /// </summary>
    /// <remarks>
    /// State is null when <see cref="Status"/> is <see cref="ForkPathStatus.Failed"/>
    /// because terminal failures clear path state to prevent merge corruption.
    /// </remarks>
    public TState? State { get; init; }

    /// <summary>
    /// Gets a value indicating whether this path completed successfully or recovered.
    /// </summary>
    /// <remarks>
    /// Returns true for <see cref="ForkPathStatus.Success"/> and
    /// <see cref="ForkPathStatus.FailedWithRecovery"/> statuses.
    /// </remarks>
    public bool IsSuccessful => Status is ForkPathStatus.Success or ForkPathStatus.FailedWithRecovery;

    /// <summary>
    /// Gets a value indicating whether this path has state available for merge.
    /// </summary>
    public bool HasState => State is not null;

    /// <summary>
    /// Creates a successful path result.
    /// </summary>
    /// <param name="pathIndex">The zero-based index of this path.</param>
    /// <param name="state">The final state from the path.</param>
    /// <returns>A new successful path result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pathIndex"/> is negative.</exception>
    public static ForkPathResult<TState> Success(int pathIndex, TState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentOutOfRangeException.ThrowIfNegative(pathIndex, nameof(pathIndex));

        return new ForkPathResult<TState>
        {
            PathIndex = pathIndex,
            Status = ForkPathStatus.Success,
            State = state,
        };
    }

    /// <summary>
    /// Creates a terminal failure path result.
    /// </summary>
    /// <param name="pathIndex">The zero-based index of this path.</param>
    /// <returns>A new failed path result with null state.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pathIndex"/> is negative.</exception>
    /// <remarks>
    /// Terminal failures occur when the failure handler calls <c>Complete()</c>.
    /// State is null because the path's contributions should not be merged.
    /// </remarks>
    public static ForkPathResult<TState> Failed(int pathIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pathIndex, nameof(pathIndex));

        return new ForkPathResult<TState>
        {
            PathIndex = pathIndex,
            Status = ForkPathStatus.Failed,
            State = null,
        };
    }

    /// <summary>
    /// Creates a recovered failure path result.
    /// </summary>
    /// <param name="pathIndex">The zero-based index of this path.</param>
    /// <param name="state">The recovered state from the failure handler.</param>
    /// <returns>A new recovered path result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pathIndex"/> is negative.</exception>
    /// <remarks>
    /// Recovery occurs when the failure handler completes without calling <c>Complete()</c>.
    /// The recovered state is preserved for merge in the join step.
    /// </remarks>
    public static ForkPathResult<TState> FailedWithRecovery(int pathIndex, TState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentOutOfRangeException.ThrowIfNegative(pathIndex, nameof(pathIndex));

        return new ForkPathResult<TState>
        {
            PathIndex = pathIndex,
            Status = ForkPathStatus.FailedWithRecovery,
            State = state,
        };
    }
}
