// =============================================================================
// <copyright file="IArtifactStoreContractTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Reflection;

namespace Agentic.Workflow.Tests.Abstractions;

/// <summary>
/// Contract tests for <see cref="IArtifactStore"/> interface.
/// </summary>
/// <remarks>
/// These tests verify the interface contract using reflection to ensure
/// methods return ValueTask instead of Task for allocation-free synchronous paths.
/// </remarks>
[Property("Category", "Unit")]
public sealed class IArtifactStoreContractTests
{
    // =============================================================================
    // A. Interface Return Type Contract Tests
    // =============================================================================

    /// <summary>
    /// Verifies that StoreAsync returns ValueTask{Uri} instead of Task{Uri}.
    /// </summary>
    /// <remarks>
    /// ValueTask eliminates allocations on synchronous completion paths,
    /// which is important for high-throughput artifact store implementations.
    /// </remarks>
    [Test]
    public async Task StoreAsync_ReturnsValueTask()
    {
        // Arrange
        var interfaceType = typeof(IArtifactStore);
        var storeAsyncMethod = interfaceType.GetMethod("StoreAsync");

        // Assert
        await Assert.That(storeAsyncMethod).IsNotNull();
        await Assert.That(storeAsyncMethod!.ReturnType.IsGenericType).IsTrue();
        await Assert.That(storeAsyncMethod.ReturnType.GetGenericTypeDefinition())
            .IsEqualTo(typeof(ValueTask<>));
        await Assert.That(storeAsyncMethod.ReturnType.GetGenericArguments()[0])
            .IsEqualTo(typeof(Uri));
    }

    /// <summary>
    /// Verifies that RetrieveAsync returns ValueTask{T} instead of Task{T}.
    /// </summary>
    /// <remarks>
    /// ValueTask eliminates allocations on synchronous completion paths,
    /// especially beneficial for in-memory artifact stores.
    /// </remarks>
    [Test]
    public async Task RetrieveAsync_ReturnsValueTask()
    {
        // Arrange
        var interfaceType = typeof(IArtifactStore);
        var retrieveAsyncMethod = interfaceType.GetMethod("RetrieveAsync");

        // Assert
        await Assert.That(retrieveAsyncMethod).IsNotNull();
        await Assert.That(retrieveAsyncMethod!.ReturnType.IsGenericType).IsTrue();
        await Assert.That(retrieveAsyncMethod.ReturnType.GetGenericTypeDefinition())
            .IsEqualTo(typeof(ValueTask<>));
    }

    /// <summary>
    /// Verifies that DeleteAsync returns ValueTask instead of Task.
    /// </summary>
    /// <remarks>
    /// ValueTask eliminates allocations on synchronous completion paths,
    /// which is the common case for delete operations.
    /// </remarks>
    [Test]
    public async Task DeleteAsync_ReturnsValueTask()
    {
        // Arrange
        var interfaceType = typeof(IArtifactStore);
        var deleteAsyncMethod = interfaceType.GetMethod("DeleteAsync");

        // Assert
        await Assert.That(deleteAsyncMethod).IsNotNull();
        await Assert.That(deleteAsyncMethod!.ReturnType).IsEqualTo(typeof(ValueTask));
    }

    /// <summary>
    /// Verifies that all async methods on IArtifactStore use ValueTask for allocation optimization.
    /// </summary>
    /// <remarks>
    /// This comprehensive test ensures all current and future async methods
    /// follow the ValueTask pattern for optimal performance.
    /// </remarks>
    [Test]
    public async Task AllAsyncMethods_UseValueTaskReturnType()
    {
        // Arrange
        var interfaceType = typeof(IArtifactStore);
        var asyncMethods = interfaceType.GetMethods()
            .Where(m => m.Name.EndsWith("Async", StringComparison.Ordinal))
            .ToList();

        // Assert - should have exactly 3 async methods
        await Assert.That(asyncMethods.Count).IsEqualTo(3);

        foreach (var method in asyncMethods)
        {
            var returnType = method.ReturnType;

            // Method should return either ValueTask or ValueTask<T>
            var isValueTask = returnType == typeof(ValueTask);
            var isGenericValueTask = returnType.IsGenericType &&
                                     returnType.GetGenericTypeDefinition() == typeof(ValueTask<>);

            await Assert.That(isValueTask || isGenericValueTask)
                .IsTrue()
                .Because($"Method {method.Name} should return ValueTask or ValueTask<T>, not {returnType.Name}");
        }
    }
}
