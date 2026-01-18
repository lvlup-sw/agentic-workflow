// =============================================================================
// <copyright file="MockPublishingService.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace ContentPipeline.Services;

/// <summary>
/// Mock implementation of <see cref="IPublishingService"/> for testing purposes.
/// </summary>
/// <remarks>
/// This service simulates content publishing by generating mock URLs
/// and tracking published/unpublished state in memory.
/// </remarks>
public sealed class MockPublishingService : IPublishingService
{
    private readonly HashSet<string> _publishedUrls = [];
    private readonly string _baseUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockPublishingService"/> class.
    /// </summary>
    /// <param name="baseUrl">The base URL for published content.</param>
    public MockPublishingService(string baseUrl = "https://example.com/articles")
    {
        _baseUrl = baseUrl;
    }

    /// <inheritdoc/>
    public Task<string> PublishAsync(string title, string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(content);

        var slug = GenerateSlug(title);
        var url = $"{_baseUrl}/{slug}";

        _publishedUrls.Add(url);

        return Task.FromResult(url);
    }

    /// <inheritdoc/>
    public Task<bool> UnpublishAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);

        var removed = _publishedUrls.Remove(url);
        return Task.FromResult(removed);
    }

    /// <summary>
    /// Checks if content is currently published at the given URL.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <returns>True if content is published at the URL.</returns>
    public bool IsPublished(string url) => _publishedUrls.Contains(url);

    private static string GenerateSlug(string title)
    {
        return title
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace("'", string.Empty)
            .Replace("\"", string.Empty);
    }
}
