// =============================================================================
// <copyright file="InMemoryBeliefStoreTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Infrastructure.Selection;
using Agentic.Workflow.Selection;

namespace Agentic.Workflow.Infrastructure.Tests.Selection;

/// <summary>
/// Unit tests for <see cref="InMemoryBeliefStore"/> covering the in-memory
/// implementation of belief persistence for Thompson Sampling.
/// </summary>
[Property("Category", "Unit")]
public class InMemoryBeliefStoreTests
{
    // =============================================================================
    // A. GetBeliefAsync Tests
    // =============================================================================

    /// <summary>
    /// Verifies that GetBeliefAsync returns a default prior for unknown agent/category.
    /// </summary>
    [Test]
    public async Task GetBeliefAsync_NoExistingBelief_ReturnsPrior()
    {
        // Arrange
        var store = new InMemoryBeliefStore();

        // Act
        var result = await store.GetBeliefAsync("agent-1", "CodeGeneration").ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.AgentId).IsEqualTo("agent-1");
        await Assert.That(result.Value.TaskCategory).IsEqualTo("CodeGeneration");
        await Assert.That(result.Value.Alpha).IsEqualTo(AgentBelief.DefaultPriorAlpha);
        await Assert.That(result.Value.Beta).IsEqualTo(AgentBelief.DefaultPriorBeta);
        await Assert.That(result.Value.ObservationCount).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that GetBeliefAsync returns stored belief after update.
    /// </summary>
    [Test]
    public async Task GetBeliefAsync_AfterUpdate_ReturnsUpdatedBelief()
    {
        // Arrange
        var store = new InMemoryBeliefStore();
        await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true).ConfigureAwait(false);

