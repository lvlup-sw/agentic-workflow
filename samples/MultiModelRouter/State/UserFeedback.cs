// =============================================================================
// <copyright file="UserFeedback.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace MultiModelRouter.State;

/// <summary>
/// User feedback recorded after response generation.
/// </summary>
/// <param name="Rating">The user rating (1-5).</param>
/// <param name="Comment">Optional comment from the user.</param>
/// <param name="RecordedAt">When the feedback was recorded.</param>
public sealed record UserFeedback(int Rating, string? Comment, DateTimeOffset RecordedAt);

