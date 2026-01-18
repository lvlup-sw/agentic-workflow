// =============================================================================
// <copyright file="ClassifyQuery.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Steps;
using MultiModelRouter.State;

namespace MultiModelRouter.Steps;

/// <summary>
/// Classifies user queries into categories for model routing.
/// </summary>
/// <remarks>
/// <para>
/// This step analyzes the user query and categorizes it to help
/// the model selector choose the most appropriate model:
/// <list type="bullet">
///   <item><description>Factual: Questions about facts, definitions, data</description></item>
///   <item><description>Creative: Requests for creative writing, stories, poems</description></item>
///   <item><description>Technical: Programming, algorithms, technical explanations</description></item>
///   <item><description>Conversational: Greetings, small talk, casual chat</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class ClassifyQuery : IWorkflowStep<RouterState>
{
    private static readonly string[] FactualKeywords =
    [
        "what is", "what are", "who is", "when did", "where is", "how many",
        "capital", "population", "definition", "fact", "history", "date",
    ];

    private static readonly string[] CreativeKeywords =
    [
        "write", "create", "compose", "poem", "story", "imagine",
        "fiction", "creative", "invent", "design",
    ];

    private static readonly string[] TechnicalKeywords =
    [
        "implement", "algorithm", "code", "debug", "function", "class",
        "programming", "technical", "software", "api", "database", "binary",
    ];

    private static readonly string[] ConversationalKeywords =
    [
        "how are you", "hello", "hi ", "hey", "what's up", "good morning",
        "good evening", "nice to meet", "today",
    ];

    /// <inheritdoc/>
    public Task<StepResult<RouterState>> ExecuteAsync(
        RouterState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        var query = state.UserQuery.ToLowerInvariant();
        var category = ClassifyByKeywords(query);

        var updatedState = state with { Category = category };
        return Task.FromResult(StepResult<RouterState>.FromState(updatedState));
    }

    private static QueryCategory ClassifyByKeywords(string query)
    {
        // Check conversational first (most specific greeting patterns)
        if (ContainsAny(query, ConversationalKeywords))
        {
            return QueryCategory.Conversational;
        }

        // Check factual (common question patterns)
        if (ContainsAny(query, FactualKeywords))
        {
            return QueryCategory.Factual;
        }

        // Check creative
        if (ContainsAny(query, CreativeKeywords))
        {
            return QueryCategory.Creative;
        }

        // Check technical
        if (ContainsAny(query, TechnicalKeywords))
        {
            return QueryCategory.Technical;
        }

        // Default to factual for general knowledge questions
        return QueryCategory.Factual;
    }

    private static bool ContainsAny(string text, string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

