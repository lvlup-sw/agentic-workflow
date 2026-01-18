// =============================================================================
// <copyright file="MockLlmServiceTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using ContentPipeline.Services;

namespace ContentPipeline.Tests.Services;

/// <summary>
/// Unit tests for <see cref="MockLlmService"/>.
/// </summary>
[Property("Category", "Unit")]
public class MockLlmServiceTests
{
    /// <summary>
    /// Verifies that GenerateDraftAsync returns non-empty content.
    /// </summary>
    [Test]
    public async Task GenerateDraftAsync_WithPrompt_ReturnsContent()
    {
        // Arrange
        var service = new MockLlmService();
        var prompt = "Write an article about software testing";

        // Act
        var result = await service.GenerateDraftAsync(prompt);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Length).IsGreaterThan(0);
    }

    /// <summary>
    /// Verifies that GenerateDraftAsync includes the topic in the response.
    /// </summary>
    [Test]
    public async Task GenerateDraftAsync_WithTopic_IncludesTopicInContent()
    {
        // Arrange
        var service = new MockLlmService();
        var prompt = "Write about artificial intelligence";

        // Act
        var result = await service.GenerateDraftAsync(prompt);

        // Assert
        await Assert.That(result).Contains("artificial intelligence");
    }

    /// <summary>
    /// Verifies that ReviewContentAsync returns feedback and score.
    /// </summary>
    [Test]
    public async Task ReviewContentAsync_WithContent_ReturnsFeedbackAndScore()
    {
        // Arrange
        var service = new MockLlmService();
        var content = "This is a test article about software development.";

        // Act
        var (feedback, score) = await service.ReviewContentAsync(content);

        // Assert
        await Assert.That(feedback).IsNotNull();
        await Assert.That(feedback.Length).IsGreaterThan(0);
        await Assert.That(score).IsGreaterThanOrEqualTo(0m);
        await Assert.That(score).IsLessThanOrEqualTo(1m);
    }

    /// <summary>
    /// Verifies that ReviewContentAsync returns higher score for longer content.
    /// </summary>
    [Test]
    public async Task ReviewContentAsync_WithLongerContent_ReturnsHigherScore()
    {
        // Arrange
        var service = new MockLlmService();
        var shortContent = "Short";
        var longContent = "This is a much longer piece of content that provides detailed information " +
                          "about the topic at hand, including multiple paragraphs and comprehensive coverage " +
                          "of the subject matter with supporting details and examples.";

        // Act
        var (_, shortScore) = await service.ReviewContentAsync(shortContent);
        var (_, longScore) = await service.ReviewContentAsync(longContent);

        // Assert
        await Assert.That(longScore).IsGreaterThan(shortScore);
    }

    /// <summary>
    /// Verifies that the service implements ILlmService.
    /// </summary>
    [Test]
    public async Task MockLlmService_ImplementsILlmService()
    {
        // Arrange & Act
        var service = new MockLlmService();

        // Assert
        await Assert.That(service).IsAssignableTo<ILlmService>();
    }
}
