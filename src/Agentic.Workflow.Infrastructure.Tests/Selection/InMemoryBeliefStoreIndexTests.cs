// =============================================================================
// <copyright file="InMemoryBeliefStoreIndexTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Reflection;
using Agentic.Workflow.Infrastructure.Selection;

namespace Agentic.Workflow.Infrastructure.Tests.Selection;

/// <summary>
/// Unit tests for <see cref="InMemoryBeliefStore"/> index implementation,
/// specifically verifying the use of HashSet for memory efficiency.
/// </summary>
[Property("Category", "Unit")]
public sealed class InMemoryBeliefStoreIndexTests
{
    /// <summary>
    /// Verifies that the secondary indices use HashSet instead of ConcurrentDictionary
    /// to eliminate the byte sentinel overhead.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Using ConcurrentDictionary&lt;string, byte&gt; as a set wastes memory because:
    /// - Each entry has a byte value (1 byte + padding = 8 bytes on 64-bit)
    /// - The byte is always 0 (sentinel) and never used
    /// </para>
    /// <para>
    /// HashSet&lt;string&gt; eliminates this overhead by storing only the keys.
    /// This test uses reflection to verify the internal implementation choice.
    /// </para>
    /// </remarks>
    [Test]
    public async Task AddToIndices_HashSet_EliminatesByteSentinel()
    {
        // Arrange
        var store = new InMemoryBeliefStore();

        // Act - Add a belief to trigger index population
        await store.UpdateBeliefAsync("agent-1", "CodeGeneration", success: true).ConfigureAwait(false);

        // Assert - Use reflection to verify internal index types
        var storeType = store.GetType();

        // Check _byAgent field
        var byAgentField = storeType.GetField("_byAgent", BindingFlags.NonPublic | BindingFlags.Instance);
        await Assert.That(byAgentField).IsNotNull();

        var byAgentValue = byAgentField!.GetValue(store);
        await Assert.That(byAgentValue).IsNotNull();

        // The outer dictionary should map string -> HashSet<string>
        var byAgentType = byAgentValue!.GetType();
        var genericArgs = byAgentType.GetGenericArguments();

        // Verify the value type is HashSet<string> (wrapped in a thread-safe container)
        // We check that it's NOT ConcurrentDictionary<string, byte>
        await Assert.That(genericArgs.Length).IsGreaterThanOrEqualTo(2);

        var valueType = genericArgs[1];
        var isConcurrentDictionaryWithByte = valueType.IsGenericType
            && valueType.GetGenericTypeDefinition() == typeof(System.Collections.Concurrent.ConcurrentDictionary<,>)
            && valueType.GetGenericArguments().Length == 2
            && valueType.GetGenericArguments()[1] == typeof(byte);

        await Assert.That(isConcurrentDictionaryWithByte)
            .IsFalse()
            .Because("Index should use HashSet<string>, not ConcurrentDictionary<string, byte>");

        // Check _byCategory field
        var byCategoryField = storeType.GetField("_byCategory", BindingFlags.NonPublic | BindingFlags.Instance);
        await Assert.That(byCategoryField).IsNotNull();

        var byCategoryValue = byCategoryField!.GetValue(store);
        await Assert.That(byCategoryValue).IsNotNull();

        var byCategoryType = byCategoryValue!.GetType();
        var categoryGenericArgs = byCategoryType.GetGenericArguments();

        await Assert.That(categoryGenericArgs.Length).IsGreaterThanOrEqualTo(2);

        var categoryValueType = categoryGenericArgs[1];
        var isCategoryDictionaryWithByte = categoryValueType.IsGenericType
            && categoryValueType.GetGenericTypeDefinition() == typeof(System.Collections.Concurrent.ConcurrentDictionary<,>)
            && categoryValueType.GetGenericArguments().Length == 2
            && categoryValueType.GetGenericArguments()[1] == typeof(byte);

        await Assert.That(isCategoryDictionaryWithByte)
            .IsFalse()
            .Because("Index should use HashSet<string>, not ConcurrentDictionary<string, byte>");
    }

    /// <summary>
    /// Verifies that the HashSet-based indices still maintain thread safety
    /// under concurrent access.
    /// </summary>
    [Test]
    public async Task HashSetIndices_ConcurrentAccess_MaintainsThreadSafety()
    {
        // Arrange
        var store = new InMemoryBeliefStore();
        const int taskCount = 100;
        const int categoriesPerTask = 10;

        // Act - Perform concurrent updates from multiple tasks
        var tasks = new List<Task>();
        for (int t = 0; t < taskCount; t++)
        {
            var taskId = t;
            tasks.Add(Task.Run(async () =>
            {
                for (int c = 0; c < categoriesPerTask; c++)
                {
                    await store.UpdateBeliefAsync($"agent-{taskId}", $"category-{c}", success: true).ConfigureAwait(false);
                }
            }));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Assert - Verify all beliefs were indexed correctly
        for (int t = 0; t < taskCount; t++)
        {
            var agentBeliefs = await store.GetBeliefsForAgentAsync($"agent-{t}").ConfigureAwait(false);
            await Assert.That(agentBeliefs.IsSuccess).IsTrue();
            await Assert.That(agentBeliefs.Value.Count).IsEqualTo(categoriesPerTask);
        }

        for (int c = 0; c < categoriesPerTask; c++)
        {
            var categoryBeliefs = await store.GetBeliefsForCategoryAsync($"category-{c}").ConfigureAwait(false);
            await Assert.That(categoryBeliefs.IsSuccess).IsTrue();
            await Assert.That(categoryBeliefs.Value.Count).IsEqualTo(taskCount);
        }
    }

    /// <summary>
    /// Verifies that simultaneous reads and writes to the HashSet indices
    /// do not cause exceptions or data corruption.
    /// </summary>
    [Test]
    public async Task HashSetIndices_SimultaneousReadsWrites_NoExceptions()
    {
        // Arrange
        var store = new InMemoryBeliefStore();
        const int iterationCount = 50;
        var exceptions = new List<Exception>();

        // Pre-populate with some data
        for (int i = 0; i < 10; i++)
        {
            await store.UpdateBeliefAsync($"agent-{i}", "initial-category", success: true).ConfigureAwait(false);
        }

        // Act - Concurrent reads and writes
        var tasks = new List<Task>();
        for (int i = 0; i < iterationCount; i++)
        {
            var index = i;
            // Writers
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await store.UpdateBeliefAsync($"agent-{index % 10}", $"category-{index}", success: true).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    lock (exceptions) { exceptions.Add(ex); }
                }
            }));

            // Readers (agent index)
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await store.GetBeliefsForAgentAsync($"agent-{index % 10}").ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    lock (exceptions) { exceptions.Add(ex); }
                }
            }));

            // Readers (category index)
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await store.GetBeliefsForCategoryAsync($"category-{index % 5}").ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    lock (exceptions) { exceptions.Add(ex); }
                }
            }));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Assert - No exceptions should have occurred
        await Assert.That(exceptions).IsEmpty();
    }
}
