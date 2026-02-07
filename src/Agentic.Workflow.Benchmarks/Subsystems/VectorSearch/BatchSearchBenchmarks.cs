// =============================================================================
// <copyright file="BatchSearchBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Benchmarks.Fixtures;
using Agentic.Workflow.Rag.Adapters;

using BenchmarkDotNet.Attributes;

namespace Agentic.Workflow.Benchmarks.Subsystems.VectorSearch;

/// <summary>
/// Benchmarks for batch vector search operations comparing sequential vs parallel execution.
/// </summary>
/// <remarks>
/// <para>
/// <b>Note:</b> A dedicated batch search API is not yet implemented in the vector search adapter.
/// These benchmarks measure the performance of multiple individual searches executed
/// sequentially vs in parallel using Task.WhenAll.
/// </para>
/// <para>
/// Future batch API implementation may provide:
/// <list type="bullet">
/// <item>Batched embedding computation for multiple queries</item>
/// <item>Optimized multi-query execution on vector databases</item>
/// <item>Connection pooling and request coalescing</item>
/// </list>
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class BatchSearchBenchmarks
{
    private InMemoryVectorSearchAdapter adapter = null!;
    private IReadOnlyList<string> queries = null!;

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

        // Create 10 queries for batch operations
        this.queries = TestDocuments.CreateQueries(10);
    }

    /// <summary>
    /// Benchmarks 10 sequential search operations (baseline).
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    /// <remarks>
    /// <para>
    /// Executes 10 search queries sequentially, awaiting each one before starting
    /// the next. This establishes the baseline for batch operation performance.
    /// </para>
    /// </remarks>
    [Benchmark(Baseline = true)]
    public async Task<int> SearchAsync_Sequential_10Queries()
    {
        var totalResults = 0;

        foreach (var query in this.queries)
        {
            var results = await this.adapter.SearchAsync(query, topK: 5, minRelevance: 0.0);
            totalResults += results.Count;
        }

        return totalResults;
    }

    /// <summary>
    /// Benchmarks 10 parallel search operations.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    /// <remarks>
    /// <para>
    /// Executes 10 search queries in parallel using <see cref="Task.WhenAll"/>.
    /// For I/O-bound operations against remote vector databases, this pattern
    /// can significantly improve throughput.
    /// </para>
    /// <para>
    /// <b>Note:</b> With the in-memory adapter, parallel execution may show
    /// minimal improvement as all operations complete synchronously. When
    /// benchmarking against real vector databases (PgVector, Azure AI Search),
    /// parallel execution should demonstrate improved throughput.
    /// </para>
    /// </remarks>
    [Benchmark]
    public async Task<int> SearchAsync_Parallel_10Queries()
    {
        var tasks = this.queries.Select(query =>
            this.adapter.SearchAsync(query, topK: 5, minRelevance: 0.0));

        var allResults = await Task.WhenAll(tasks);

        return allResults.Sum(r => r.Count);
    }
}
