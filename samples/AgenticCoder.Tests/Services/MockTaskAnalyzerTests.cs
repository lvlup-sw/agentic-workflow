// =============================================================================
// <copyright file="MockTaskAnalyzerTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using AgenticCoder.Services;

namespace AgenticCoder.Tests.Services;

/// <summary>
/// Unit tests for <see cref="MockTaskAnalyzer"/>.
/// </summary>
[Property("Category", "Unit")]
public class MockTaskAnalyzerTests
{
    /// <summary>
    /// Verifies that AnalyzeTaskAsync handles null taskDescription without throwing.
    /// </summary>
    [Test]
    public async Task AnalyzeTaskAsync_NullTaskDescription_DoesNotThrow()
    {
        // Arrange
        var analyzer = new MockTaskAnalyzer();

        // Act
        var result = await analyzer.AnalyzeTaskAsync(null!, CancellationToken.None);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.IsValid).IsFalse();
    }

    /// <summary>
    /// Verifies that AnalyzeTaskAsync handles empty taskDescription.
    /// </summary>
    [Test]
    public async Task AnalyzeTaskAsync_EmptyTaskDescription_ReturnsInvalid()
    {
        // Arrange
        var analyzer = new MockTaskAnalyzer();

        // Act
        var result = await analyzer.AnalyzeTaskAsync(string.Empty, CancellationToken.None);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.IsValid).IsFalse();
    }

    /// <summary>
    /// Verifies that AnalyzeTaskAsync handles whitespace-only taskDescription.
    /// </summary>
    [Test]
    public async Task AnalyzeTaskAsync_WhitespaceTaskDescription_ReturnsInvalid()
    {
        // Arrange
        var analyzer = new MockTaskAnalyzer();

        // Act
        var result = await analyzer.AnalyzeTaskAsync("   ", CancellationToken.None);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.IsValid).IsFalse();
    }

    /// <summary>
    /// Verifies that AnalyzeTaskAsync returns valid result for valid task description.
    /// </summary>
    [Test]
    public async Task AnalyzeTaskAsync_ValidTaskDescription_ReturnsValid()
    {
        // Arrange
        var analyzer = new MockTaskAnalyzer();

        // Act
        var result = await analyzer.AnalyzeTaskAsync("Implement a feature", CancellationToken.None);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.IsValid).IsTrue();
    }

    /// <summary>
    /// Verifies that AnalyzeTaskAsync returns Low complexity for short descriptions.
    /// </summary>
    [Test]
    public async Task AnalyzeTaskAsync_ShortDescription_ReturnsLowComplexity()
    {
        // Arrange
        var analyzer = new MockTaskAnalyzer();

        // Act
        var result = await analyzer.AnalyzeTaskAsync("Short task", CancellationToken.None);

        // Assert
        await Assert.That(result.Complexity).IsEqualTo("Low");
    }

    /// <summary>
    /// Verifies that AnalyzeTaskAsync returns High complexity for long descriptions.
    /// </summary>
    [Test]
    public async Task AnalyzeTaskAsync_LongDescription_ReturnsHighComplexity()
    {
        // Arrange
        var analyzer = new MockTaskAnalyzer();
        var longDescription = new string('a', 101);

        // Act
        var result = await analyzer.AnalyzeTaskAsync(longDescription, CancellationToken.None);

        // Assert
        await Assert.That(result.Complexity).IsEqualTo("High");
    }

    /// <summary>
    /// Verifies that AnalyzeTaskAsync adds FizzBuzz requirements when description contains FizzBuzz.
    /// </summary>
    [Test]
    public async Task AnalyzeTaskAsync_FizzBuzzDescription_AddsFizzBuzzRequirements()
    {
        // Arrange
        var analyzer = new MockTaskAnalyzer();

        // Act
        var result = await analyzer.AnalyzeTaskAsync("Implement FizzBuzz function", CancellationToken.None);

        // Assert
        await Assert.That(result.Requirements).Contains("Return 'Fizz' for multiples of 3");
        await Assert.That(result.Requirements).Contains("Return 'Buzz' for multiples of 5");
    }
}
