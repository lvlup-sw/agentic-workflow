// =============================================================================
// <copyright file="MockModelProvider.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace MultiModelRouter.Services;

/// <summary>
/// Mock model provider that simulates different LLM models.
/// </summary>
/// <remarks>
/// <para>
/// This provider simulates responses from different models with varying
/// quality and latency characteristics:
/// <list type="bullet">
///   <item><description>gpt-4: High quality, slow, expensive</description></item>
///   <item><description>claude-3: High quality, moderate speed</description></item>
///   <item><description>local-model: Variable quality, fast, cheap</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class MockModelProvider : IModelProvider
{
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockModelProvider"/> class.
    /// </summary>
    /// <param name="seed">Optional random seed for reproducibility.</param>
    public MockModelProvider(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <inheritdoc/>
    public Task<ModelResponse> GenerateAsync(
        string modelId,
        string query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(modelId);
        ArgumentNullException.ThrowIfNull(query);

        // Simulate model-specific behavior
        var (response, confidence) = modelId switch
        {
            "gpt-4" => GenerateGpt4Response(query),
            "claude-3" => GenerateClaude3Response(query),
            "local-model" => GenerateLocalModelResponse(query),
            _ => ($"Response from {modelId}: {query}", 0.5m),
        };

        return Task.FromResult(new ModelResponse(response, confidence));
    }

    private (string Response, decimal Confidence) GenerateGpt4Response(string query)
    {
        var baseConfidence = 0.90m + ((decimal)_random.NextDouble() * 0.10m);
        var response = $"[GPT-4] {GetDetailedResponse(query)}";
        return (response, Math.Round(baseConfidence, 2));
    }

    private (string Response, decimal Confidence) GenerateClaude3Response(string query)
    {
        var baseConfidence = 0.85m + ((decimal)_random.NextDouble() * 0.12m);
        var response = $"[Claude-3] {GetDetailedResponse(query)}";
        return (response, Math.Round(baseConfidence, 2));
    }

    private (string Response, decimal Confidence) GenerateLocalModelResponse(string query)
    {
        // Local model has more variable quality
        var baseConfidence = 0.50m + ((decimal)_random.NextDouble() * 0.40m);
        var response = $"[Local] {GetSimpleResponse(query)}";
        return (response, Math.Round(baseConfidence, 2));
    }

    private static string GetDetailedResponse(string query)
    {
        if (query.Contains("capital", StringComparison.OrdinalIgnoreCase))
        {
            return "Paris is the capital of France. It has been the capital since 987 CE.";
        }

        if (query.Contains("poem", StringComparison.OrdinalIgnoreCase) ||
            query.Contains("story", StringComparison.OrdinalIgnoreCase))
        {
            return "In twilight's gentle embrace, where shadows softly dance...";
        }

        if (query.Contains("algorithm", StringComparison.OrdinalIgnoreCase) ||
            query.Contains("implement", StringComparison.OrdinalIgnoreCase))
        {
            return "Binary search divides the sorted array in half each iteration, achieving O(log n) complexity.";
        }

        if (query.Contains("hello", StringComparison.OrdinalIgnoreCase) ||
            query.Contains("how are you", StringComparison.OrdinalIgnoreCase))
        {
            return "Hello! I'm doing well, thank you for asking. How can I assist you today?";
        }

        return $"Here's a comprehensive answer to: {query}";
    }

    private static string GetSimpleResponse(string query)
    {
        if (query.Contains("capital", StringComparison.OrdinalIgnoreCase))
        {
            return "Paris is the capital of France.";
        }

        if (query.Contains("poem", StringComparison.OrdinalIgnoreCase))
        {
            return "Roses are red, violets are blue...";
        }

        if (query.Contains("algorithm", StringComparison.OrdinalIgnoreCase))
        {
            return "Binary search searches efficiently.";
        }

        return $"Answer: {query}";
    }
}
