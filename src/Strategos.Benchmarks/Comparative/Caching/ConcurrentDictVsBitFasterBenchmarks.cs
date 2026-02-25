// =============================================================================
// <copyright file="ConcurrentDictVsBitFasterBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Collections.Concurrent;

using BenchmarkDotNet.Attributes;

using BitFaster.Caching.Lfu;
using BitFaster.Caching.Lru;

namespace Strategos.Benchmarks.Comparative.Caching;

/// <summary>
/// Compares ConcurrentDictionary with BitFaster lock-free cache implementations.
/// </summary>
/// <remarks>
/// <para>
/// BitFaster.Caching provides high-performance lock-free LRU and LFU caches
/// that can outperform ConcurrentDictionary for bounded cache scenarios.
/// </para>
/// <para>
/// This benchmark measures GetOrAdd and TryGetValue operations to compare
/// throughput under typical cache usage patterns.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ConcurrentDictVsBitFasterBenchmarks
{
    private const int CacheCapacity = 1000;
    private const int KeyRange = 2000; // Some keys will miss to simulate real workload

    private ConcurrentDictionary<string, CacheEntry> _concurrentDict = null!;
    private ConcurrentLru<string, CacheEntry> _concurrentLru = null!;
    private ConcurrentLfu<string, CacheEntry> _concurrentLfu = null!;
    private string[] _keys = null!;
    private int _keyIndex;

    /// <summary>
    /// Sets up cache instances and test keys before benchmarks.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _concurrentDict = new ConcurrentDictionary<string, CacheEntry>();
        _concurrentLru = new ConcurrentLru<string, CacheEntry>(CacheCapacity);
        _concurrentLfu = new ConcurrentLfu<string, CacheEntry>(CacheCapacity);

        // Generate keys for lookup patterns
        var random = new Random(42);
        _keys = new string[KeyRange];
        for (int i = 0; i < KeyRange; i++)
        {
            _keys[i] = $"key-{i:D6}";
        }

        // Pre-populate caches with half the keys
        for (int i = 0; i < KeyRange / 2; i++)
        {
            var key = _keys[i];
            var entry = CreateEntry(key);
            _concurrentDict[key] = entry;
            _concurrentLru.GetOrAdd(key, _ => entry);
            _concurrentLfu.GetOrAdd(key, _ => entry);
        }

        _keyIndex = 0;
    }

    /// <summary>
    /// Baseline ConcurrentDictionary GetOrAdd operation.
    /// </summary>
    /// <returns>The cached or newly added entry.</returns>
    [Benchmark(Baseline = true)]
    public CacheEntry ConcurrentDictionary_GetOrAdd()
    {
        var key = GetNextKey();
        return _concurrentDict.GetOrAdd(key, k => CreateEntry(k));
    }

    /// <summary>
    /// Baseline ConcurrentDictionary TryGetValue operation.
    /// </summary>
    /// <returns>True if key was found.</returns>
    [Benchmark]
    public bool ConcurrentDictionary_TryGetValue()
    {
        var key = GetNextKey();
        return _concurrentDict.TryGetValue(key, out _);
    }

    /// <summary>
    /// BitFaster ConcurrentLru GetOrAdd operation with bounded capacity.
    /// </summary>
    /// <returns>The cached or newly added entry.</returns>
    [Benchmark]
    public CacheEntry ConcurrentLru_GetOrAdd()
    {
        var key = GetNextKey();
        return _concurrentLru.GetOrAdd(key, k => CreateEntry(k));
    }

    /// <summary>
    /// BitFaster ConcurrentLfu GetOrAdd operation with bounded capacity.
    /// </summary>
    /// <returns>The cached or newly added entry.</returns>
    [Benchmark]
    public CacheEntry ConcurrentLfu_GetOrAdd()
    {
        var key = GetNextKey();
        return _concurrentLfu.GetOrAdd(key, k => CreateEntry(k));
    }

    private string GetNextKey()
    {
        var index = Interlocked.Increment(ref _keyIndex) % _keys.Length;
        return _keys[index];
    }

    private static CacheEntry CreateEntry(string key)
    {
        return new CacheEntry(key, DateTime.UtcNow, 42);
    }
}

/// <summary>
/// Represents a cached entry for benchmarking.
/// </summary>
/// <param name="Key">The cache key.</param>
/// <param name="CreatedAt">When the entry was created.</param>
/// <param name="Value">The cached value.</param>
public readonly record struct CacheEntry(string Key, DateTime CreatedAt, int Value);
