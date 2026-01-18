// =============================================================================
// <copyright file="MockPlanner.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text;

namespace AgenticCoder.Services;

/// <summary>
/// Mock implementation of planner for demonstration.
/// </summary>
public sealed class MockPlanner : IPlanner
{
    /// <inheritdoc/>
    public Task<string> CreatePlanAsync(
        string taskDescription,
        IReadOnlyList<string> requirements,
        CancellationToken cancellationToken = default)
    {
        var plan = new StringBuilder();
        plan.AppendLine("## Implementation Plan");
        plan.AppendLine();
        plan.AppendLine($"### Task: {taskDescription}");
        plan.AppendLine();
        plan.AppendLine("### Requirements:");

        foreach (var requirement in requirements)
        {
            plan.AppendLine($"- {requirement}");
        }

        plan.AppendLine();
        plan.AppendLine("### Steps:");
        plan.AppendLine("1. Create public static method with appropriate signature");
        plan.AppendLine("2. Implement core logic based on requirements");
        plan.AppendLine("3. Handle edge cases");
        plan.AppendLine("4. Add appropriate comments");

        return Task.FromResult(plan.ToString());
    }
}
