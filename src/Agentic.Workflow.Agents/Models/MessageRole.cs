// =============================================================================
// <copyright file="MessageRole.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Text.Json.Serialization;

namespace Agentic.Workflow.Agents.Models;

/// <summary>
/// Defines the roles of participants in a conversation with an LLM.
/// </summary>
/// <remarks>
/// <para>
/// These roles align with standard LLM chat completion APIs (OpenAI, Azure OpenAI,
/// Anthropic, etc.) and are used to track message provenance in the audit trail.
/// </para>
/// <para>
/// The <see cref="JsonStringEnumMemberName"/> attributes ensure consistent
/// JSON serialization that matches API conventions.
/// </para>
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageRole
{
    /// <summary>
    /// A system message that sets the context or instructions for the assistant.
    /// </summary>
    /// <remarks>
    /// System messages typically contain the specialist's persona, capabilities,
    /// and behavioral guidelines. They are usually set at the start of a conversation.
    /// </remarks>
    [JsonStringEnumMemberName("system")]
    System,

    /// <summary>
    /// A message from the user or the orchestrating system requesting action.
    /// </summary>
    /// <remarks>
    /// In the Magentic-One pattern, user messages often contain the task description
    /// and any context injected by the orchestrator.
    /// </remarks>
    [JsonStringEnumMemberName("user")]
    User,

    /// <summary>
    /// A response from the AI assistant.
    /// </summary>
    /// <remarks>
    /// Assistant messages contain the specialist's generated output, typically
    /// Python code in the "Everything is a Coder" architecture.
    /// </remarks>
    [JsonStringEnumMemberName("assistant")]
    Assistant,

    /// <summary>
    /// A message representing a tool call or tool result.
    /// </summary>
    /// <remarks>
    /// Tool messages are used when the assistant invokes MCP tools and receives
    /// results. They maintain the conversation context for multi-turn tool usage.
    /// </remarks>
    [JsonStringEnumMemberName("tool")]
    Tool
}