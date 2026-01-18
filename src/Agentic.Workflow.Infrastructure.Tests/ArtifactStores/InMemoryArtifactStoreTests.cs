// =============================================================================
// <copyright file="InMemoryArtifactStoreTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Infrastructure.Tests.ArtifactStores;

/// <summary>
/// Tests for the <see cref="InMemoryArtifactStore"/> class.
/// </summary>
/// <remarks>
/// Tests verify the in-memory implementation of the artifact store contract.
/// This implementation is suitable for testing and development scenarios.
/// </remarks>
[Property("Category", "Unit")]
public sealed class InMemoryArtifactStoreTests
{
    // =========================================================================
    // A. StoreAsync Tests
    // =========================================================================

    /// <summary>
    /// Verifies that StoreAsync returns a valid URI with memory scheme.
    /// </summary>
    [Test]
    public async Task StoreAsync_WithValidArtifact_ReturnsUriWithMemoryScheme()
    {
        // Arrange
        var store = new InMemoryArtifactStore();
        var artifact = new TestArtifact { Data = "test-data" };

        // Act
        var result = await store.StoreAsync(artifact, "test-category", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Scheme).IsEqualTo("memory");
    }

    /// <summary>
    /// Verifies that StoreAsync throws ArgumentNullException when artifact is null.
    /// </summary>
    [Test]
    public async Task StoreAsync_WithNullArtifact_ThrowsArgumentNullException()
    {
        // Arrange
        var store = new InMemoryArtifactStore();

        // Act & Assert
        await Assert.That(async () => await store.StoreAsync<TestArtifact>(null!, "category", CancellationToken.None))
            .Throws<ArgumentNullException>()
            .WithParameterName("artifact");
    }

    /// <summary>
    /// Verifies that StoreAsync throws ArgumentException when category is null, empty, or whitespace.
    /// </summary>
    /// <param name="category">The invalid category value.</param>
    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public async Task StoreAsync_WithInvalidCategory_ThrowsArgumentException(string? category)
    {
        // Arrange
        var store = new InMemoryArtifactStore();
        var artifact = new TestArtifact { Data = "test-data" };

        // Act & Assert
        await Assert.That(async () => await store.StoreAsync(artifact, category!, CancellationToken.None))
            .Throws<ArgumentException>()
            .WithParameterName("category");
    }

