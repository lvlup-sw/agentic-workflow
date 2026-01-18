// =============================================================================
// <copyright file="InMemoryStepExecutionLedgerTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Infrastructure.ExecutionLedgers;
using MemoryPack;
using Microsoft.Extensions.Time.Testing;

namespace Agentic.Workflow.Infrastructure.Tests.ExecutionLedgers;

/// <summary>
/// Tests for the <see cref="InMemoryStepExecutionLedger"/> class.
/// </summary>
/// <remarks>
/// Tests verify the in-memory implementation of the step execution ledger contract.
/// This implementation is suitable for testing and development scenarios.
/// </remarks>
[Property("Category", "Unit")]
public sealed partial class InMemoryStepExecutionLedgerTests
{
    // =========================================================================
    // A. TryGetCachedResultAsync Tests
    // =========================================================================

    /// <summary>
    /// Verifies that TryGetCachedResultAsync returns null when cache is empty.
    /// </summary>
    [Test]
    public async Task TryGetCachedResultAsync_WhenCacheEmpty_ReturnsNull()
    {
        // Arrange
        var ledger = CreateLedger();

        // Act
        var result = await ledger.TryGetCachedResultAsync<TestResult>(
            "step-name",
            "input-hash",
            CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that TryGetCachedResultAsync returns cached result when found.
    /// </summary>
    [Test]
    public async Task TryGetCachedResultAsync_WhenCached_ReturnsCachedResult()
    {
        // Arrange
        var ledger = CreateLedger();
        var expected = new TestResult("cached-value");
        await ledger.CacheResultAsync(
            "step-name",
            "input-hash",
            expected,
            null,
            CancellationToken.None).ConfigureAwait(false);

        // Act
        var result = await ledger.TryGetCachedResultAsync<TestResult>(
            "step-name",
            "input-hash",
            CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Value).IsEqualTo("cached-value");
    }

    /// <summary>
    /// Verifies that TryGetCachedResultAsync throws ArgumentException for null step name.
    /// </summary>
    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public async Task TryGetCachedResultAsync_WithInvalidStepName_ThrowsArgumentException(string? stepName)
    {
        // Arrange
        var ledger = CreateLedger();

        // Act & Assert
        await Assert.That(async () => await ledger.TryGetCachedResultAsync<TestResult>(
            stepName!,
            "input-hash",
            CancellationToken.None))
            .Throws<ArgumentException>()
            .WithParameterName("stepName");
    }

    /// <summary>
    /// Verifies that TryGetCachedResultAsync throws ArgumentException for null input hash.
    /// </summary>
    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public async Task TryGetCachedResultAsync_WithInvalidInputHash_ThrowsArgumentException(string? inputHash)
    {
        // Arrange
        var ledger = CreateLedger();

        // Act & Assert
        await Assert.That(async () => await ledger.TryGetCachedResultAsync<TestResult>(
            "step-name",
            inputHash!,
            CancellationToken.None))
            .Throws<ArgumentException>()
            .WithParameterName("inputHash");
    }

    /// <summary>
    /// Verifies that different step names have separate cache entries.
    /// </summary>
    [Test]
    public async Task TryGetCachedResultAsync_DifferentStepNames_ReturnsSeparateResults()
    {
        // Arrange
        var ledger = CreateLedger();
        var result1 = new TestResult("result-1");
        var result2 = new TestResult("result-2");

        await ledger.CacheResultAsync("step-1", "hash", result1, null, CancellationToken.None).ConfigureAwait(false);
        await ledger.CacheResultAsync("step-2", "hash", result2, null, CancellationToken.None).ConfigureAwait(false);

        // Act
        var cached1 = await ledger.TryGetCachedResultAsync<TestResult>("step-1", "hash", CancellationToken.None).ConfigureAwait(false);
        var cached2 = await ledger.TryGetCachedResultAsync<TestResult>("step-2", "hash", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(cached1!.Value).IsEqualTo("result-1");
        await Assert.That(cached2!.Value).IsEqualTo("result-2");
    }

    // =========================================================================
    // B. CacheResultAsync Tests
    // =========================================================================

    /// <summary>
    /// Verifies that CacheResultAsync stores the result successfully.
    /// </summary>
    [Test]
    public async Task CacheResultAsync_WithValidInput_StoresResult()
    {
        // Arrange
        var ledger = CreateLedger();
        var result = new TestResult("test-value");

        // Act
        await ledger.CacheResultAsync(
            "step-name",
            "input-hash",
            result,
            null,
            CancellationToken.None).ConfigureAwait(false);

        // Assert - Verify by retrieval
        var cached = await ledger.TryGetCachedResultAsync<TestResult>(
            "step-name",
            "input-hash",
            CancellationToken.None).ConfigureAwait(false);

        await Assert.That(cached).IsNotNull();
        await Assert.That(cached!.Value).IsEqualTo("test-value");
    }

    /// <summary>
    /// Verifies that CacheResultAsync throws ArgumentException for null step name.
    /// </summary>
    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public async Task CacheResultAsync_WithInvalidStepName_ThrowsArgumentException(string? stepName)
    {
        // Arrange
        var ledger = CreateLedger();
        var result = new TestResult("test-value");

        // Act & Assert
        await Assert.That(() => ledger.CacheResultAsync(
            stepName!,
            "input-hash",
            result,
            null,
            CancellationToken.None))
            .Throws<ArgumentException>()
            .WithParameterName("stepName");
    }

    /// <summary>
    /// Verifies that CacheResultAsync throws ArgumentException for null input hash.
    /// </summary>
    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public async Task CacheResultAsync_WithInvalidInputHash_ThrowsArgumentException(string? inputHash)
    {
        // Arrange
        var ledger = CreateLedger();
        var result = new TestResult("test-value");

        // Act & Assert
        await Assert.That(() => ledger.CacheResultAsync(
            "step-name",
            inputHash!,
            result,
            null,
            CancellationToken.None))
            .Throws<ArgumentException>()
            .WithParameterName("inputHash");
    }

    /// <summary>
    /// Verifies that CacheResultAsync throws ArgumentNullException for null result.
    /// </summary>
    [Test]
    public async Task CacheResultAsync_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        var ledger = CreateLedger();

        // Act & Assert
        await Assert.That(() => ledger.CacheResultAsync<TestResult>(
            "step-name",
            "input-hash",
            null!,
            null,
            CancellationToken.None))
            .Throws<ArgumentNullException>()
            .WithParameterName("result");
    }

    /// <summary>
    /// Verifies that CacheResultAsync overwrites existing cache entry.
    /// </summary>
    [Test]
    public async Task CacheResultAsync_CalledTwice_OverwritesPreviousEntry()
    {
        // Arrange
        var ledger = CreateLedger();
        var result1 = new TestResult("first");
        var result2 = new TestResult("second");

        await ledger.CacheResultAsync("step", "hash", result1, null, CancellationToken.None).ConfigureAwait(false);

        // Act
        await ledger.CacheResultAsync("step", "hash", result2, null, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var cached = await ledger.TryGetCachedResultAsync<TestResult>("step", "hash", CancellationToken.None).ConfigureAwait(false);
        await Assert.That(cached!.Value).IsEqualTo("second");
    }

    /// <summary>
    /// Verifies that CacheResultAsync respects TTL and expires entries.
    /// </summary>
    [Test]
    public async Task CacheResultAsync_WithTtl_ExpiresAfterDuration()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var ledger = CreateLedger(timeProvider);
        var result = new TestResult("test-value");
        var ttl = TimeSpan.FromMinutes(5);

        await ledger.CacheResultAsync("step", "hash", result, ttl, CancellationToken.None).ConfigureAwait(false);

        // Act - Advance time past TTL
        timeProvider.Advance(TimeSpan.FromMinutes(6));

        var cached = await ledger.TryGetCachedResultAsync<TestResult>("step", "hash", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(cached).IsNull();
    }

    // =========================================================================
    // C. ComputeInputHash Tests
    // =========================================================================

    /// <summary>
    /// Verifies that ComputeInputHash returns consistent hash for same input.
    /// </summary>
    [Test]
    public async Task ComputeInputHash_SameInput_ReturnsSameHash()
    {
        // Arrange
        var ledger = CreateLedger();
        var input = new TestInput(1, "test");

        // Act
        var hash1 = ledger.ComputeInputHash(input);
        var hash2 = ledger.ComputeInputHash(input);

        // Assert
        await Assert.That(hash1).IsEqualTo(hash2);
    }

    /// <summary>
    /// Verifies that ComputeInputHash returns different hash for different input.
    /// </summary>
    [Test]
    public async Task ComputeInputHash_DifferentInput_ReturnsDifferentHash()
    {
        // Arrange
        var ledger = CreateLedger();
        var input1 = new TestInput(1, "test");
        var input2 = new TestInput(2, "test");

        // Act
        var hash1 = ledger.ComputeInputHash(input1);
        var hash2 = ledger.ComputeInputHash(input2);

        // Assert
        await Assert.That(hash1).IsNotEqualTo(hash2);
    }

    /// <summary>
    /// Verifies that ComputeInputHash throws ArgumentNullException for null input.
    /// </summary>
    [Test]
    public async Task ComputeInputHash_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var ledger = CreateLedger();

        // Act & Assert
        await Assert.That(() => ledger.ComputeInputHash<TestInput>(null!))
            .Throws<ArgumentNullException>()
            .WithParameterName("input");
    }

    /// <summary>
    /// Verifies that ComputeInputHash returns hex-encoded string.
    /// </summary>
    [Test]
    public async Task ComputeInputHash_ReturnsHexEncodedString()
    {
        // Arrange
        var ledger = CreateLedger();
        var input = new TestInput(1, "test");

        // Act
        var hash = ledger.ComputeInputHash(input);

        // Assert - SHA256 produces 64 hex characters
        await Assert.That(hash.Length).IsEqualTo(64);
        await Assert.That(hash.All(c => char.IsLetterOrDigit(c))).IsTrue();
    }

    // =========================================================================
    // D. ValueTask Optimization Tests
    // =========================================================================

    /// <summary>
    /// Verifies that TryGetCachedResultAsync returns ValueTask to avoid Task allocations
    /// for synchronous cache operations.
    /// </summary>
    /// <remarks>
    /// This test validates the optimization where TryGetCachedResultAsync returns
    /// ValueTask instead of Task, avoiding heap allocations for the common case
    /// of synchronous dictionary lookups.
    /// </remarks>
    [Test]
    public async Task TryGetCachedResultAsync_ReturnsValueTask()
    {
        // Arrange
        var ledger = CreateLedger();
        var result = new TestResult("test-value");
        await ledger.CacheResultAsync("step", "hash", result, null, CancellationToken.None).ConfigureAwait(false);

        // Act - Get the method return type to verify it's ValueTask
        var method = typeof(InMemoryStepExecutionLedger).GetMethod(
            nameof(InMemoryStepExecutionLedger.TryGetCachedResultAsync));

        // Assert - Method should return ValueTask<TResult?>
        await Assert.That(method).IsNotNull();
        await Assert.That(method!.ReturnType.GetGenericTypeDefinition()).IsEqualTo(typeof(ValueTask<>));
    }

    /// <summary>
    /// Verifies that cache miss returns default ValueTask without allocation.
    /// </summary>
    [Test]
    public async Task TryGetCachedResultAsync_CacheMiss_ReturnsDefaultValueTask()
    {
        // Arrange
        var ledger = CreateLedger();

        // Act
        var result = await ledger.TryGetCachedResultAsync<TestResult>(
            "nonexistent-step",
            "nonexistent-hash",
            CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that cache hit returns ValueTask with result.
    /// </summary>
    [Test]
    public async Task TryGetCachedResultAsync_CacheHit_ReturnsValueTaskWithResult()
    {
        // Arrange
        var ledger = CreateLedger();
        var expected = new TestResult("cached-value");
        await ledger.CacheResultAsync("step", "hash", expected, null, CancellationToken.None).ConfigureAwait(false);

        // Act
        var result = await ledger.TryGetCachedResultAsync<TestResult>(
            "step",
            "hash",
            CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Value).IsEqualTo("cached-value");
    }

    // =========================================================================
    // E. Integration Tests
    // =========================================================================

    /// <summary>
    /// Verifies end-to-end flow: compute hash, cache, and retrieve.
    /// </summary>
    [Test]
    public async Task EndToEnd_ComputeCacheRetrieve_WorksCorrectly()
    {
        // Arrange
        var ledger = CreateLedger();
        var input = new TestInput(42, "workflow-step");
        var result = new TestResult("computed-result");

        // Act
        var inputHash = ledger.ComputeInputHash(input);
        await ledger.CacheResultAsync("process-step", inputHash, result, null, CancellationToken.None).ConfigureAwait(false);
        var cached = await ledger.TryGetCachedResultAsync<TestResult>("process-step", inputHash, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(cached).IsNotNull();
        await Assert.That(cached!.Value).IsEqualTo("computed-result");
    }

    // =========================================================================
    // Helper Methods
    // =========================================================================

    private static InMemoryStepExecutionLedger CreateLedger(TimeProvider? timeProvider = null)
    {
        return new InMemoryStepExecutionLedger(timeProvider ?? TimeProvider.System);
    }

    // =========================================================================
    // Test Fixtures
    // =========================================================================

    /// <summary>
    /// Test result for unit tests.
    /// </summary>
    [MemoryPackable]
    private sealed partial record TestResult(string Value);

    /// <summary>
    /// Test input for hash computation tests.
    /// </summary>
    [MemoryPackable]
    private sealed partial record TestInput(int Id, string Name);
}
