// =============================================================================
// <copyright file="ITaskFeatureExtractor.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Selection;

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Contract for extracting features from task descriptions for contextual agent selection.
/// </summary>
/// <remarks>
/// <para>
/// Feature extractors analyze task descriptions and context to produce a
/// <see cref="TaskFeatures"/> representation that can inform agent selection decisions.
/// </para>
/// <para>
/// The default implementation uses keyword matching, but the abstraction enables
/// future implementations using embeddings, LLM classification, or domain-specific
/// feature extraction.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var extractor = new KeywordTaskFeatureExtractor();
/// var context = new AgentSelectionContext
/// {
///     TaskDescription = "Implement a sorting algorithm in Python",
///     // ... other properties
/// };
/// var features = extractor.ExtractFeatures(context);
/// // features.Category == TaskCategory.CodeGeneration
/// // features.Complexity ~= 0.3 (moderate)
/// </code>
/// </example>
public interface ITaskFeatureExtractor
{
    /// <summary>
    /// Extracts features from the given agent selection context.
    /// </summary>
    /// <param name="context">The context containing task description and metadata.</param>
    /// <returns>
    /// A <see cref="TaskFeatures"/> instance representing the extracted features.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method should be deterministic for the same input context to ensure
    /// consistent agent selection behavior.
    /// </para>
    /// <para>
    /// If the task description is null or empty, implementations should return
    /// <see cref="TaskFeatures.CreateDefault()"/> with <see cref="TaskCategory.General"/>
    /// and zero complexity.
    /// </para>
    /// </remarks>
    TaskFeatures ExtractFeatures(AgentSelectionContext context);
}
