// =============================================================================
// <copyright file="ApprovalOption.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Models;

/// <summary>
/// Represents an option that the approver can select in response to an approval request.
/// </summary>
/// <param name="OptionId">Unique identifier for this option.</param>
/// <param name="Label">Short display label for the option.</param>
/// <param name="Description">Detailed description of what selecting this option means.</param>
/// <param name="IsDefault">Whether this option is the default/recommended choice.</param>
public sealed record ApprovalOption(
    string OptionId,
    string Label,
    string Description,
    bool IsDefault = false);