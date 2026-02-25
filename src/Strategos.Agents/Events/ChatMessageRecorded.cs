// =============================================================================
// <copyright file="ChatMessageRecorded.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Agents.Abstractions;
using Strategos.Agents.Models;

namespace Strategos.Agents.Events;

/// <summary>
/// Event raised when a chat message is exchanged with an LLM during workflow execution.
/// </summary>
/// <remarks>
/// <para>
/// This event captures all messages in conversations between the system and LLM specialists,
/// enabling a complete audit trail for compliance, debugging, and conversation replay.
/// </para>
/// <para>
/// Messages are associated with a specific <see cref="SpecialistType"/> to maintain
/// the per-specialist thread isolation recommended by MAF best practices. The
/// <see cref="TaskId"/> provides correlation with the workflow's task ledger.
/// </para>
/// <para>
/// When stored in Marten event streams, these events can be projected to reconstruct
/// full conversation histories for any workflow or specialist type combination.
/// </para>
/// </remarks>
/// <param name="WorkflowId">The unique identifier for the workflow this message belongs to.</param>
/// <param name="TaskId">The task identifier from the TaskLedger this message is associated with.</param>
/// <param name="SpecialistType">The type of specialist agent that sent or received this message.</param>
/// <param name="Role">The role of the message sender (System, User, Assistant, or Tool).</param>
/// <param name="Content">The content of the message.</param>
/// <param name="Timestamp">The timestamp when this message was recorded.</param>
public sealed record ChatMessageRecorded(
    Guid WorkflowId,
    string TaskId,
    SpecialistType SpecialistType,
    MessageRole Role,
    string Content,
    DateTimeOffset Timestamp) : IProgressEvent;
