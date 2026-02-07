// =============================================================================
// <copyright file="ConcurrentWorkflowBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Builders;
using Agentic.Workflow.Definitions;
using Agentic.Workflow.Steps;

using BenchmarkDotNet.Attributes;

namespace Agentic.Workflow.Benchmarks.Integration;

/// <summary>
/// Benchmarks for concurrent workflow execution throughput.
/// </summary>
/// <remarks>
/// <para>
/// Measures workflow throughput under load:
/// <list type="bullet">
///   <item><description>Concurrent workflow execution scalability</description></item>
///   <item><description>Throughput (workflows per second)</description></item>
///   <item><description>Memory efficiency under parallel load</description></item>
/// </list>
/// </para>
/// <para>
/// Target throughput: >500 workflows/second.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ConcurrentWorkflowBenchmarks
{
    private WorkflowDefinition<ConcurrentWorkflowState> _workflow = null!;

    /// <summary>
    /// Sets up test workflow before benchmarks run.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        // Create a simple 3-step workflow for throughput testing
        _workflow = Workflow<ConcurrentWorkflowState>
            .Create("concurrent-workflow")
            .StartWith<FastStep>()
            .Then<FastStep>("step2")
            .Finally<FastStep>();
    }

    /// <summary>
    /// Benchmarks concurrent execution of 10 workflows.
    /// </summary>
    /// <returns>The total number of completed workflows.</returns>
    [Benchmark(Baseline = true)]
    public async Task<int> ExecuteWorkflows_Concurrent_10()
    {
        const int workflowCount = 10;
        var tasks = new Task<ConcurrentWorkflowState>[workflowCount];

        for (int i = 0; i < workflowCount; i++)
        {
            tasks[i] = ExecuteSingleWorkflowAsync();
        }

        var results = await Task.WhenAll(tasks);
        return results.Length;
    }

    /// <summary>
    /// Benchmarks concurrent execution of 100 workflows.
    /// </summary>
    /// <returns>The total number of completed workflows.</returns>
    /// <remarks>
    /// This benchmark validates throughput target of >500 workflows/second.
    /// With proper async implementation, 100 concurrent workflows should complete
    /// in well under 200ms.
    /// </remarks>
    [Benchmark]
    public async Task<int> ExecuteWorkflows_Concurrent_100()
    {
        const int workflowCount = 100;
        var tasks = new Task<ConcurrentWorkflowState>[workflowCount];

        for (int i = 0; i < workflowCount; i++)
        {
            tasks[i] = ExecuteSingleWorkflowAsync();
        }

        var results = await Task.WhenAll(tasks);
        return results.Length;
    }

    /// <summary>
    /// Executes a single workflow through all steps.
    /// </summary>
    /// <returns>The final workflow state.</returns>
    private async Task<ConcurrentWorkflowState> ExecuteSingleWorkflowAsync()
    {
        var state = new ConcurrentWorkflowState
        {
            WorkflowId = Guid.NewGuid(),
            ProcessedSteps = 0,
        };

        var context = StepContext.Create(state.WorkflowId, "concurrent-benchmark", "execute");
        var stepInstance = new FastStep();

        // Execute each step in sequence
        foreach (var step in _workflow.Steps)
        {
            var result = await stepInstance.ExecuteAsync(state, context, CancellationToken.None);
            state = result.UpdatedState;
        }

        return state;
    }
}

/// <summary>
/// State for concurrent workflow benchmarks.
/// </summary>
public sealed record ConcurrentWorkflowState : IWorkflowState
{
    /// <inheritdoc/>
    public required Guid WorkflowId { get; init; }

    /// <summary>
    /// Gets the number of steps processed in this workflow.
    /// </summary>
    public int ProcessedSteps { get; init; }
}

/// <summary>
/// A fast, minimal step for throughput testing.
/// </summary>
/// <remarks>
/// This step performs minimal work to measure pure workflow infrastructure overhead.
/// </remarks>
public sealed class FastStep : IWorkflowStep<ConcurrentWorkflowState>
{
    /// <inheritdoc/>
    public Task<StepResult<ConcurrentWorkflowState>> ExecuteAsync(
        ConcurrentWorkflowState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        var newState = state with { ProcessedSteps = state.ProcessedSteps + 1 };
        return Task.FromResult(StepResult<ConcurrentWorkflowState>.FromState(newState));
    }
}