// =============================================================================
// <copyright file="StepExecutionLedgerOptions.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Infrastructure.ExecutionLedgers;

/// <summary>
/// Configuration options for <see cref="InMemoryStepExecutionLedger"/>.
/// </summary>
/// <remarks>
/// <para>
/// These options control the caching behavior of the step execution ledger:
/// <list type="bullet">
///   <item><description>Cache implementation: ConcurrentDictionary (default) or BitFaster ConcurrentLru</description></item>
///   <item><description>Cache capacity: Maximum number of entries (only for BitFaster)</description></item>
/// </list>
/// </para>
/// <para>
/// Use BitFaster.Caching's ConcurrentLru for high-throughput scenarios where bounded memory
/// is required. The LRU cache automatically evicts least-recently-used entries when capacity
/// is exceeded.
/// </para>
/// </remarks>
public sealed class StepExecutionLedgerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to use BitFaster ConcurrentLru cache.
    /// </summary>
    /// <value>
    /// <c>true</c> to use BitFaster ConcurrentLru; <c>false</c> to use ConcurrentDictionary (default).
    /// </value>
    /// <remarks>
    /// BitFaster's ConcurrentLru provides bounded capacity with LRU eviction policy.
    /// ConcurrentDictionary is unbounded but uses less memory per entry.
    /// </remarks>
    public bool UseBitFasterCache { get; set; }

    /// <summary>
    /// Gets or sets the maximum capacity of the cache when using BitFaster ConcurrentLru.
    /// </summary>
    /// <value>
    /// The maximum number of cache entries. Default is 10,000.
    /// </value>
    /// <remarks>
    /// This setting only applies when <see cref="UseBitFasterCache"/> is <c>true</c>.
    /// When capacity is exceeded, the least-recently-used entries are evicted.
    /// </remarks>
    public int CacheCapacity { get; set; } = 10000;
}
