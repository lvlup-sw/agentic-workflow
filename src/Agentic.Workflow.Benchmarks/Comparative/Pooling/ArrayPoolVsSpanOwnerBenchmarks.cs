// =============================================================================
// <copyright file="ArrayPoolVsSpanOwnerBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Buffers;

using BenchmarkDotNet.Attributes;

using CommunityToolkit.HighPerformance.Buffers;

namespace Agentic.Workflow.Benchmarks.Comparative.Pooling;

/// <summary>
/// Compares array allocation strategies: new array, ArrayPool, SpanOwner, and StringPool.
/// </summary>
/// <remarks>
/// <para>
/// This benchmark measures the allocation and deallocation costs of different
/// buffer management strategies to identify optimal approaches for hot paths.
/// </para>
/// <para>
/// SpanOwner from CommunityToolkit provides a convenient wrapper around ArrayPool
/// with automatic disposal, while StringPool helps reduce string allocations.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ArrayPoolVsSpanOwnerBenchmarks
{
    private const int BufferSize = 4096;
    private const int StringCount = 100;

    private string[] _testStrings = null!;
    private StringPool _stringPool = null!;
    private int _stringIndex;

    /// <summary>
    /// Sets up test data before benchmarks.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        // Generate test strings for string pooling benchmarks
        var random = new Random(42);
        _testStrings = new string[StringCount];
        for (int i = 0; i < StringCount; i++)
        {
            // Create strings with some duplicates to test pooling
            var index = random.Next(StringCount / 2);
            _testStrings[i] = $"pooled-string-{index:D4}";
        }

        _stringPool = new StringPool();
        _stringIndex = 0;
    }

    /// <summary>
    /// Cleans up resources after benchmarks.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        _stringPool.Reset();
    }

    /// <summary>
    /// Baseline: allocate a new array (causes GC pressure).
    /// </summary>
    /// <returns>Sum of array elements.</returns>
    [Benchmark(Baseline = true)]
    public int NewArray_Allocate()
    {
        var buffer = new byte[BufferSize];
        // Simulate some work to prevent dead code elimination
        buffer[0] = 1;
        buffer[BufferSize - 1] = 2;
        return buffer[0] + buffer[BufferSize - 1];
    }

    /// <summary>
    /// ArrayPool: rent and return from shared pool.
    /// </summary>
    /// <returns>Sum of array elements.</returns>
    [Benchmark]
    public int ArrayPool_RentReturn()
    {
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        try
        {
            // Simulate some work
            buffer[0] = 1;
            buffer[BufferSize - 1] = 2;
            return buffer[0] + buffer[BufferSize - 1];
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// SpanOwner: convenient wrapper with automatic disposal.
    /// </summary>
    /// <returns>Sum of span elements.</returns>
    [Benchmark]
    public int SpanOwner_RentReturn()
    {
        using var owner = SpanOwner<byte>.Allocate(BufferSize);
        var span = owner.Span;
        // Simulate some work
        span[0] = 1;
        span[BufferSize - 1] = 2;
        return span[0] + span[BufferSize - 1];
    }

    /// <summary>
    /// StringPool: deduplicate strings to reduce allocations.
    /// </summary>
    /// <returns>The pooled string.</returns>
    [Benchmark]
    public string StringPool_GetOrAdd()
    {
        var index = Interlocked.Increment(ref _stringIndex) % _testStrings.Length;
        var original = _testStrings[index];

        // Create a span from the string to simulate building a string
        var span = original.AsSpan();
        return _stringPool.GetOrAdd(span);
    }
}