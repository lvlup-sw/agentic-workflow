// =============================================================================
// <copyright file="IPlanner.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace AgenticCoder.Services;

/// <summary>
/// Contract for generating implementation plans.
/// </summary>
public interface IPlanner
{
    /// <summary>
    /// Creates an implementation plan for a coding task.
    /// </summary>
    /// <param name="taskDescription">The task to plan.</param>
    /// <param name="requirements">Extracted requirements from analysis.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Implementation plan as a structured string.</returns>
    Task<string> CreatePlanAsync(
        string taskDescription,
        IReadOnlyList<string> requirements,
        CancellationToken cancellationToken = default);
}
