// =============================================================================
// <copyright file="DocumentSearchBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Benchmarks.Fixtures;
using Agentic.Workflow.Rag.Adapters;

using BenchmarkDotNet.Attributes;

namespace Agentic.Workflow.Benchmarks.Subsystems.LargeScale;

/// <summary>
/// Large-scale benchmarks for document search operations at production scale.
/// </summary>
/// <remarks>
/// <para>
/// These benchmarks measure the performance of the <see cref="InMemoryVectorSearchAdapter"/>
/// search operation at scales up to 10K+ documents to understand production-scale behavior.
/// </para>
/// <para>
/// Performance characteristics measured:
/// <list type="bullet">
///   <item><description>Search latency scaling from 100 to 10K documents</description></item>
///   <item><description>Memory allocation patterns at scale</description></item>
///   <item><description>TopK result retrieval efficiency at large corpus sizes</description></item>
///   <item><description>Relevance filtering effectiveness at scale</description></item>
/// </list>
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class DocumentSearchBenchmarks
{
    private InMemoryVectorSearchAdapter adapter = null!;
    private IReadOnlyList<string> queries = null!;

    /// <summary>
    /// Gets or sets the number of documents in the search corpus.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Scales from 100 to 10,000 documents to measure performance
    /// characteristics across two orders of magnitude.
    /// </para>
    /// </remarks>
    [Params(100, 1000, 10000)]
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

        // Create multiple queries for more realistic benchmarks
        this.queries = TestDocuments.CreateQueries(10);
    }

    /// <summary>
    /// Benchmarks search with TopK=5 (retrieve top 5 results) at scale.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    /// <remarks>
    /// <para>
    /// Measures baseline search performance with minimal result set
    /// to isolate search algorithm efficiency from result collection overhead.
    /// </para>
    /// </remarks>
    [Benchmark(Baseline = true)]
    public async Task<int> SearchAsync_TopK5()
    {
        var results = await this.adapter.SearchAsync(this.queries[0], topK: 5, minRelevance: 0.0);
        return results.Count;
    }

    /// <summary>
    /// Benchmarks search with TopK=20 (retrieve top 20 results) at scale.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    /// <remarks>
    /// <para>
    /// Comparing TopK=5 vs TopK=20 at large scales helps identify whether
    /// the implementation maintains efficiency with larger result sets.
    /// </para>
    /// </remarks>
    [Benchmark]
    public async Task<int> SearchAsync_TopK20()
    {
        var results = await this.adapter.SearchAsync(this.queries[0], topK: 20, minRelevance: 0.0);
        return results.Count;
    }

    /// <summary>
    /// Benchmarks search with TopK=50 at scale.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    /// <remarks>
    /// <para>
    /// At 10K documents, retrieving 50 results tests the efficiency of
    /// partial sorting algorithms under scale.
    /// </para>
    /// </remarks>
    [Benchmark]
    public async Task<int> SearchAsync_TopK50()
    {
        var results = await this.adapter.SearchAsync(this.queries[0], topK: 50, minRelevance: 0.0);
        return results.Count;
    }

    /// <summary>
    /// Benchmarks search with minimum relevance filtering at scale.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    /// <remarks>
    /// <para>
    /// Measures the effectiveness of early termination and relevance
    /// filtering when processing large document corpora.
    /// </para>
    /// </remarks>
    [Benchmark]
    public async Task<int> SearchAsync_WithMinRelevance()
    {
        var results = await this.adapter.SearchAsync(this.queries[0], topK: 20, minRelevance: 0.5);
        return results.Count;
    }

    /// <summary>
    /// Benchmarks multiple sequential searches to measure amortized performance.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    /// <remarks>
    /// <para>
    /// Simulates realistic usage patterns where multiple queries are
    /// executed against the same corpus. This measures cache effectiveness
    /// and steady-state performance.
    /// </para>
    /// </remarks>
    [Benchmark]
    public async Task<int> SearchAsync_MultipleQueries()
    {
        int totalResults = 0;
        foreach (var query in this.queries)
        {
            var results = await this.adapter.SearchAsync(query, topK: 10, minRelevance: 0.0);
            totalResults += results.Count;
        }

        return totalResults;
    }
}