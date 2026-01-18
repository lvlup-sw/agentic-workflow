// =============================================================================
// <copyright file="IPublishingService.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace ContentPipeline.Services;

/// <summary>
/// Interface for content publishing services.
/// </summary>
public interface IPublishingService
{
    /// <summary>
    /// Publishes content and returns the published URL.
    /// </summary>
    /// <param name="title">The content title.</param>
    /// <param name="content">The content to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The URL where the content was published.</returns>
    Task<string> PublishAsync(string title, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unpublishes content from the given URL.
    /// </summary>
    /// <param name="url">The URL of the published content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if unpublishing was successful.</returns>
    Task<bool> UnpublishAsync(string url, CancellationToken cancellationToken = default);
}
