// =============================================================================
// <copyright file="TestDocument.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Benchmarks.Fixtures;

/// <summary>
/// Represents a test document with content and identifier.
/// </summary>
/// <param name="Content">The document content.</param>
/// <param name="Id">The document identifier.</param>
public readonly record struct TestDocument(string Content, string Id);
