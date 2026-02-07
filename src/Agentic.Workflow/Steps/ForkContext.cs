// =============================================================================
// <copyright file="ForkContext.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Definitions;

namespace Agentic.Workflow.Steps;

/// <summary>
/// Immutable context for join steps containing results from all fork paths.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <remarks>
/// <para>
/// The fork context is provided to join steps and contains:
/// <list type="bullet">
///   <item><description>PathResults: Ordered list of results from each parallel path</description></item>
///   <item><description>AllSucceeded: Quick check if all paths completed successfully</description></item>
///   <item><description>SuccessfulStates: Enumerable of states available for merge</description></item>
/// </list>
/// </para>
/// <para>
/// Join steps use this context to implement custom merge logic:
/// <code>
/// public async Task&lt;StepResult&lt;OrderState&gt;&gt; ExecuteAsync(
///     OrderState state,
///     ForkContext&lt;OrderState&gt; context,
///     CancellationToken cancellationToken)
/// {
///     if (context.AllSucceeded)
///     {
///         // Merge states from all paths
///         foreach (var pathState in context.SuccessfulStates)
///         {
///             // Merge logic
///         }
///     }
///     return StepResult&lt;OrderState&gt;.FromState(state);
/// }
/// </code>
/// </para>
/// </remarks>
public sealed record ForkContext<TState>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Gets the results from all fork paths in index order.
    /// </summary>
    public required IReadOnlyList<ForkPathResult<TState>> PathResults { get; init; }

    /// <summary>
    /// Gets a value indicating whether all paths completed successfully.
    /// </summary>
    /// <remarks>
    /// Returns true when all paths have status <see cref="ForkPathStatus.Success"/>.
    /// Does not include <see cref="ForkPathStatus.FailedWithRecovery"/> paths.
    /// </remarks>
    public bool AllSucceeded => PathResults.All(r => r.Status == ForkPathStatus.Success);

    /// <summary>
    /// Gets a value indicating whether any path failed terminally.
    /// </summary>
    /// <remarks>
    /// Returns true when any path has status <see cref="ForkPathStatus.Failed"/>.
    /// </remarks>
    public bool AnyFailed => PathResults.Any(r => r.Status == ForkPathStatus.Failed);

    /// <summary>
    /// Gets a value indicating whether any path recovered from failure.
    /// </summary>
    /// <remarks>
    /// Returns true when any path has status <see cref="ForkPathStatus.FailedWithRecovery"/>.
    /// </remarks>
    public bool AnyRecovered => PathResults.Any(r => r.Status == ForkPathStatus.FailedWithRecovery);

    /// <summary>
    /// Gets the states from all successful paths (including recovered paths).
    /// </summary>
    /// <remarks>
    /// Includes states from paths with <see cref="ForkPathStatus.Success"/> and
    /// <see cref="ForkPathStatus.FailedWithRecovery"/> statuses.
    /// </remarks>
    public IEnumerable<TState> SuccessfulStates => PathResults
        .Where(r => r.IsSuccessful && r.State is not null)
        .Select(r => r.State!);

    /// <summary>
    /// Gets the path result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based path index.</param>
    /// <returns>The path result at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="index"/> is outside the valid range.
    /// </exception>
    public ForkPathResult<TState> this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, PathResults.Count, nameof(index));

            return PathResults[index];
        }
    }

    /// <summary>
    /// Creates a new fork context from path results.
    /// </summary>
    /// <param name="pathResults">The results from all fork paths.</param>
    /// <returns>A new fork context.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="pathResults"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="pathResults"/> is empty.
    /// </exception>
    public static ForkContext<TState> Create(IReadOnlyList<ForkPathResult<TState>> pathResults)
    {
        ArgumentNullException.ThrowIfNull(pathResults, nameof(pathResults));

        if (pathResults.Count == 0)
        {
            throw new ArgumentException("Fork context must have at least one path result.", nameof(pathResults));
        }

        return new ForkContext<TState>
        {
            PathResults = pathResults,
        };
    }
}