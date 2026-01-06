// =============================================================================
// <copyright file="Capability.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Orchestration;

/// <summary>
/// Defines the capabilities that executors can possess.
/// </summary>
/// <remarks>
/// <para>
/// This flags enum enables discriminative executor selection by matching task
/// requirements against executor capabilities. The orchestrator categorizes
/// tasks into required capabilities and selects executors with the best match.
/// </para>
/// <para>
/// Using flags allows executors to have multiple capabilities, enabling
/// efficient matching via bitwise operations: <c>required &amp; executor != 0</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define executor capabilities
/// var webSurferCaps = Capability.WebSearch | Capability.WebBrowsing;
/// var analystCaps = Capability.DataAnalysis | Capability.Visualization | Capability.DatabaseQuery;
///
/// // Check if executor can handle task
/// var required = Capability.WebSearch | Capability.DataAnalysis;
/// bool canHandle = (webSurferCaps &amp; required) != Capability.None;
/// </code>
/// </example>
[Flags]
public enum Capability
{
    /// <summary>
    /// No capabilities defined.
    /// </summary>
    None = 0,

    /// <summary>
    /// Ability to search the web for information.
    /// </summary>
    /// <remarks>
    /// Includes search engine queries, API searches, and keyword-based lookups.
    /// </remarks>
    WebSearch = 1 << 0,

    /// <summary>
    /// Ability to browse and navigate web pages.
    /// </summary>
    /// <remarks>
    /// Includes page fetching, content extraction, and link following.
    /// Often paired with WebSearch for comprehensive web research.
    /// </remarks>
    WebBrowsing = 1 << 1,

    /// <summary>
    /// Ability to perform data analysis and statistics.
    /// </summary>
    /// <remarks>
    /// Includes statistical computation, data transformation, and aggregation.
    /// </remarks>
    DataAnalysis = 1 << 2,

    /// <summary>
    /// Ability to generate source code.
    /// </summary>
    /// <remarks>
    /// Includes writing new code, implementing algorithms, and creating scripts.
    /// </remarks>
    CodeGeneration = 1 << 3,

    /// <summary>
    /// Ability to execute code in the sandbox environment.
    /// </summary>
    /// <remarks>
    /// Execution occurs via the Code Execution Bridge to the execution tier.
    /// </remarks>
    CodeExecution = 1 << 4,

    /// <summary>
    /// Ability to access and manipulate local files.
    /// </summary>
    /// <remarks>
    /// Includes file reading, writing, and directory operations.
    /// </remarks>
    FileAccess = 1 << 5,

    /// <summary>
    /// Ability to create visualizations and charts.
    /// </summary>
    /// <remarks>
    /// Includes generating plots, graphs, and other visual representations.
    /// Often paired with DataAnalysis for comprehensive reporting.
    /// </remarks>
    Visualization = 1 << 6,

    /// <summary>
    /// Ability to query databases.
    /// </summary>
    /// <remarks>
    /// Includes SQL queries, data retrieval, and database operations.
    /// </remarks>
    DatabaseQuery = 1 << 7
}
