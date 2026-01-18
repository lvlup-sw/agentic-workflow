// =============================================================================
// <copyright file="MockLlmService.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace ContentPipeline.Services;

/// <summary>
/// Mock implementation of <see cref="ILlmService"/> for testing purposes.
/// </summary>
/// <remarks>
/// This service simulates LLM behavior without making actual API calls.
/// Quality scores are based on content length as a simple heuristic.
/// </remarks>
public sealed class MockLlmService : ILlmService
{
    /// <inheritdoc/>
    public Task<string> GenerateDraftAsync(string prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prompt);

        // Extract key topic from prompt for mock response
        var topic = ExtractTopic(prompt);

        var draft = $"""
            # Article about {topic}

            This is a comprehensive article about {topic}. The content explores
            various aspects of this fascinating subject, providing readers with
            valuable insights and practical information.

            ## Introduction

            {topic} has become increasingly important in today's world. Understanding
            the fundamentals of {topic} can help professionals and enthusiasts alike
            navigate the complexities of this domain.

            ## Key Concepts

            When discussing {topic}, several key concepts come to mind:

            1. The foundational principles that govern {topic}
            2. Best practices for working with {topic}
            3. Common challenges and how to overcome them

            ## Conclusion

            In conclusion, {topic} represents an exciting area with tremendous potential.
            By following the guidelines outlined in this article, readers can develop
            a deeper understanding and practical skills related to {topic}.
            """;

        return Task.FromResult(draft);
    }

    /// <inheritdoc/>
    public Task<(string Feedback, decimal QualityScore)> ReviewContentAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        // Simple heuristic: longer, well-structured content gets higher scores
        var length = content.Length;
        var score = CalculateQualityScore(length);

        var feedback = GenerateFeedback(score);

        return Task.FromResult((feedback, score));
    }

    private static string ExtractTopic(string prompt)
    {
        // Case-insensitive search for "about" followed by optional colon and/or whitespace
        var aboutIndex = prompt.IndexOf("about", StringComparison.OrdinalIgnoreCase);
        if (aboutIndex >= 0)
        {
            var startIndex = aboutIndex + "about".Length;

            // Skip optional colon after "about"
            if (startIndex < prompt.Length && prompt[startIndex] == ':')
            {
                startIndex++;
            }

            // Skip optional whitespace after "about" or "about:"
            if (startIndex < prompt.Length && prompt[startIndex] == ' ')
            {
                startIndex++;
            }

            var endIndex = prompt.IndexOfAny(['.', ',', '!', '?'], startIndex);
            if (endIndex == -1)
            {
                endIndex = prompt.Length;
            }

            return prompt[startIndex..endIndex].Trim();
        }

        // Fallback: use first meaningful words
        return prompt.Length > 50 ? prompt[..50] + "..." : prompt;
    }

    private static decimal CalculateQualityScore(int contentLength)
    {
        // Score based on content length with diminishing returns
        return contentLength switch
        {
            < 50 => 0.3m,
            < 100 => 0.5m,
            < 200 => 0.65m,
            < 500 => 0.75m,
            < 1000 => 0.85m,
            _ => 0.9m,
        };
    }

    private static string GenerateFeedback(decimal score)
    {
        return score switch
        {
            >= 0.85m => "Excellent content with comprehensive coverage and clear structure. " +
                        "The article effectively communicates key points and maintains reader engagement.",
            >= 0.7m => "Good content with solid structure. Consider adding more specific examples " +
                       "and expanding certain sections for better depth.",
            >= 0.5m => "Adequate content but could use improvement. Recommendations: " +
                       "Add more detail, improve structure, include concrete examples.",
            _ => "Content needs significant improvement. Please expand the article with more " +
                 "comprehensive information, clearer structure, and supporting details.",
        };
    }
}
