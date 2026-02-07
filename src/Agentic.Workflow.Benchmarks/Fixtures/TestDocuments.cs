// =============================================================================
// <copyright file="TestDocuments.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Benchmarks.Fixtures;

/// <summary>
/// Provides test data generators for document-related benchmarks.
/// </summary>
/// <remarks>
/// <para>
/// Generates synthetic documents and queries for RAG and
/// semantic search benchmarks with reproducible random data.
/// </para>
/// </remarks>
public static class TestDocuments
{
    private static readonly string[] Keywords =
    [
        "workflow", "agent", "task", "execution", "step", "state",
        "budget", "token", "ledger", "progress", "cache", "belief",
        "selection", "sampling", "thompson", "detection", "loop",
    ];

    /// <summary>
    /// Creates a list of documents with synthetic content for benchmarks.
    /// </summary>
    /// <param name="count">The number of documents to create.</param>
    /// <returns>A read-only list of test documents.</returns>
    /// <remarks>
    /// <para>
    /// Documents are generated using a fixed seed for reproducibility
    /// across benchmark runs. Each document contains between 50-200 words
    /// drawn from a domain-specific keyword set.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<TestDocument> CreateDocuments(int count)
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var documents = new List<TestDocument>(count);

        for (int i = 0; i < count; i++)
        {
            var wordCount = random.Next(50, 200);
            var words = new List<string>(wordCount);

            for (int w = 0; w < wordCount; w++)
            {
                words.Add(Keywords[random.Next(Keywords.Length)]);
            }

            documents.Add(new TestDocument(string.Join(" ", words), $"doc-{i:D6}"));
        }

        return documents;
    }

    /// <summary>
    /// Creates a list of search queries for benchmarks.
    /// </summary>
    /// <param name="count">The number of queries to create.</param>
    /// <returns>A read-only list of query strings.</returns>
    /// <remarks>
    /// <para>
    /// Queries are generated using a different seed than documents
    /// to avoid artificial correlation. Each query contains between
    /// 2-5 keywords from the domain vocabulary.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<string> CreateQueries(int count)
    {
        var random = new Random(123); // Different seed
        var queries = new List<string>(count);

        for (int i = 0; i < count; i++)
        {
            var keywordCount = random.Next(2, 5);
            var queryWords = new List<string>(keywordCount);

            for (int k = 0; k < keywordCount; k++)
            {
                queryWords.Add(Keywords[random.Next(Keywords.Length)]);
            }

            queries.Add(string.Join(" ", queryWords));
        }

        return queries;
    }
}