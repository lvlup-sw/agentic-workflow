// =============================================================================
// <copyright file="FileSystemArtifactStoreOptions.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace Strategos.Infrastructure.Configuration;

/// <summary>
/// Configuration options for <see cref="ArtifactStores.FileSystemArtifactStore"/>.
/// </summary>
public sealed class FileSystemArtifactStoreOptions
{
    /// <summary>
    /// Gets or sets the base path for artifact storage.
    /// </summary>
    /// <remarks>
    /// All artifacts will be stored under this directory, organized by category.
    /// The directory will be created if it does not exist.
    /// </remarks>
    [Required]
    public string BasePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file extension for artifact files.
    /// </summary>
    /// <value>The default value is ".json".</value>
    public string FileExtension { get; set; } = ".json";
}
