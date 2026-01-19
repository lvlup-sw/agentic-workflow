// =============================================================================
// <copyright file="AuditEntry.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace ContentPipeline.State;

/// <summary>
/// Represents an entry in the audit trail.
/// </summary>
/// <param name="Timestamp">When the action occurred.</param>
/// <param name="Action">The action that was taken.</param>
/// <param name="Actor">Who or what performed the action.</param>
/// <param name="Details">Additional details about the action.</param>
public sealed record AuditEntry(
    DateTimeOffset Timestamp,
    string Action,
    string Actor,
    string? Details);
