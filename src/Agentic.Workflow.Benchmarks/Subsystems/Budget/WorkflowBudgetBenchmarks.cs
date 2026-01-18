// =============================================================================
// <copyright file="WorkflowBudgetBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Infrastructure.Budget;
using Agentic.Workflow.Orchestration.Budget;
using BenchmarkDotNet.Attributes;

namespace Agentic.Workflow.Benchmarks.Subsystems.Budget;

/// <summary>
/// Benchmarks for <see cref="WorkflowBudget"/> scarcity computation and state updates.
/// </summary>
/// <remarks>
/// <para>
/// These benchmarks focus on:
/// <list type="bullet">
///   <item><description>Scarcity level computation (lazy caching validation)</description></item>
///   <item><description>Dictionary copy cost during consumption updates</description></item>
/// </list>
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class WorkflowBudgetBenchmarks
{
    private WorkflowBudget _budget = null!;
    private WorkflowBudget _budgetForCachedAccess = null!;
    private ScarcityLevel _cachedScarcity;

    /// <summary>
    /// Sets up the benchmark with a fresh workflow budget for each iteration.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _budget = WorkflowBudget.Create(
            workflowId: "benchmark-workflow",
            steps: 25,
            tokens: 50000,
            executions: 15,
            toolCalls: 40,
            wallTimeSeconds: 300);

        _budgetForCachedAccess = WorkflowBudget.Create(
            workflowId: "benchmark-workflow-cached",
            steps: 25,
            tokens: 50000,
            executions: 15,
            toolCalls: 40,
            wallTimeSeconds: 300);

        // Pre-warm the cached access budget by reading scarcity once
        _cachedScarcity = _budgetForCachedAccess.OverallScarcity;
    }

    /// <summary>
    /// Benchmarks the first access to OverallScarcity which computes the max across all resources.
    /// </summary>
    /// <returns>The computed scarcity level.</returns>
    /// <remarks>
    /// This measures the cost of iterating through all resource budgets
    /// and computing the maximum scarcity level.
    /// </remarks>
    [Benchmark(Description = "OverallScarcity - First Access (compute)")]
    public ScarcityLevel OverallScarcity_FirstAccess()
    {
        // Create fresh budget each time to ensure first access
        var freshBudget = WorkflowBudget.Create(
            workflowId: "fresh-workflow",
            steps: 25,
            tokens: 50000,
            executions: 15,
            toolCalls: 40,
            wallTimeSeconds: 300);

        return freshBudget.OverallScarcity;
    }

    /// <summary>
    /// Benchmarks cached access to OverallScarcity on the same budget instance.
    /// </summary>
    /// <returns>The cached scarcity level.</returns>
    /// <remarks>
    /// <para>
    /// This measures repeated reads of OverallScarcity on the same record instance.
    /// Since WorkflowBudget is a record with a computed property, each access
    /// re-computes the value. This benchmark validates the computation cost
    /// for repeated access patterns.
    /// </para>
    /// </remarks>
    [Benchmark(Description = "OverallScarcity - Repeated Access")]
    public ScarcityLevel OverallScarcity_CachedAccess()
    {
        return _budgetForCachedAccess.OverallScarcity;
    }

    /// <summary>
    /// Benchmarks the cost of WithConsumption which creates a new dictionary copy.
    /// </summary>
    /// <returns>The updated workflow budget.</returns>
    /// <remarks>
    /// <para>
    /// WithConsumption creates a new dictionary with the updated resource budget.
    /// This benchmark measures the allocation and copy overhead.
    /// </para>
    /// </remarks>
    [Benchmark(Description = "WithConsumption - Dictionary Copy")]
    public IWorkflowBudget WithConsumption_DictionaryCopy()
    {
        return _budget.WithConsumption(ResourceType.Tokens, 100);
    }
}
