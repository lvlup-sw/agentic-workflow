// =============================================================================
// <copyright file="WorkflowExecutionBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Benchmarks.Fixtures;
using Agentic.Workflow.Builders;
using Agentic.Workflow.Definitions;
using Agentic.Workflow.Steps;

using BenchmarkDotNet.Attributes;

namespace Agentic.Workflow.Benchmarks.Integration;

/// <summary>
/// Benchmarks for end-to-end workflow execution performance.
/// </summary>
/// <remarks>
/// <para>
/// Measures full workflow execution including:
/// <list type="bullet">
///   <item><description>Workflow definition creation and validation</description></item>
///   <item><description>Step execution through multiple phases</description></item>
///   <item><description>State propagation between steps</description></item>
///   <item><description>Budget constraint checking</description></item>
/// </list>
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class WorkflowExecutionBenchmarks
{
    private WorkflowDefinition<BenchmarkWorkflowState> _simpleWorkflow = null!;
    private WorkflowDefinition<BenchmarkWorkflowState> _complexWorkflow = null!;
    private WorkflowDefinition<BenchmarkWorkflowState> _budgetWorkflow = null!;
    private BenchmarkWorkflowState _initialState = null!;

    /// <summary>
    /// Sets up test workflows and initial state before benchmarks run.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _initialState = new BenchmarkWorkflowState { WorkflowId = Guid.NewGuid() };

        // Simple 3-step workflow
        _simpleWorkflow = Workflow<BenchmarkWorkflowState>
            .Create("simple-workflow")
            .StartWith<NoOpStep>()
            .Then<NoOpStep>("step2")
            .Finally<NoOpStep>();

        // Complex 10-step workflow
        _complexWorkflow = Workflow<BenchmarkWorkflowState>
            .Create("complex-workflow")
            .StartWith<IncrementStep>()
            .Then<IncrementStep>("step2")
            .Then<IncrementStep>("step3")
            .Then<IncrementStep>("step4")
            .Then<IncrementStep>("step5")
            .Then<IncrementStep>("step6")
            .Then<IncrementStep>("step7")
            .Then<IncrementStep>("step8")
            .Then<IncrementStep>("step9")
            .Finally<IncrementStep>();

        // Workflow with budget constraints (same structure, tested with budget checking)
        _budgetWorkflow = Workflow<BenchmarkWorkflowState>
            .Create("budget-workflow")
            .StartWith<BudgetCheckStep>()
            .Then<BudgetCheckStep>("step2")
            .Then<BudgetCheckStep>("step3")
            .Finally<BudgetCheckStep>();
    }

    /// <summary>
    /// Benchmarks execution of a minimal 3-step workflow.
    /// </summary>
    /// <returns>The final workflow state after all steps.</returns>
    [Benchmark(Baseline = true)]
    public async Task<BenchmarkWorkflowState> ExecuteWorkflow_Simple_3Steps()
    {
        var state = _initialState with { WorkflowId = Guid.NewGuid(), Counter = 0 };
        var context = StepContext.Create(state.WorkflowId, "benchmark", "execute");

        // Execute each step in sequence
        foreach (var step in _simpleWorkflow.Steps)
        {
            var stepInstance = new NoOpStep();
            var result = await stepInstance.ExecuteAsync(state, context, CancellationToken.None);
            state = result.UpdatedState;
        }

        return state;
    }

    /// <summary>
    /// Benchmarks execution of a 10-step workflow with state accumulation.
    /// </summary>
    /// <returns>The final workflow state after all steps.</returns>
    [Benchmark]
    public async Task<BenchmarkWorkflowState> ExecuteWorkflow_Complex_10Steps()
    {
        var state = _initialState with { WorkflowId = Guid.NewGuid(), Counter = 0 };
        var context = StepContext.Create(state.WorkflowId, "benchmark", "execute");

        // Execute each step in sequence
        foreach (var step in _complexWorkflow.Steps)
        {
            var stepInstance = new IncrementStep();
            var result = await stepInstance.ExecuteAsync(state, context, CancellationToken.None);
            state = result.UpdatedState;
        }

        return state;
    }

    /// <summary>
    /// Benchmarks execution of a workflow with budget constraint checking.
    /// </summary>
    /// <returns>The final workflow state after all steps.</returns>
    [Benchmark]
    public async Task<BenchmarkWorkflowState> ExecuteWorkflow_WithBudget()
    {
        var state = _initialState with
        {
            WorkflowId = Guid.NewGuid(),
            Counter = 0,
            RemainingBudget = 1000.0,
        };
        var context = StepContext.Create(state.WorkflowId, "benchmark", "execute");

        // Execute each step with budget checking
        foreach (var step in _budgetWorkflow.Steps)
        {
            var stepInstance = new BudgetCheckStep();
            var result = await stepInstance.ExecuteAsync(state, context, CancellationToken.None);
            state = result.UpdatedState;
        }

        return state;
    }
}

/// <summary>
/// Benchmark workflow state for testing.
/// </summary>
public sealed record BenchmarkWorkflowState : IWorkflowState
{
    /// <inheritdoc/>
    public required Guid WorkflowId { get; init; }

    /// <summary>
    /// Gets the counter value for state accumulation tests.
    /// </summary>
    public int Counter { get; init; }

    /// <summary>
    /// Gets the remaining budget for budget constraint tests.
    /// </summary>
    public double RemainingBudget { get; init; } = 1000.0;
}

/// <summary>
/// A no-operation step for baseline benchmarks.
/// </summary>
public sealed class NoOpStep : IWorkflowStep<BenchmarkWorkflowState>
{
    /// <inheritdoc/>
    public Task<StepResult<BenchmarkWorkflowState>> ExecuteAsync(
        BenchmarkWorkflowState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(StepResult<BenchmarkWorkflowState>.FromState(state));
    }
}

/// <summary>
/// A step that increments a counter for state propagation tests.
/// </summary>
public sealed class IncrementStep : IWorkflowStep<BenchmarkWorkflowState>
{
    /// <inheritdoc/>
    public Task<StepResult<BenchmarkWorkflowState>> ExecuteAsync(
        BenchmarkWorkflowState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        var newState = state with { Counter = state.Counter + 1 };
        return Task.FromResult(StepResult<BenchmarkWorkflowState>.FromState(newState));
    }
}

/// <summary>
/// A step that checks and consumes budget for budget constraint tests.
/// </summary>
public sealed class BudgetCheckStep : IWorkflowStep<BenchmarkWorkflowState>
{
    private const double CostPerStep = 10.0;

    /// <inheritdoc/>
    public Task<StepResult<BenchmarkWorkflowState>> ExecuteAsync(
        BenchmarkWorkflowState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        // Check budget before proceeding
        if (state.RemainingBudget < CostPerStep)
        {
            return Task.FromResult(StepResult<BenchmarkWorkflowState>.FromState(state));
        }

        var newState = state with
        {
            Counter = state.Counter + 1,
            RemainingBudget = state.RemainingBudget - CostPerStep,
        };

        return Task.FromResult(StepResult<BenchmarkWorkflowState>.FromState(newState));
    }
}
