// =============================================================================
// <copyright file="SearchBenchmarks.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Benchmarks.Fixtures;
using Agentic.Workflow.Rag.Adapters;
using BenchmarkDotNet.Attributes;

namespace Agentic.Workflow.Benchmarks.Subsystems.VectorSearch;

/// <summary>
/// Benchmarks for vector search operations with varying document counts and TopK values.
/// </summary>
/// <remarks>
/// <para>
/// These benchmarks measure the performance of the <see cref="InMemoryVectorSearchAdapter"/>
/// search operation across different corpus sizes and result set sizes.
/// </para>
/// <para>
/// Performance characteristics:
/// <list type="bullet">
/// <item>TopK=5 vs TopK=20 comparison enables O(n log n) vs O(n + k log k) analysis</item>
/// <item>MinRelevance filtering benchmarks measure early termination effectiveness</item>
/// </list>
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class SearchBenchmarks
{
    private InMemoryVectorSearchAdapter adapter = null!;
    private string query = null!;

    /// <summary>
    /// Gets or sets the number of documents in the search corpus.
    /// </summary>
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

        // Use a consistent query for all benchmarks
        var queries = TestDocuments.CreateQueries(1);
        this.query = queries[0];
    }

    /// <summary>
    /// Benchmarks search with TopK=5 (retrieve top 5 results).
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    [Benchmark(Baseline = true)]
    public async Task<int> SearchAsync_TopK5()
    {
        var results = await this.adapter.SearchAsync(this.query, topK: 5, minRelevance: 0.0);
        return results.Count;
    }

    /// <summary>
    /// Benchmarks search with TopK=20 (retrieve top 20 results).
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    /// <remarks>
    /// <para>
    /// Comparing TopK=5 vs TopK=20 helps identify whether the implementation
    /// uses an efficient partial sort (O(n + k log k)) or full sort (O(n log n)).
    /// </para>
    /// </remarks>
    [Benchmark]
    public async Task<int> SearchAsync_TopK20()
    {
        var results = await this.adapter.SearchAsync(this.query, topK: 20, minRelevance: 0.0);
        return results.Count;
    }

    /// <summary>
    /// Benchmarks search with minimum relevance filtering enabled.
    /// </summary>
    /// <returns>A task representing the asynchronous benchmark operation.</returns>
    /// <remarks>
    /// <para>
    /// This benchmark measures the effectiveness of early termination
    /// when documents below the relevance threshold can be pruned.
    /// </para>
    /// </remarks>
    [Benchmark]
    public async Task<int> SearchAsync_WithMinRelevance()
    {
        var results = await this.adapter.SearchAsync(this.query, topK: 10, minRelevance: 0.5);
        return results.Count;
    }
}
