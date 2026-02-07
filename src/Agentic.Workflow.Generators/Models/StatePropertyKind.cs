// -----------------------------------------------------------------------
// <copyright file="StatePropertyKind.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Generators.Models;

/// <summary>
/// Categorizes property types for state reducer generation.
/// </summary>
internal enum StatePropertyKind
{
    /// <summary>
    /// Standard property - generates simple overwrite assignment.
    /// </summary>
    Standard,

    /// <summary>
    /// Collection property marked with [Append] - generates append methods.
    /// </summary>
    Append,

    /// <summary>
    /// Dictionary property marked with [Merge] - generates merge methods.
    /// </summary>
    Merge,
}