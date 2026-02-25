// =============================================================================
// <copyright file="ApprovalConfiguration.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Models;

namespace Strategos.Definitions;

/// <summary>
/// Immutable configuration for an approval checkpoint.
/// </summary>
/// <remarks>
/// <para>
/// Approval configuration captures:
/// <list type="bullet">
///   <item><description>Type: The category of approval (safety, clarification, etc.)</description></item>
///   <item><description>Context: Message explaining what needs approval</description></item>
///   <item><description>Timeout: Maximum time to wait for response</description></item>
///   <item><description>Options: Discrete choices for the approver</description></item>
///   <item><description>Metadata: Additional context for the approval UI</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record ApprovalConfiguration
{
    /// <summary>
    /// Gets the type of approval being requested.
    /// </summary>
    public ApprovalType Type { get; init; } = ApprovalType.GeneralApproval;

    /// <summary>
    /// Gets the static context message (null if using dynamic context).
    /// </summary>
    public string? StaticContext { get; init; }

    /// <summary>
    /// Gets the serialized context factory expression (for code generation).
    /// </summary>
    /// <remarks>
    /// This stores the lambda expression as a string for source generator consumption.
    /// Example: "state => $\"Claim {state.ClaimId} for ${state.Amount}\"".
    /// </remarks>
    public string? ContextFactoryExpression { get; init; }

    /// <summary>
    /// Gets the timeout duration for the approval request.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Gets the approval options available to the approver.
    /// </summary>
    public IReadOnlyList<ApprovalOptionDefinition> Options { get; init; } = [];

    /// <summary>
    /// Gets the static metadata values.
    /// </summary>
    public IReadOnlyDictionary<string, object> StaticMetadata { get; init; } =
        new Dictionary<string, object>();

    /// <summary>
    /// Gets the dynamic metadata factory expressions (key to expression body).
    /// </summary>
    /// <remarks>
    /// Stores lambda expressions as strings for source generator consumption.
    /// Example: { "claimAmount": "state => state.Amount" }.
    /// </remarks>
    public IReadOnlyDictionary<string, string> DynamicMetadataExpressions { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Gets a default approval configuration with sensible defaults.
    /// </summary>
    public static ApprovalConfiguration Default { get; } = new();
}
