// -----------------------------------------------------------------------
// <copyright file="XmlDocHelper.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Polyfills;

namespace Agentic.Workflow.Generators.Helpers;

/// <summary>
/// Provides helper methods for generating XML documentation comments in source generators.
/// </summary>
internal static class XmlDocHelper
{
    /// <summary>
    /// Appends a summary XML documentation block.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
    /// <param name="summary">The summary text.</param>
    /// <param name="indent">Optional indentation to apply to each line.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sb"/> or <paramref name="summary"/> is null.</exception>
    public static void AppendSummary(StringBuilder sb, string summary, string indent = "")
    {
        ThrowHelper.ThrowIfNull(sb, nameof(sb));
        ThrowHelper.ThrowIfNull(summary, nameof(summary));

        sb.AppendLine($"{indent}/// <summary>");
        sb.AppendLine($"{indent}/// {summary}");
        sb.AppendLine($"{indent}/// </summary>");
    }

    /// <summary>
    /// Appends a param XML documentation element.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
    /// <param name="name">The parameter name.</param>
    /// <param name="description">The parameter description.</param>
    /// <param name="indent">Optional indentation to apply.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sb"/>, <paramref name="name"/>, or <paramref name="description"/> is null.</exception>
    public static void AppendParam(StringBuilder sb, string name, string description, string indent = "")
    {
        ThrowHelper.ThrowIfNull(sb, nameof(sb));
        ThrowHelper.ThrowIfNull(name, nameof(name));
        ThrowHelper.ThrowIfNull(description, nameof(description));

        sb.AppendLine($"{indent}/// <param name=\"{name}\">{description}</param>");
    }

    /// <summary>
    /// Appends a returns XML documentation element.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
    /// <param name="description">The return value description.</param>
    /// <param name="indent">Optional indentation to apply.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sb"/> or <paramref name="description"/> is null.</exception>
    public static void AppendReturns(StringBuilder sb, string description, string indent = "")
    {
        ThrowHelper.ThrowIfNull(sb, nameof(sb));
        ThrowHelper.ThrowIfNull(description, nameof(description));

        sb.AppendLine($"{indent}/// <returns>{description}</returns>");
    }

    /// <summary>
    /// Appends a remarks XML documentation block.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
    /// <param name="lines">The remark lines to include.</param>
    /// <param name="indent">Optional indentation to apply to each line.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sb"/> or <paramref name="lines"/> is null.</exception>
    public static void AppendRemarks(StringBuilder sb, IEnumerable<string> lines, string indent = "")
    {
        ThrowHelper.ThrowIfNull(sb, nameof(sb));
        ThrowHelper.ThrowIfNull(lines, nameof(lines));

        sb.AppendLine($"{indent}/// <remarks>");
        foreach (var line in lines)
        {
            sb.AppendLine($"{indent}/// {line}");
        }

        sb.AppendLine($"{indent}/// </remarks>");
    }

    /// <summary>
    /// Appends a single-line remarks XML documentation element.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
    /// <param name="remark">The remark text.</param>
    /// <param name="indent">Optional indentation to apply.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sb"/> or <paramref name="remark"/> is null.</exception>
    public static void AppendRemarks(StringBuilder sb, string remark, string indent = "")
    {
        ThrowHelper.ThrowIfNull(sb, nameof(sb));
        ThrowHelper.ThrowIfNull(remark, nameof(remark));

        sb.AppendLine($"{indent}/// <remarks>");
        sb.AppendLine($"{indent}/// {remark}");
        sb.AppendLine($"{indent}/// </remarks>");
    }
}