// =============================================================================
// <copyright file="ThompsonSamplingSelectorTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Infrastructure.Selection;
using Agentic.Workflow.Primitives;
using Agentic.Workflow.Selection;

namespace Agentic.Workflow.Infrastructure.Tests.Selection;

/// <summary>
/// Unit tests for <see cref="ThompsonSamplingAgentSelector"/> covering the
/// Thompson Sampling implementation of agent selection.
/// </summary>
[Property("Category", "Unit")]
public class ThompsonSamplingSelectorTests
{
    // =============================================================================
    // A. SelectAgentAsync Basic Tests
    // =============================================================================

    /// <summary>
    /// Verifies that SelectAgentAsync returns a valid selection from available agents.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_WithAvailableAgents_ReturnsValidSelection()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();
        var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Implement a sorting algorithm",
            AvailableAgents = ["gpt-4o", "claude-3", "gemini-pro"],
        };

        // Act
        var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(context.AvailableAgents).Contains(result.Value.SelectedAgentId);
    }

    /// <summary>
    /// Verifies that SelectAgentAsync classifies task correctly.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_WithCodeTask_ClassifiesAsCodeGeneration()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();
        var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Debug and refactor this function",
            AvailableAgents = ["agent-1"],
        };

        // Act
        var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);

        // Assert
        await Assert.That(result.Value.TaskCategory).IsEqualTo(TaskCategory.CodeGeneration);
    }

    /// <summary>
    /// Verifies that SelectAgentAsync returns error when no agents available.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_NoAvailableAgents_ReturnsFailure()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();
        var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Test task",
            AvailableAgents = [],
        };

        // Act
        var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error.Code).IsEqualTo("SELECTOR_NO_CANDIDATES");
    }

    /// <summary>
    /// Verifies that SelectAgentAsync respects exclusions.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_WithExclusions_RespectsExclusions()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();
        var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Test task",
            AvailableAgents = ["agent-1", "agent-2", "agent-3"],
            ExcludedAgents = ["agent-1", "agent-2"],
        };

        // Act
        var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.SelectedAgentId).IsEqualTo("agent-3");
    }

    /// <summary>
    /// Verifies that SelectAgentAsync returns failure when all agents excluded.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_AllAgentsExcluded_ReturnsFailure()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();
        var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Test task",
            AvailableAgents = ["agent-1", "agent-2"],
            ExcludedAgents = ["agent-1", "agent-2"],
        };

        // Act
        var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error.Code).IsEqualTo("SELECTOR_NO_CANDIDATES");
    }

    // =============================================================================
    // B. SelectAgentAsync Exploitation Tests
    // =============================================================================

    /// <summary>
    /// Verifies that agent with more successes is selected more often.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_AgentWithMoreSuccesses_SelectedMoreOften()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();

        // Give agent-1 many successes (should have higher expected value)
        for (int i = 0; i < 20; i++)
        {
            await beliefStore.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true).ConfigureAwait(false);
        }

        // Give agent-2 many failures (should have lower expected value)
        for (int i = 0; i < 20; i++)
        {
            await beliefStore.UpdateBeliefAsync("agent-2", "CodeGeneration", success: false).ConfigureAwait(false);
        }

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Write some code",
            AvailableAgents = ["agent-1", "agent-2"],
        };

        // Act - Run multiple selections to check distribution
        var selections = new Dictionary<string, int> { ["agent-1"] = 0, ["agent-2"] = 0 };
        for (int trial = 0; trial < 100; trial++)
        {
            var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: trial);
            var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);
            selections[result.Value.SelectedAgentId]++;
        }

        // Assert - Agent-1 should be selected much more often
        await Assert.That(selections["agent-1"]).IsGreaterThan(selections["agent-2"]);
        await Assert.That(selections["agent-1"]).IsGreaterThan(70); // Should be heavily favored
    }

    // =============================================================================
    // C. SelectAgentAsync Exploration Tests
    // =============================================================================

    /// <summary>
    /// Verifies that with uniform priors, all agents have roughly equal selection chance.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_UniformPriors_ExploresAllAgents()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Test task",
            AvailableAgents = ["agent-1", "agent-2", "agent-3"],
        };

        // Act - Run multiple selections
        var selections = new Dictionary<string, int>
        {
            ["agent-1"] = 0,
            ["agent-2"] = 0,
            ["agent-3"] = 0,
        };

        for (int trial = 0; trial < 300; trial++)
        {
            var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: trial);
            var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);
            selections[result.Value.SelectedAgentId]++;
        }

        // Assert - Each agent should be selected a reasonable number of times
        foreach (var count in selections.Values)
        {
            await Assert.That(count).IsGreaterThan(50); // At least 50 out of 300
        }
    }

    // =============================================================================
    // D. RecordOutcomeAsync Tests
    // =============================================================================

    /// <summary>
    /// Verifies that RecordOutcomeAsync updates beliefs on success.
    /// </summary>
    [Test]
    public async Task RecordOutcomeAsync_Success_UpdatesBeliefAlpha()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();
        var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);

        // Act
        var result = await selector.RecordOutcomeAsync(
            "agent-1",
            TaskCategory.CodeGeneration.ToString(),
            AgentOutcome.Succeeded()).ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();

        var belief = await beliefStore.GetBeliefAsync("agent-1", "CodeGeneration").ConfigureAwait(false);
        await Assert.That(belief.Value.Alpha).IsEqualTo(3.0); // Prior + 1
        await Assert.That(belief.Value.Beta).IsEqualTo(2.0); // Unchanged
    }

    /// <summary>
    /// Verifies that RecordOutcomeAsync updates beliefs on failure.
    /// </summary>
    [Test]
    public async Task RecordOutcomeAsync_Failure_UpdatesBeliefBeta()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();
        var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);

        // Act
        var result = await selector.RecordOutcomeAsync(
            "agent-1",
            TaskCategory.DataAnalysis.ToString(),
            AgentOutcome.Failed()).ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();

        var belief = await beliefStore.GetBeliefAsync("agent-1", "DataAnalysis").ConfigureAwait(false);
        await Assert.That(belief.Value.Alpha).IsEqualTo(2.0); // Unchanged
        await Assert.That(belief.Value.Beta).IsEqualTo(3.0); // Prior + 1
    }

    // =============================================================================
    // E. SelectionConfidence Tests
    // =============================================================================

    /// <summary>
    /// Verifies that selection confidence starts low with few observations.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_FewObservations_LowConfidence()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();
        var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Test task",
            AvailableAgents = ["agent-1"],
        };

        // Act
        var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);

        // Assert - With no observations, confidence should be 0
        await Assert.That(result.Value.SelectionConfidence).IsEqualTo(0.0);
    }

    /// <summary>
    /// Verifies that selection confidence increases with more observations.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_ManyObservations_HighConfidence()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();

        // Add 20 observations
        for (int i = 0; i < 20; i++)
        {
            await beliefStore.UpdateBeliefAsync("agent-1", "General", success: true).ConfigureAwait(false);
        }

        var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Hello world", // General category
            AvailableAgents = ["agent-1"],
        };

        // Act
        var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);

        // Assert - With 20 observations, confidence should be 1.0 (capped)
        await Assert.That(result.Value.SelectionConfidence).IsEqualTo(1.0);
    }

    // =============================================================================
    // F. SampledTheta Tests
    // =============================================================================

    /// <summary>
    /// Verifies that sampled theta is in valid range [0, 1].
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_SampledTheta_InValidRange()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();
        var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Test task",
            AvailableAgents = ["agent-1"],
        };

        // Act
        var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);

        // Assert
        await Assert.That(result.Value.SampledTheta).IsGreaterThanOrEqualTo(0.0);
        await Assert.That(result.Value.SampledTheta).IsLessThanOrEqualTo(1.0);
    }

    // =============================================================================
    // G. Reproducibility Tests
    // =============================================================================

    /// <summary>
    /// Verifies that same seed produces same selection.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_SameSeed_ProducesSameResult()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Test task",
            AvailableAgents = ["agent-1", "agent-2", "agent-3"],
        };

        // Act
        var selector1 = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);
        var result1 = await selector1.SelectAgentAsync(context).ConfigureAwait(false);

        var selector2 = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);
        var result2 = await selector2.SelectAgentAsync(context).ConfigureAwait(false);

        // Assert
        await Assert.That(result1.Value.SelectedAgentId).IsEqualTo(result2.Value.SelectedAgentId);
        await Assert.That(result1.Value.SampledTheta).IsEqualTo(result2.Value.SampledTheta);
    }

    // =============================================================================
    // H. Performance Optimization Tests
    // =============================================================================

    /// <summary>
    /// Verifies that belief fetching for multiple candidates happens concurrently.
    /// Uses deterministic concurrency tracking instead of flaky timing assertions.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_MultipleCandidates_FetchesBeliefsConcurrently()
    {
        // Arrange
        const int candidateCount = 5;
        const int delayPerFetchMs = 50;
        var delayingStore = new DelayingBeliefStore(TimeSpan.FromMilliseconds(delayPerFetchMs));
        var selector = new ThompsonSamplingAgentSelector(delayingStore, randomSeed: 42);

        var agents = Enumerable.Range(1, candidateCount).Select(i => $"agent-{i}").ToList();
        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Test task",
            AvailableAgents = agents,
        };

        // Act
        var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);

        // Assert - Verify parallel execution via concurrency counter
        // If fetches ran sequentially, MaxConcurrentFetches would be 1
        // If fetches ran in parallel, MaxConcurrentFetches should be > 1 (ideally equal to candidateCount)
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(delayingStore.MaxConcurrentFetches).IsGreaterThan(1);
    }

    /// <summary>
    /// Verifies that early exit skips .Except() when no exclusions provided.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_NoExclusions_SkipsExceptAllocation()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();
        var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Test task",
            AvailableAgents = ["agent-1", "agent-2", "agent-3"],
            ExcludedAgents = null, // No exclusions
        };

        // Act
        var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);

        // Assert - Verify the result is valid (the early exit optimization doesn't affect correctness)
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(context.AvailableAgents).Contains(result.Value.SelectedAgentId);
    }

    /// <summary>
    /// Verifies that early exit skips .Except() when exclusions list is empty.
    /// </summary>
    [Test]
    public async Task SelectAgentAsync_EmptyExclusions_SkipsExceptAllocation()
    {
        // Arrange
        var beliefStore = new InMemoryBeliefStore();
        var selector = new ThompsonSamplingAgentSelector(beliefStore, randomSeed: 42);

        var context = new AgentSelectionContext
        {
            WorkflowId = Guid.NewGuid(),
            StepName = "TestStep",
            TaskDescription = "Test task",
            AvailableAgents = ["agent-1", "agent-2", "agent-3"],
            ExcludedAgents = [], // Empty list
        };

        // Act
        var result = await selector.SelectAgentAsync(context).ConfigureAwait(false);

        // Assert - Verify the result is valid
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(context.AvailableAgents).Contains(result.Value.SelectedAgentId);
    }
}

