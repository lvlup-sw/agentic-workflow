// =============================================================================
// <copyright file="SpecialistType.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Agentic.Workflow.Agents.Models;

/// <summary>
/// Defines the types of specialist agents available in the Magentic-One architecture.
/// </summary>
/// <remarks>
/// <para>
/// In the "Everything is a Coder" architecture, each specialist is a persona with a
/// specialized system prompt that generates Python code. The specialists differ in
/// their domain expertise and prompt engineering, not in their fundamental capabilities.
/// </para>
/// <para>
/// Each specialist type maps to a set of capability flags that define
/// what the specialist is best suited to accomplish.
/// </para>
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SpecialistType
{
    /// <summary>
    /// Specialist for web research and information retrieval.
    /// </summary>
    /// <remarks>
    /// System prompt: "You are a web researcher. You answer by writing Python code
    /// using the tools.search library." Capable of web search, browsing, and
    /// information extraction from online sources.
    /// </remarks>
    [JsonStringEnumMemberName("web_surfer")]
    WebSurfer,

    /// <summary>
    /// Specialist for data analysis and statistical computation.
    /// </summary>
    /// <remarks>
    /// System prompt focuses on data processing, statistical analysis, and visualization.
    /// Generates Python code using pandas, numpy, matplotlib, and similar libraries.
    /// </remarks>
    [JsonStringEnumMemberName("analyst")]
    Analyst,

    /// <summary>
    /// General-purpose specialist for code generation and software tasks.
    /// </summary>
    /// <remarks>
    /// Handles code writing, debugging, refactoring, and general programming tasks.
    /// The most versatile specialist with broad code generation capabilities.
    /// </remarks>
    [JsonStringEnumMemberName("coder")]
    Coder,

    /// <summary>
    /// Specialist for local file system operations and document analysis.
    /// </summary>
    /// <remarks>
    /// System prompt focuses on file reading, writing, and analysis of local documents.
    /// Generates Python code for file I/O, document parsing, and content extraction.
    /// </remarks>
    [JsonStringEnumMemberName("file_surfer")]
    FileSurfer
}
