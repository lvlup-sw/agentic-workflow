// =============================================================================
// <copyright file="FilterIndexBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Benchmarks.Fixtures;
using Strategos.Rag.Adapters;

using BenchmarkDotNet.Attributes;

namespace Strategos.Benchmarks.Subsystems.VectorSearch;

/// <summary>
/// Benchmarks for vector search operations with metadata filtering.
/// </summary>
/// <remarks>
/// <para>
/// <b>Note:</b> Filter indexing is not yet implemented in the vector search adapter.
/// These benchmarks serve as placeholders to establish baseline performance for
/// unfiltered searches and will be extended when filter support is added.
/// </para>
/// <para>
/// Future implementation will measure:
/// <list type="bullet">
/// <item>Pre-filter vs post-filter performance tradeoffs</item>
/// <item>Index selectivity impact on query performance</item>
/// <item>Multi-field filter combinations</item>
/// </list>
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class FilterIndexBenchmarks
{
    private InMemoryVectorSearchAdapter adapter = null!;
    private string query = null!;

    /// <summary>
    /// Gets or sets the number of documents in the search corpus.
    /// </summary>
    [Params(1000)]
    public int DocumentCount { get; set; }

    /// <summary>
    /// Sets up the benchmark by populating the vector store with test documents.
    /// </summary>
    [GlobalSetup]
    public void GlobalSetup()
    {
        this.adapter = new InMemoryVectorSearchAdapter();
        var documents = TestDocuments.CreateDocuments(this.DocumentCount);

        foreach (var doc in documents)
        {
            this.adapter.AddDocument(doc.Content, doc.Id);
        }

        var queries = TestDocuments.CreateQueries(1);
        this.query = queries[0];
    }

    /// <summary>
    /// Benchmarks search without any metadata filtering (baseline).
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    /// <remarks>
    /// This benchmark establishes the baseline performance for unfiltered searches,
    /// which will be compared against filtered searches once filter indexing is implemented.
    /// </remarks>
    [Benchmark(Baseline = true)]
    public async Task<int> SearchAsync_NoFilter()
    {
        var results = await this.adapter.SearchAsync(this.query, topK: 10, minRelevance: 0.0);
        return results.Count;
    }

    /// <summary>
    /// Placeholder benchmark for filtered search operations.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    /// <remarks>
    /// <para>
    /// <b>Not Implemented:</b> The current <see cref="InMemoryVectorSearchAdapter"/>
    /// does not support metadata filter indices. This benchmark runs an unfiltered
    /// search as a placeholder.
    /// </para>
    /// <para>
    /// When filter support is implemented, this benchmark will be updated to measure:
    /// <list type="bullet">
    /// <item>Filter predicate evaluation overhead</item>
    /// <item>Index lookup performance for common filter patterns</item>
    /// <item>Combined filter + relevance score optimization</item>
    /// </list>
    /// </para>
    /// </remarks>
    [Benchmark]
    public async Task<int> SearchAsync_WithFilter_NotImplemented()
    {
        // TODO: Replace with filtered search when filter indexing is implemented
        // Example future usage:
        // var filters = new Dictionary<string, object> { ["category"] = "workflow" };
        // var results = await this.adapter.SearchAsync(this.query, topK: 10, filters: filters);
        var results = await this.adapter.SearchAsync(this.query, topK: 10, minRelevance: 0.0);
        return results.Count;
    }
}
