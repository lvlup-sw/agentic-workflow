// =============================================================================
// <copyright file="UserFeedbackTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using MultiModelRouter.State;

namespace MultiModelRouter.Tests.State;

/// <summary>
/// Unit tests for <see cref="UserFeedback"/> record.
/// </summary>
[Property("Category", "Unit")]
public class UserFeedbackTests
{
    /// <summary>
    /// Verifies that UserFeedback record is properly constructed.
    /// </summary>
    [Test]
    public async Task UserFeedback_CanBeConstructed()
    {
        // Arrange
        var rating = 5;
        var comment = "Great response!";
        var recordedAt = DateTimeOffset.UtcNow;

        // Act
        var feedback = new UserFeedback(rating, comment, recordedAt);

        // Assert
        await Assert.That(feedback.Rating).IsEqualTo(rating);
        await Assert.That(feedback.Comment).IsEqualTo(comment);
        await Assert.That(feedback.RecordedAt).IsEqualTo(recordedAt);
    }

    /// <summary>
    /// Verifies that UserFeedback can have null comment.
    /// </summary>
    [Test]
    public async Task UserFeedback_CommentCanBeNull()
    {
        // Arrange & Act
        var feedback = new UserFeedback(3, null, DateTimeOffset.UtcNow);

        // Assert
        await Assert.That(feedback.Comment).IsNull();
        await Assert.That(feedback.Rating).IsEqualTo(3);
    }
}

