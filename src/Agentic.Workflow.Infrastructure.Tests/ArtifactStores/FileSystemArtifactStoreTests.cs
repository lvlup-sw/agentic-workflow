// =============================================================================
// <copyright file="FileSystemArtifactStoreTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Agentic.Workflow.Infrastructure.Tests.ArtifactStores;

/// <summary>
/// Tests for the <see cref="FileSystemArtifactStore"/> class.
/// </summary>
/// <remarks>
/// Tests verify the file system implementation of the artifact store contract.
/// This implementation is suitable for local deployments with durability requirements.
/// </remarks>
[Property("Category", "Unit")]
public sealed class FileSystemArtifactStoreTests : IAsyncDisposable
{
    private readonly string _testBasePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemArtifactStoreTests"/> class.
    /// </summary>
    public FileSystemArtifactStoreTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), $"artifact-store-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testBasePath);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, recursive: true);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    // =========================================================================
    // A. StoreAsync Tests
    // =========================================================================

    /// <summary>
    /// Verifies that StoreAsync returns a valid URI with file scheme.
    /// </summary>
    [Test]
    public async Task StoreAsync_WithValidArtifact_ReturnsUriWithFileScheme()
    {
        // Arrange
        var store = CreateStore();
        var artifact = new TestArtifact { Data = "test-data" };

        // Act
        var result = await store.StoreAsync(artifact, "test-category", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Scheme).IsEqualTo("file");
    }

    /// <summary>
    /// Verifies that StoreAsync creates the artifact file on disk.
    /// </summary>
    [Test]
    public async Task StoreAsync_WithValidArtifact_CreatesFileOnDisk()
    {
        // Arrange
        var store = CreateStore();
        var artifact = new TestArtifact { Data = "test-data" };

        // Act
        var uri = await store.StoreAsync(artifact, "test-category", CancellationToken.None).ConfigureAwait(false);

        // Assert
        var filePath = uri.LocalPath;
        await Assert.That(File.Exists(filePath)).IsTrue();
    }

    /// <summary>
    /// Verifies that StoreAsync throws ArgumentNullException when artifact is null.
    /// </summary>
    [Test]
    public async Task StoreAsync_WithNullArtifact_ThrowsArgumentNullException()
    {
        // Arrange
        var store = CreateStore();

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
        var store = CreateStore();
        var artifact = new TestArtifact { Data = "test-data" };

        // Act & Assert
        await Assert.That(async () => await store.StoreAsync(artifact, category!, CancellationToken.None))
            .Throws<ArgumentException>()
            .WithParameterName("category");
    }

    /// <summary>
    /// Verifies that StoreAsync creates category subdirectory.
    /// </summary>
    [Test]
    public async Task StoreAsync_WithCategory_CreatesCategorySubdirectory()
    {
        // Arrange
        var store = CreateStore();
        var artifact = new TestArtifact { Data = "test-data" };

        // Act
        var uri = await store.StoreAsync(artifact, "my-category", CancellationToken.None).ConfigureAwait(false);

        // Assert
        var filePath = uri.LocalPath;
        await Assert.That(filePath).Contains("my-category");
        await Assert.That(Directory.Exists(Path.GetDirectoryName(filePath))).IsTrue();
    }

    /// <summary>
    /// Verifies that multiple store operations return unique URIs.
    /// </summary>
    [Test]
    public async Task StoreAsync_CalledMultipleTimes_ReturnsUniqueUris()
    {
        // Arrange
        var store = CreateStore();
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
        var store = CreateStore();
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
        var store = CreateStore();

        // Act & Assert
        await Assert.That(async () => await store.RetrieveAsync<TestArtifact>(null!, CancellationToken.None))
            .Throws<ArgumentNullException>()
            .WithParameterName("reference");
    }

    /// <summary>
    /// Verifies that RetrieveAsync throws KeyNotFoundException when file not found.
    /// </summary>
    [Test]
    public async Task RetrieveAsync_WithNonExistentReference_ThrowsKeyNotFoundException()
    {
        // Arrange
        var store = CreateStore();
        var nonExistentUri = new Uri($"file:///{_testBasePath}/nonexistent/12345.json");

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
        var store = CreateStore();
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
    /// Verifies that DeleteAsync removes the artifact file from disk.
    /// </summary>
    [Test]
    public async Task DeleteAsync_WithValidReference_RemovesFileFromDisk()
    {
        // Arrange
        var store = CreateStore();
        var artifact = new TestArtifact { Data = "test-data" };
        var uri = await store.StoreAsync(artifact, "category", CancellationToken.None).ConfigureAwait(false);
        var filePath = uri.LocalPath;

        // Act
        await store.DeleteAsync(uri, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.That(File.Exists(filePath)).IsFalse();
    }

    /// <summary>
    /// Verifies that DeleteAsync throws ArgumentNullException when reference is null.
    /// </summary>
    [Test]
    public async Task DeleteAsync_WithNullReference_ThrowsArgumentNullException()
    {
        // Arrange
        var store = CreateStore();

        // Act & Assert
        await Assert.That(async () => await store.DeleteAsync(null!, CancellationToken.None))
            .Throws<ArgumentNullException>()
            .WithParameterName("reference");
    }

    /// <summary>
    /// Verifies that DeleteAsync is idempotent - succeeds silently for non-existent file.
    /// </summary>
    [Test]
    public async Task DeleteAsync_WithNonExistentReference_SucceedsSilently()
    {
        // Arrange
        var store = CreateStore();
        var nonExistentUri = new Uri($"file:///{_testBasePath}/nonexistent/12345.json");

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
        var store = CreateStore();
        var artifact = new TestArtifact { Data = "test-data" };
        var uri = await store.StoreAsync(artifact, "category", CancellationToken.None).ConfigureAwait(false);

        // Act - Delete twice
        await store.DeleteAsync(uri, CancellationToken.None).ConfigureAwait(false);
        await store.DeleteAsync(uri, CancellationToken.None).ConfigureAwait(false);

        // Assert - No exception means success (implicit assertion)
    }

    // =========================================================================
    // D. Options Tests
    // =========================================================================

    /// <summary>
    /// Verifies that constructor throws when options is null.
    /// </summary>
    [Test]
    public async Task Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.That(() => new FileSystemArtifactStore(null!))
            .Throws<ArgumentNullException>()
            .WithParameterName("options");
    }

    // =========================================================================
    // Helper Methods
    // =========================================================================

    private FileSystemArtifactStore CreateStore()
    {
        var options = Options.Create(new FileSystemArtifactStoreOptions
        {
            BasePath = _testBasePath,
        });

        return new FileSystemArtifactStore(options);
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
