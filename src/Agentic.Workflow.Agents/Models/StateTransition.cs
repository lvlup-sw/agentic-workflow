// =============================================================================
// <copyright file="StateTransition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Agents.Models;

/// <summary>
/// Represents a state transition in the HSM.
/// </summary>
/// <param name="From">The source state.</param>
/// <param name="To">The target state.</param>
/// <param name="Timestamp">When the transition occurred.</param>
public sealed record StateTransition(
    SpecialistState From,
    SpecialistState To,
    DateTimeOffset Timestamp);