        // Act
        var result = await store.GetBeliefAsync("agent-1", "CodeGeneration").ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Alpha).IsEqualTo(3.0); // Prior(2) + Success(1)
        await Assert.That(result.Value.Beta).IsEqualTo(2.0); // Prior(2)
        await Assert.That(result.Value.ObservationCount).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that different agent/category pairs are stored separately.
    /// </summary>
    [Test]
    public async Task GetBeliefAsync_DifferentKeys_ReturnsSeparateBeliefs()
    {
        // Arrange
        var store = new InMemoryBeliefStore();
        await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-1", "DataAnalysis", success: false).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-2", "CodeGeneration", success: true).ConfigureAwait(false);

        // Act
        var belief1Code = await store.GetBeliefAsync("agent-1", "CodeGeneration").ConfigureAwait(false);
        var belief1Data = await store.GetBeliefAsync("agent-1", "DataAnalysis").ConfigureAwait(false);
        var belief2Code = await store.GetBeliefAsync("agent-2", "CodeGeneration").ConfigureAwait(false);

        // Assert
        await Assert.That(belief1Code.Value.Alpha).IsEqualTo(3.0);
        await Assert.That(belief1Code.Value.Beta).IsEqualTo(2.0);

        await Assert.That(belief1Data.Value.Alpha).IsEqualTo(2.0);
        await Assert.That(belief1Data.Value.Beta).IsEqualTo(3.0);

        await Assert.That(belief2Code.Value.Alpha).IsEqualTo(3.0);
        await Assert.That(belief2Code.Value.Beta).IsEqualTo(2.0);
    }

    // =============================================================================
    // B. UpdateBeliefAsync Tests
    // =============================================================================

    /// <summary>
    /// Verifies that UpdateBeliefAsync increments Alpha on success.
    /// </summary>
    [Test]
    public async Task UpdateBeliefAsync_Success_IncrementsAlpha()
    {
        // Arrange
        var store = new InMemoryBeliefStore();

        // Act
        var result = await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true).ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();

        var belief = await store.GetBeliefAsync("agent-1", "CodeGeneration").ConfigureAwait(false);
        await Assert.That(belief.Value.Alpha).IsEqualTo(3.0); // 2 + 1
        await Assert.That(belief.Value.Beta).IsEqualTo(2.0); // unchanged
    }

    /// <summary>
    /// Verifies that UpdateBeliefAsync increments Beta on failure.
    /// </summary>
    [Test]
    public async Task UpdateBeliefAsync_Failure_IncrementsBeta()
    {
        // Arrange
        var store = new InMemoryBeliefStore();

        // Act
        var result = await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: false).ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();

        var belief = await store.GetBeliefAsync("agent-1", "CodeGeneration").ConfigureAwait(false);
        await Assert.That(belief.Value.Alpha).IsEqualTo(2.0); // unchanged
        await Assert.That(belief.Value.Beta).IsEqualTo(3.0); // 2 + 1
    }

    /// <summary>
    /// Verifies that multiple updates accumulate correctly.
    /// </summary>
    [Test]
    public async Task UpdateBeliefAsync_MultipleUpdates_AccumulatesCorrectly()
    {
        // Arrange
        var store = new InMemoryBeliefStore();

        // Act - 3 successes, 2 failures
        await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: false).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: false).ConfigureAwait(false);

        // Assert
        var belief = await store.GetBeliefAsync("agent-1", "CodeGeneration").ConfigureAwait(false);
        await Assert.That(belief.Value.Alpha).IsEqualTo(5.0); // 2 + 3
        await Assert.That(belief.Value.Beta).IsEqualTo(4.0); // 2 + 2
        await Assert.That(belief.Value.ObservationCount).IsEqualTo(5);
    }

    // =============================================================================
    // C. GetBeliefsForAgentAsync Tests
    // =============================================================================

    /// <summary>
    /// Verifies that GetBeliefsForAgentAsync returns empty list for unknown agent.
    /// </summary>
    [Test]
    public async Task GetBeliefsForAgentAsync_NoBeliefs_ReturnsEmptyList()
    {
        // Arrange
        var store = new InMemoryBeliefStore();

        // Act
        var result = await store.GetBeliefsForAgentAsync("unknown-agent").ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).IsEmpty();
    }

    /// <summary>
    /// Verifies that GetBeliefsForAgentAsync returns all beliefs for an agent.
    /// </summary>
    [Test]
    public async Task GetBeliefsForAgentAsync_MultipleBeliefsExist_ReturnsAllForAgent()
    {
        // Arrange
        var store = new InMemoryBeliefStore();
        await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-1", "DataAnalysis", success: false).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-1", "WebSearch", success: true).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-2", "CodeGeneration", success: true).ConfigureAwait(false);

        // Act
        var result = await store.GetBeliefsForAgentAsync("agent-1").ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Count).IsEqualTo(3);
        await Assert.That(result.Value.All(b => b.AgentId == "agent-1")).IsTrue();
    }

    /// <summary>
    /// Verifies that GetBeliefsForAgentAsync excludes other agents' beliefs.
    /// </summary>
    [Test]
    public async Task GetBeliefsForAgentAsync_MultipleAgents_ExcludesOtherAgents()
    {
        // Arrange
        var store = new InMemoryBeliefStore();
        await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-2", "CodeGeneration", success: true).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-2", "DataAnalysis", success: true).ConfigureAwait(false);

        // Act
        var result = await store.GetBeliefsForAgentAsync("agent-2").ConfigureAwait(false);

        // Assert
        await Assert.That(result.Value.Count).IsEqualTo(2);
        await Assert.That(result.Value.Any(b => b.AgentId == "agent-1")).IsFalse();
    }

    // =============================================================================
    // D. GetBeliefsForCategoryAsync Tests
    // =============================================================================

    /// <summary>
    /// Verifies that GetBeliefsForCategoryAsync returns empty list for unknown category.
    /// </summary>
    [Test]
    public async Task GetBeliefsForCategoryAsync_NoBeliefs_ReturnsEmptyList()
    {
        // Arrange
        var store = new InMemoryBeliefStore();

        // Act
        var result = await store.GetBeliefsForCategoryAsync("UnknownCategory").ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).IsEmpty();
    }

    /// <summary>
    /// Verifies that GetBeliefsForCategoryAsync returns all beliefs for a category.
    /// </summary>
    [Test]
    public async Task GetBeliefsForCategoryAsync_MultipleBeliefsExist_ReturnsAllForCategory()
    {
        // Arrange
        var store = new InMemoryBeliefStore();
        await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-2", "CodeGeneration", success: false).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-3", "CodeGeneration", success: true).ConfigureAwait(false);
        await store.UpdateBeliefAsync("agent-1", "DataAnalysis", success: true).ConfigureAwait(false);

        // Act
        var result = await store.GetBeliefsForCategoryAsync("CodeGeneration").ConfigureAwait(false);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Count).IsEqualTo(3);
        await Assert.That(result.Value.All(b => b.TaskCategory == "CodeGeneration")).IsTrue();
    }

    // =============================================================================
    // E. Thread Safety Tests
    // =============================================================================

    /// <summary>
    /// Verifies that concurrent updates are thread-safe.
    /// </summary>
    [Test]
    public async Task ConcurrentUpdates_ThreadSafe()
    {
        // Arrange
        var store = new InMemoryBeliefStore();
        const int iterationCount = 100;

        // Act - Run concurrent updates
        var tasks = new List<Task>();
        for (int i = 0; i < iterationCount; i++)
        {
            tasks.Add(store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true));
            tasks.Add(store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: false));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Assert - All updates should be recorded
        var belief = await store.GetBeliefAsync("agent-1", "CodeGeneration").ConfigureAwait(false);
        await Assert.That(belief.Value.ObservationCount).IsEqualTo(iterationCount * 2);
        await Assert.That(belief.Value.Alpha).IsEqualTo(2.0 + iterationCount); // Prior + successes
        await Assert.That(belief.Value.Beta).IsEqualTo(2.0 + iterationCount); // Prior + failures
    }

    /// <summary>
    /// Verifies that concurrent reads and writes are thread-safe.
    /// </summary>
    [Test]
    public async Task ConcurrentReadsAndWrites_ThreadSafe()
    {
        // Arrange
        var store = new InMemoryBeliefStore();
        const int iterationCount = 50;

        // Act - Concurrent reads and writes
        var tasks = new List<Task>();
        for (int i = 0; i < iterationCount; i++)
        {
            tasks.Add(store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true));
            tasks.Add(store.GetBeliefAsync("agent-1", "CodeGeneration"));
            tasks.Add(store.GetBeliefsForAgentAsync("agent-1"));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Assert - Should complete without exceptions
        var belief = await store.GetBeliefAsync("agent-1", "CodeGeneration").ConfigureAwait(false);
        await Assert.That(belief.IsSuccess).IsTrue();
        await Assert.That(belief.Value.ObservationCount).IsEqualTo(iterationCount);
    }

    // =============================================================================
    // F. Performance Optimization Tests (Secondary Indices)
    // =============================================================================

    /// <summary>
    /// Verifies that GetBeliefsForAgentAsync returns in near-constant time with many beliefs.
    /// </summary>
    [Test]
    public async Task GetBeliefsForAgentAsync_ManyBeliefs_ReturnsInConstantTime()
    {
        // Arrange
        var store = new InMemoryBeliefStore();
        const int agentCount = 100;
        const int categoriesPerAgent = 100;

        // Populate store with 10,000 beliefs (100 agents x 100 categories)
        for (int a = 0; a < agentCount; a++)
        {
            for (int c = 0; c < categoriesPerAgent; c++)
            {
                await store.UpdateBeliefAsync($"agent-{a}", $"category-{c}", success: true).ConfigureAwait(false);
            }
        }

        // Act - Measure lookup time for a specific agent
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await store.GetBeliefsForAgentAsync("agent-50").ConfigureAwait(false);
        stopwatch.Stop();

        // Assert
        // With O(1) index lookup, should be < 5ms even with 10K total beliefs
        // Without index (O(n) scan), could be much slower
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Count).IsEqualTo(categoriesPerAgent);
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(5);
    }

    /// <summary>
    /// Verifies that GetBeliefsForCategoryAsync returns in near-constant time with many beliefs.
    /// </summary>
    [Test]
    public async Task GetBeliefsForCategoryAsync_ManyBeliefs_ReturnsInConstantTime()
    {
        // Arrange
        var store = new InMemoryBeliefStore();
        const int agentCount = 100;
        const int categoriesPerAgent = 100;

        // Populate store with 10,000 beliefs (100 agents x 100 categories)
        for (int a = 0; a < agentCount; a++)
        {
            for (int c = 0; c < categoriesPerAgent; c++)
            {
                await store.UpdateBeliefAsync($"agent-{a}", $"category-{c}", success: true).ConfigureAwait(false);
            }
        }

        // Act - Measure lookup time for a specific category
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await store.GetBeliefsForCategoryAsync("category-50").ConfigureAwait(false);
        stopwatch.Stop();

        // Assert
        // With O(1) index lookup, should be < 5ms even with 10K total beliefs
        // Without index (O(n) scan), could be much slower
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Count).IsEqualTo(agentCount);
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(5);
    }

    /// <summary>
    /// Verifies that SaveBeliefAsync maintains secondary indices.
    /// </summary>
    [Test]
    public async Task SaveBeliefAsync_MaintainsIndices()
    {
        // Arrange
        var store = new InMemoryBeliefStore();
        var belief = AgentBelief.CreatePrior("agent-1", "CodeGeneration").WithSuccess();

        // Act
        await store.SaveBeliefAsync(belief).ConfigureAwait(false);

        // Assert - Verify the belief is accessible via both indices
        var byAgent = await store.GetBeliefsForAgentAsync("agent-1").ConfigureAwait(false);
        var byCategory = await store.GetBeliefsForCategoryAsync("CodeGeneration").ConfigureAwait(false);

        await Assert.That(byAgent.Value.Count).IsEqualTo(1);
        await Assert.That(byCategory.Value.Count).IsEqualTo(1);
        await Assert.That(byAgent.Value[0].Alpha).IsEqualTo(belief.Alpha);
        await Assert.That(byCategory.Value[0].Alpha).IsEqualTo(belief.Alpha);
    }
}
