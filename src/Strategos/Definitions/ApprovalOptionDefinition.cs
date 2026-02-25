// =============================================================================
// <copyright file="ApprovalOptionDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Definitions;

/// <summary>
/// Immutable definition of an approval option that approvers can select.
/// </summary>
/// <remarks>
/// <para>
/// Approval options provide discrete choices for approvers:
/// <list type="bullet">
///   <item><description>OptionId: Unique identifier for programmatic handling</description></item>
///   <item><description>Label: Short display text for UI</description></item>
///   <item><description>Description: Detailed explanation of the option</description></item>
///   <item><description>IsDefault: Whether this option is pre-selected</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="OptionId">Unique identifier for the option.</param>
/// <param name="Label">Display label for the option.</param>
/// <param name="Description">Detailed description of the option.</param>
/// <param name="IsDefault">Whether this is the default option.</param>
public sealed record ApprovalOptionDefinition(
    string OptionId,
    string Label,
    string Description,
    bool IsDefault = false);
