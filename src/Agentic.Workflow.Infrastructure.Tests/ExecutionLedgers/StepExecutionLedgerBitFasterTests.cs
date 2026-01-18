// =============================================================================
// <copyright file="StepExecutionLedgerBitFasterTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Infrastructure.ExecutionLedgers;
using Microsoft.Extensions.Time.Testing;

namespace Agentic.Workflow.Infrastructure.Tests.ExecutionLedgers;

/// <summary>
/// Tests for BitFaster ConcurrentLru cache support in <see cref="InMemoryStepExecutionLedger"/>.
/// </summary>
/// <remarks>
/// Tests verify that the ledger can optionally use BitFaster.Caching's ConcurrentLru
/// for high-performance caching scenarios with bounded capacity.
/// </remarks>
[Property("Category", "Unit")]
public sealed class StepExecutionLedgerBitFasterTests
{
    // =========================================================================
    // A. BitFaster Configuration Tests
    // =========================================================================

    /// <summary>
    /// Verifies that StepExecutionLedgerOptions can enable BitFaster cache.
    /// </summary>
    [Test]
    public async Task StepExecutionLedgerOptions_BitFasterEnabled_UsesConcurrentLru()
    {
        // Arrange
        var options = new StepExecutionLedgerOptions
        {
            UseBitFasterCache = true,
            CacheCapacity = 100
        };
        var ledger = new InMemoryStepExecutionLedger(TimeProvider.System, Options.Create(options));
        var result = new TestResult { Value = "cached-value" };

        // Act
        await ledger.CacheResultAsync("step", "hash", result, null, CancellationToken.None).ConfigureAwait(false);
        var cached = await ledger.TryGetCachedResultAsync<TestResult>("step", "hash", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(cached).IsNotNull();
        await Assert.That(cached!.Value).IsEqualTo("cached-value");
    }

    /// <summary>
    /// Verifies that default options use ConcurrentDictionary for backwards compatibility.
    /// </summary>
    [Test]
    public async Task StepExecutionLedgerOptions_DefaultSettings_UsesConcurrentDictionary()
    {
        // Arrange
        var options = new StepExecutionLedgerOptions();

        // Assert defaults
        await Assert.That(options.UseBitFasterCache).IsFalse();
        await Assert.That(options.CacheCapacity).IsEqualTo(10000);
    }

    /// <summary>
    /// Verifies that ledger without options constructor uses default ConcurrentDictionary.
    /// </summary>
    [Test]
    public async Task InMemoryStepExecutionLedger_WithoutOptions_UsesConcurrentDictionary()
    {
        // Arrange - use existing constructor without options
        var ledger = new InMemoryStepExecutionLedger(TimeProvider.System);
        var result = new TestResult { Value = "cached-value" };

        // Act
        await ledger.CacheResultAsync("step", "hash", result, null, CancellationToken.None).ConfigureAwait(false);
        var cached = await ledger.TryGetCachedResultAsync<TestResult>("step", "hash", CancellationToken.None).ConfigureAwait(false);

        // Assert - should work with default dictionary implementation
        await Assert.That(cached).IsNotNull();
        await Assert.That(cached!.Value).IsEqualTo("cached-value");
    }

    // =========================================================================
    // B. BitFaster Cache Behavior Tests
    // =========================================================================

    /// <summary>
    /// Verifies that BitFaster cache evicts oldest entries when capacity is exceeded.
    /// </summary>
    [Test]
    public async Task BitFasterCache_WhenCapacityExceeded_EvictsOldestEntries()
    {
        // Arrange
        var options = new StepExecutionLedgerOptions
        {
            UseBitFasterCache = true,
            CacheCapacity = 3
        };
        var ledger = new InMemoryStepExecutionLedger(TimeProvider.System, Options.Create(options));

        // Act - Add 4 entries (exceeding capacity of 3)
        for (var i = 1; i <= 4; i++)
        {
            var result = new TestResult { Value = $"value-{i}" };
            await ledger.CacheResultAsync($"step-{i}", "hash", result, null, CancellationToken.None).ConfigureAwait(false);
        }

        // Assert - First entry should be evicted, last 3 should be present
        var first = await ledger.TryGetCachedResultAsync<TestResult>("step-1", "hash", CancellationToken.None).ConfigureAwait(false);
        var last = await ledger.TryGetCachedResultAsync<TestResult>("step-4", "hash", CancellationToken.None).ConfigureAwait(false);

        await Assert.That(first).IsNull();
        await Assert.That(last).IsNotNull();
        await Assert.That(last!.Value).IsEqualTo("value-4");
    }

    /// <summary>
    /// Verifies that BitFaster cache supports TTL expiration.
    /// </summary>
    [Test]
    public async Task BitFasterCache_WithTtl_ExpiresEntries()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var options = new StepExecutionLedgerOptions
        {
            UseBitFasterCache = true,
            CacheCapacity = 100
        };
        var ledger = new InMemoryStepExecutionLedger(timeProvider, Options.Create(options));
        var result = new TestResult { Value = "test-value" };
        var ttl = TimeSpan.FromMinutes(5);

        // Act
        await ledger.CacheResultAsync("step", "hash", result, ttl, CancellationToken.None).ConfigureAwait(false);

        // Advance time past TTL
        timeProvider.Advance(TimeSpan.FromMinutes(6));

        var cached = await ledger.TryGetCachedResultAsync<TestResult>("step", "hash", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(cached).IsNull();
    }

    /// <summary>
    /// Verifies that BitFaster cache supports overwriting entries.
    /// </summary>
    [Test]
    public async Task BitFasterCache_OverwriteEntry_ReturnsNewValue()
    {
        // Arrange
        var options = new StepExecutionLedgerOptions
        {
            UseBitFasterCache = true,
            CacheCapacity = 100
        };
        var ledger = new InMemoryStepExecutionLedger(TimeProvider.System, Options.Create(options));
        var result1 = new TestResult { Value = "first" };
        var result2 = new TestResult { Value = "second" };

        // Act
        await ledger.CacheResultAsync("step", "hash", result1, null, CancellationToken.None).ConfigureAwait(false);
        await ledger.CacheResultAsync("step", "hash", result2, null, CancellationToken.None).ConfigureAwait(false);
        var cached = await ledger.TryGetCachedResultAsync<TestResult>("step", "hash", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(cached).IsNotNull();
        await Assert.That(cached!.Value).IsEqualTo("second");
    }

    // =========================================================================
    // C. Integration Tests
    // =========================================================================

    /// <summary>
    /// Verifies end-to-end flow with BitFaster cache: compute hash, cache, and retrieve.
    /// </summary>
    [Test]
    public async Task BitFasterCache_EndToEnd_ComputeCacheRetrieve_WorksCorrectly()
    {
        // Arrange
        var options = new StepExecutionLedgerOptions
        {
            UseBitFasterCache = true,
            CacheCapacity = 100
        };
        var ledger = new InMemoryStepExecutionLedger(TimeProvider.System, Options.Create(options));
        var input = new TestInput { Id = 42, Name = "workflow-step" };
        var result = new TestResult { Value = "computed-result" };

        // Act
        var inputHash = ledger.ComputeInputHash(input);
        await ledger.CacheResultAsync("process-step", inputHash, result, null, CancellationToken.None).ConfigureAwait(false);
        var cached = await ledger.TryGetCachedResultAsync<TestResult>("process-step", inputHash, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(cached).IsNotNull();
        await Assert.That(cached!.Value).IsEqualTo("computed-result");
    }

    // =========================================================================
    // Test Fixtures
    // =========================================================================

    /// <summary>
    /// Test result for unit tests.
    /// </summary>
    private sealed class TestResult
    {
        /// <summary>
        /// Gets or initializes the value.
        /// </summary>
        public string Value { get; init; } = string.Empty;
    }

    /// <summary>
    /// Test input for hash computation tests.
    /// </summary>
    private sealed class TestInput
    {
        /// <summary>
        /// Gets or initializes the ID.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets or initializes the name.
        /// </summary>
        public string Name { get; init; } = string.Empty;
    }
}
