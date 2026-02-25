// =============================================================================
// <copyright file="TaskCategory.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Selection;

/// <summary>
/// Categories of tasks for agent selection bandit learning.
/// </summary>
/// <remarks>
/// <para>
/// Task categories are used by Thompson Sampling to maintain separate
/// belief distributions for each (agent, category) pair. This enables
/// learning agent-specific performance across different task types.
/// </para>
/// </remarks>
public enum TaskCategory
{
    /// <summary>
    /// General tasks not matching specific categories.
    /// </summary>
    General,

    /// <summary>
    /// Code generation, refactoring, debugging, and programming tasks.
    /// </summary>
    CodeGeneration,

    /// <summary>
    /// Data analysis, visualization, and statistics tasks.
    /// </summary>
    DataAnalysis,

    /// <summary>
    /// Web search and information retrieval tasks.
    /// </summary>
    WebSearch,

    /// <summary>
    /// File operations and document processing tasks.
    /// </summary>
    FileOperation,

    /// <summary>
    /// File operations (plural alias for FileOperation).
    /// </summary>
    FileOperations,

    /// <summary>
    /// Reasoning, planning, and decision making tasks.
    /// </summary>
    Reasoning,

    /// <summary>
    /// Text generation, summarization, and translation tasks.
    /// </summary>
    TextGeneration,

    /// <summary>
    /// Documentation reading, creation, and maintenance tasks.
    /// </summary>
    Documentation,

    /// <summary>
    /// Testing, validation, and quality assurance tasks.
    /// </summary>
    Testing,
}