    /// <summary>
    /// Verifies that the returned URI contains the category in its path.
    /// </summary>
    [Test]
    public async Task StoreAsync_ReturnedUri_ContainsCategory()
    {
        // Arrange
        var store = new InMemoryArtifactStore();
        var artifact = new TestArtifact { Data = "test-data" };

        // Act
        var result = await store.StoreAsync(artifact, "my-category", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(result.ToString()).Contains("my-category");
    }

    /// <summary>
    /// Verifies that multiple store operations return unique URIs.
    /// </summary>
    [Test]
    public async Task StoreAsync_CalledMultipleTimes_ReturnsUniqueUris()
    {
        // Arrange
        var store = new InMemoryArtifactStore();
        var artifact1 = new TestArtifact { Data = "data-1" };
        var artifact2 = new TestArtifact { Data = "data-2" };

        // Act
        var uri1 = await store.StoreAsync(artifact1, "category", CancellationToken.None).ConfigureAwait(false);
        var uri2 = await store.StoreAsync(artifact2, "category", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(uri1).IsNotEqualTo(uri2);
    }

    // =========================================================================
    // B. RetrieveAsync Tests
    // =========================================================================

    /// <summary>
    /// Verifies that RetrieveAsync returns the previously stored artifact.
    /// </summary>
    [Test]
    public async Task RetrieveAsync_WithValidReference_ReturnsStoredArtifact()
    {
        // Arrange
        var store = new InMemoryArtifactStore();
        var artifact = new TestArtifact { Data = "test-data" };
        var uri = await store.StoreAsync(artifact, "category", CancellationToken.None).ConfigureAwait(false);

        // Act
        var result = await store.RetrieveAsync<TestArtifact>(uri, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Data).IsEqualTo("test-data");
    }

    /// <summary>
    /// Verifies that RetrieveAsync throws ArgumentNullException when reference is null.
    /// </summary>
    [Test]
    public async Task RetrieveAsync_WithNullReference_ThrowsArgumentNullException()
    {
        // Arrange
        var store = new InMemoryArtifactStore();

        // Act & Assert
        await Assert.That(async () => await store.RetrieveAsync<TestArtifact>(null!, CancellationToken.None))
            .Throws<ArgumentNullException>()
            .WithParameterName("reference");
    }

    /// <summary>
    /// Verifies that RetrieveAsync throws KeyNotFoundException when artifact not found.
    /// </summary>
    [Test]
    public async Task RetrieveAsync_WithNonExistentReference_ThrowsKeyNotFoundException()
    {
        // Arrange
        var store = new InMemoryArtifactStore();
        var nonExistentUri = new Uri("memory://artifacts/nonexistent/12345");

        // Act & Assert
        await Assert.That(async () => await store.RetrieveAsync<TestArtifact>(nonExistentUri, CancellationToken.None))
            .Throws<KeyNotFoundException>();
    }

    /// <summary>
    /// Verifies that RetrieveAsync throws KeyNotFoundException after artifact is deleted.
    /// </summary>
    [Test]
    public async Task RetrieveAsync_AfterDelete_ThrowsKeyNotFoundException()
    {
        // Arrange
        var store = new InMemoryArtifactStore();
        var artifact = new TestArtifact { Data = "test-data" };
        var uri = await store.StoreAsync(artifact, "category", CancellationToken.None).ConfigureAwait(false);
        await store.DeleteAsync(uri, CancellationToken.None).ConfigureAwait(false);

        // Act & Assert
        await Assert.That(async () => await store.RetrieveAsync<TestArtifact>(uri, CancellationToken.None))
            .Throws<KeyNotFoundException>();
    }

    // =========================================================================
    // C. DeleteAsync Tests
    // =========================================================================

    /// <summary>
    /// Verifies that DeleteAsync removes the artifact from the store.
    /// </summary>
    [Test]
    public async Task DeleteAsync_WithValidReference_RemovesArtifact()
    {
        // Arrange
        var store = new InMemoryArtifactStore();
        var artifact = new TestArtifact { Data = "test-data" };
        var uri = await store.StoreAsync(artifact, "category", CancellationToken.None).ConfigureAwait(false);

        // Act
        await store.DeleteAsync(uri, CancellationToken.None).ConfigureAwait(false);

        // Assert - Verify artifact is gone
        await Assert.That(async () => await store.RetrieveAsync<TestArtifact>(uri, CancellationToken.None))
            .Throws<KeyNotFoundException>();
    }

    /// <summary>
    /// Verifies that DeleteAsync throws ArgumentNullException when reference is null.
    /// </summary>
    [Test]
    public async Task DeleteAsync_WithNullReference_ThrowsArgumentNullException()
    {
        // Arrange
        var store = new InMemoryArtifactStore();

        // Act & Assert
        await Assert.That(async () => await store.DeleteAsync(null!, CancellationToken.None))
            .Throws<ArgumentNullException>()
            .WithParameterName("reference");
    }

    /// <summary>
    /// Verifies that DeleteAsync is idempotent - succeeds silently for non-existent artifact.
    /// </summary>
    [Test]
    public async Task DeleteAsync_WithNonExistentReference_SucceedsSilently()
    {
        // Arrange
        var store = new InMemoryArtifactStore();
        var nonExistentUri = new Uri("memory://artifacts/nonexistent/12345");

        // Act & Assert - Should not throw (idempotent)
        await store.DeleteAsync(nonExistentUri, CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// Verifies that DeleteAsync is idempotent - calling twice succeeds silently.
    /// </summary>
    [Test]
    public async Task DeleteAsync_CalledTwice_SucceedsSilently()
    {
        // Arrange
        var store = new InMemoryArtifactStore();
        var artifact = new TestArtifact { Data = "test-data" };
        var uri = await store.StoreAsync(artifact, "category", CancellationToken.None).ConfigureAwait(false);

        // Act - Delete twice
        await store.DeleteAsync(uri, CancellationToken.None).ConfigureAwait(false);
        await store.DeleteAsync(uri, CancellationToken.None).ConfigureAwait(false);

        // Assert - No exception means success (implicit assertion)
    }

    // =========================================================================
    // D. Concurrency Tests
    // =========================================================================

    /// <summary>
    /// Verifies that concurrent store operations all succeed with unique URIs.
    /// </summary>
    [Test]
    public async Task StoreAsync_ConcurrentCalls_AllSucceedWithUniqueUris()
    {
        // Arrange
        var store = new InMemoryArtifactStore();
        var tasks = new List<Task<Uri>>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var artifact = new TestArtifact { Data = $"data-{i}" };
            tasks.Add(store.StoreAsync(artifact, "category", CancellationToken.None).AsTask());
        }

        var uris = await Task.WhenAll(tasks).ConfigureAwait(false);

        // Assert - All URIs are unique
        var uniqueUris = uris.Distinct().ToList();
        await Assert.That(uniqueUris.Count).IsEqualTo(100);
    }

    // =========================================================================
    // Test Fixtures
    // =========================================================================

    /// <summary>
    /// Test artifact for unit tests.
    /// </summary>
    private sealed class TestArtifact
    {
        /// <summary>
        /// Gets or initializes the test data.
        /// </summary>
        public string Data { get; init; } = string.Empty;
    }
}
