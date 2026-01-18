// =============================================================================
// <copyright file="QueryCategoryTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using MultiModelRouter.State;

namespace MultiModelRouter.Tests.State;

/// <summary>
/// Unit tests for <see cref="QueryCategory"/> enum.
/// </summary>
[Property("Category", "Unit")]
public class QueryCategoryTests
{
    /// <summary>
    /// Verifies that QueryCategory has expected values.
    /// </summary>
    [Test]
    public async Task QueryCategory_HasExpectedValues()
    {
        // Assert
        await Assert.That(Enum.GetNames<QueryCategory>()).Contains("Factual");
        await Assert.That(Enum.GetNames<QueryCategory>()).Contains("Creative");
        await Assert.That(Enum.GetNames<QueryCategory>()).Contains("Technical");
        await Assert.That(Enum.GetNames<QueryCategory>()).Contains("Conversational");
    }
}

