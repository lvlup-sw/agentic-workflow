// =============================================================================
// <copyright file="JsonVsMemoryPackBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json;

using BenchmarkDotNet.Attributes;

using MemoryPack;

namespace Agentic.Workflow.Benchmarks.Comparative.Serialization;

/// <summary>
/// Compares System.Text.Json serialization with MemoryPack binary serialization.
/// </summary>
/// <remarks>
/// <para>
/// MemoryPack is a high-performance binary serializer that can provide
/// significant speed improvements over JSON for internal data transfer.
/// </para>
/// <para>
/// This benchmark measures both serialization and deserialization throughput
/// across varying data sizes to identify crossover points.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class JsonVsMemoryPackBenchmarks
{
    private List<SerializationEntry> _entries = null!;
    private byte[] _jsonBytes = null!;
    private byte[] _memoryPackBytes = null!;
    private JsonSerializerOptions _jsonOptions = null!;

    /// <summary>
    /// Gets or sets the number of entries to serialize.
    /// </summary>
    [Params(10, 100, 1000)]
    public int EntryCount { get; set; }

    /// <summary>
    /// Sets up test data before each benchmark iteration.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _entries = new List<SerializationEntry>(EntryCount);

        for (int i = 0; i < EntryCount; i++)
        {
            _entries.Add(new SerializationEntry
            {
                Id = Guid.NewGuid(),
                Name = $"Entry-{i:D6}",
                Value = random.NextDouble() * 1000,
                Timestamp = DateTime.UtcNow.AddMinutes(-random.Next(1000)),
                Tags = Enumerable.Range(0, random.Next(1, 10))
                    .Select(t => $"tag-{t}")
                    .ToList(),
            });
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        // Pre-serialize for deserialization benchmarks
        _jsonBytes = JsonSerializer.SerializeToUtf8Bytes(_entries, _jsonOptions);
        _memoryPackBytes = MemoryPackSerializer.Serialize(_entries);
    }

    /// <summary>
    /// Baseline System.Text.Json serialization.
    /// </summary>
    /// <returns>Serialized bytes.</returns>
    [Benchmark(Baseline = true)]
    public byte[] SystemTextJson_Serialize()
    {
        return JsonSerializer.SerializeToUtf8Bytes(_entries, _jsonOptions);
    }

    /// <summary>
    /// Baseline System.Text.Json deserialization.
    /// </summary>
    /// <returns>Deserialized entries.</returns>
    [Benchmark]
    public List<SerializationEntry>? SystemTextJson_Deserialize()
    {
        return JsonSerializer.Deserialize<List<SerializationEntry>>(_jsonBytes, _jsonOptions);
    }

    /// <summary>
    /// MemoryPack binary serialization.
    /// </summary>
    /// <returns>Serialized bytes.</returns>
    [Benchmark]
    public byte[] MemoryPack_Serialize()
    {
        return MemoryPackSerializer.Serialize(_entries);
    }

    /// <summary>
    /// MemoryPack binary deserialization.
    /// </summary>
    /// <returns>Deserialized entries.</returns>
    [Benchmark]
    public List<SerializationEntry>? MemoryPack_Deserialize()
    {
        return MemoryPackSerializer.Deserialize<List<SerializationEntry>>(_memoryPackBytes);
    }
}

/// <summary>
/// Test entry type for serialization benchmarks.
/// </summary>
[MemoryPackable]
public partial class SerializationEntry
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the entry name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the numeric value.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the tags collection.
    /// </summary>
    public List<string> Tags { get; set; } = [];
}