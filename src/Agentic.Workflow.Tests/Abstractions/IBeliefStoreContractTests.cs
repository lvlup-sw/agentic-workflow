// =============================================================================
// <copyright file="IBeliefStoreContractTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Reflection;
using Agentic.Workflow.Primitives;
using Agentic.Workflow.Selection;

namespace Agentic.Workflow.Tests.Abstractions;

/// <summary>
/// Contract tests for <see cref="IBeliefStore"/> interface verifying that all async methods
/// return <see cref="ValueTask{TResult}"/> to eliminate allocations on synchronous paths.
/// </summary>
/// <remarks>
/// <para>
/// ValueTask is preferred over Task for belief store methods because:
/// <list type="bullet">
///   <item><description>Most belief lookups complete synchronously from in-memory cache</description></item>
///   <item><description>Task allocates a state machine on the heap even for synchronous completion</description></item>
///   <item><description>ValueTask avoids allocation when returning synchronously completed results</description></item>
/// </list>
/// </para>
/// </remarks>
[Property("Category", "Unit")]
public sealed class IBeliefStoreContractTests
{
    /// <summary>
    /// Verifies that GetBeliefAsync returns ValueTask to avoid allocation on cache hits.
    /// </summary>
    [Test]
    public async Task GetBeliefAsync_ReturnsValueTask()
    {
        // Arrange
        var method = typeof(IBeliefStore).GetMethod(nameof(IBeliefStore.GetBeliefAsync));

        // Assert
        await Assert.That(method).IsNotNull();
        await Assert.That(method!.ReturnType).IsEqualTo(typeof(ValueTask<Result<AgentBelief>>));
    }

    /// <summary>
    /// Verifies that UpdateBeliefAsync returns ValueTask for synchronous updates.
    /// </summary>
    [Test]
    public async Task UpdateBeliefAsync_ReturnsValueTask()
    {
        // Arrange
        var method = typeof(IBeliefStore).GetMethod(nameof(IBeliefStore.UpdateBeliefAsync));

        // Assert
        await Assert.That(method).IsNotNull();
        await Assert.That(method!.ReturnType).IsEqualTo(typeof(ValueTask<Result<Unit>>));
    }

    /// <summary>
    /// Verifies that GetBeliefsForAgentAsync returns ValueTask for indexed lookups.
    /// </summary>
    [Test]
    public async Task GetBeliefsForAgentAsync_ReturnsValueTask()
    {
        // Arrange
        var method = typeof(IBeliefStore).GetMethod(nameof(IBeliefStore.GetBeliefsForAgentAsync));

        // Assert
        await Assert.That(method).IsNotNull();
        await Assert.That(method!.ReturnType).IsEqualTo(typeof(ValueTask<Result<IReadOnlyList<AgentBelief>>>));
    }

    /// <summary>
    /// Verifies that GetBeliefsForCategoryAsync returns ValueTask for indexed lookups.
    /// </summary>
    [Test]
    public async Task GetBeliefsForCategoryAsync_ReturnsValueTask()
    {
        // Arrange
        var method = typeof(IBeliefStore).GetMethod(nameof(IBeliefStore.GetBeliefsForCategoryAsync));

        // Assert
        await Assert.That(method).IsNotNull();
        await Assert.That(method!.ReturnType).IsEqualTo(typeof(ValueTask<Result<IReadOnlyList<AgentBelief>>>));
    }

    /// <summary>
    /// Verifies that SaveBeliefAsync returns ValueTask for synchronous saves.
    /// </summary>
    [Test]
    public async Task SaveBeliefAsync_ReturnsValueTask()
    {
        // Arrange
        var method = typeof(IBeliefStore).GetMethod(nameof(IBeliefStore.SaveBeliefAsync));

        // Assert
        await Assert.That(method).IsNotNull();
        await Assert.That(method!.ReturnType).IsEqualTo(typeof(ValueTask<Result<Unit>>));
    }

    /// <summary>
    /// Verifies that all IBeliefStore async methods return ValueTask variants.
    /// </summary>
    /// <remarks>
    /// This comprehensive test ensures any new async methods added to the interface
    /// also use ValueTask for consistency and performance.
    /// </remarks>
    [Test]
    public async Task AllAsyncMethods_ReturnValueTask()
    {
        // Arrange
        var asyncMethods = typeof(IBeliefStore)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name.EndsWith("Async", StringComparison.Ordinal))
            .ToList();

        // Assert - all async methods should return ValueTask variants
        await Assert.That(asyncMethods.Count).IsGreaterThan(0);

        foreach (var method in asyncMethods)
        {
            var returnType = method.ReturnType;
            var isValueTask = returnType == typeof(ValueTask) ||
                             (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>));

            await Assert.That(isValueTask)
                .IsTrue()
                .Because($"Method {method.Name} should return ValueTask but returns {returnType.Name}");
        }
    }
}
