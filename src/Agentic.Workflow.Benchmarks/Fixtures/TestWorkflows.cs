// =============================================================================
// <copyright file="TestWorkflows.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Benchmarks.Fixtures;

/// <summary>
/// Provides test data generators for workflow-related benchmarks.
/// </summary>
/// <remarks>
/// <para>
/// All methods in this class are designed to be deterministic and efficient,
/// suitable for use in benchmark setup phases.
/// </para>
/// </remarks>
public static class TestWorkflows
{
    /// <summary>
    /// Creates a list of step names for a workflow with the specified number of steps.
    /// </summary>
    /// <param name="count">The number of steps to create.</param>
    /// <returns>A read-only list of step names.</returns>
    public static IReadOnlyList<string> CreateStepNames(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => $"Step{i}")
            .ToList();
    }

    /// <summary>
    /// Creates a dictionary of workflow metadata suitable for benchmark tests.
    /// </summary>
    /// <param name="workflowId">
    /// The workflow identifier. Defaults to <see cref="Guid.Empty"/> for deterministic output.
    /// </param>
    /// <param name="startTime">
    /// The workflow start time. Defaults to <see cref="DateTimeOffset.UnixEpoch"/> for deterministic output.
    /// </param>
    /// <returns>A read-only dictionary containing workflow metadata.</returns>
    public static IReadOnlyDictionary<string, object> CreateWorkflowMetadata(
        Guid? workflowId = null,
        DateTimeOffset? startTime = null)
    {
        return new Dictionary<string, object>
        {
            ["workflowId"] = (workflowId ?? Guid.Empty).ToString(),
            ["startTime"] = startTime ?? DateTimeOffset.UnixEpoch,
            ["version"] = "1.0.0",
        };
    }
}