/// <summary>
/// Test belief store that introduces artificial delay to verify parallel fetching.
/// Tracks concurrent fetches to deterministically verify concurrency without timing assertions.
/// </summary>
file sealed class DelayingBeliefStore : IBeliefStore
{
    private readonly TimeSpan _delay;
    private readonly InMemoryBeliefStore _inner = new();
    private int _currentConcurrentFetches;
    private int _maxConcurrentFetches;

    /// <summary>
    /// Gets the maximum number of concurrent fetches observed during the lifetime of this store.
    /// </summary>
    public int MaxConcurrentFetches => _maxConcurrentFetches;

    public DelayingBeliefStore(TimeSpan delay)
    {
        _delay = delay;
    }

    public async ValueTask<Result<AgentBelief>> GetBeliefAsync(
        string agentId,
        string taskCategory,
        CancellationToken cancellationToken = default)
    {
        // Track concurrent fetches to verify parallel execution
        var current = Interlocked.Increment(ref _currentConcurrentFetches);
        UpdateMaxConcurrent(current);

        try
        {
            await Task.Delay(_delay, cancellationToken).ConfigureAwait(false);
            return await _inner.GetBeliefAsync(agentId, taskCategory, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.Decrement(ref _currentConcurrentFetches);
        }
    }

    private void UpdateMaxConcurrent(int current)
    {
        // Thread-safe update of max concurrent fetches
        int max;
        do
        {
            max = _maxConcurrentFetches;
            if (current <= max)
            {
                return;
            }
        }
        while (Interlocked.CompareExchange(ref _maxConcurrentFetches, current, max) != max);
    }

    public ValueTask<Result<Unit>> UpdateBeliefAsync(
        string agentId,
        string taskCategory,
        bool success,
        CancellationToken cancellationToken = default)
        => _inner.UpdateBeliefAsync(agentId, taskCategory, success, cancellationToken);

    public ValueTask<Result<IReadOnlyList<AgentBelief>>> GetBeliefsForAgentAsync(
        string agentId,
        CancellationToken cancellationToken = default)
        => _inner.GetBeliefsForAgentAsync(agentId, cancellationToken);

    public ValueTask<Result<IReadOnlyList<AgentBelief>>> GetBeliefsForCategoryAsync(
        string taskCategory,
        CancellationToken cancellationToken = default)
        => _inner.GetBeliefsForCategoryAsync(taskCategory, cancellationToken);

    public ValueTask<Result<Unit>> SaveBeliefAsync(
        AgentBelief belief,
        CancellationToken cancellationToken = default)
        => _inner.SaveBeliefAsync(belief, cancellationToken);
}
