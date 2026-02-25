// =============================================================================
// <copyright file="Program.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Selection;
using Strategos.Steps;
using MultiModelRouter;
using MultiModelRouter.Services;
using MultiModelRouter.State;
using MultiModelRouter.Steps;

Console.WriteLine("==============================================");
Console.WriteLine("  Multi-Model Router Sample");
Console.WriteLine("  Thompson Sampling for Model Selection");
Console.WriteLine("==============================================");
Console.WriteLine();

// Create services with fixed seed for reproducibility
var agentSelector = new MockAgentSelector(priorAlpha: 2, priorBeta: 2, seed: 42);
var modelProvider = new MockModelProvider(seed: 42);

// Subscribe to belief updates for visualization
agentSelector.BeliefUpdated += (agentId, category, success, alpha, beta) =>
{
    var rate = (double)alpha / (alpha + beta) * 100;
    var outcome = success ? "SUCCESS" : "FAILURE";
    Console.WriteLine($"  -> Belief updated: {agentId}:{category} [{outcome}] Alpha={alpha} Beta={beta} Rate={rate:F1}%");
};

// Create workflow steps
var classifyQuery = new ClassifyQuery();
var selectModel = new SelectModel(agentSelector, confidenceThreshold: 0.3m);
var generateResponse = new GenerateResponse(modelProvider);
var recordFeedback = new RecordFeedback(agentSelector);

// Sample queries representing different categories
var queries = new[]
{
    ("What is the capital of France?", QueryCategory.Factual, 5),
    ("Write a poem about the ocean", QueryCategory.Creative, 4),
    ("Explain how to implement a binary search algorithm", QueryCategory.Technical, 5),
    ("How are you doing today?", QueryCategory.Conversational, 4),
    ("What is the population of Tokyo?", QueryCategory.Factual, 3),
    ("Create a short story about a robot", QueryCategory.Creative, 5),
    ("What is machine learning?", QueryCategory.Factual, 4),
    ("Debug this code issue", QueryCategory.Technical, 2),
    ("What year did World War II end?", QueryCategory.Factual, 5),
    ("Write a haiku about spring", QueryCategory.Creative, 4),
    ("Implement a sorting algorithm", QueryCategory.Technical, 5),
    ("Hello, nice to meet you!", QueryCategory.Conversational, 5),
    ("What is the speed of light?", QueryCategory.Factual, 4),
    ("Compose a limerick", QueryCategory.Creative, 3),
    ("How does quicksort work?", QueryCategory.Technical, 5),
};

Console.WriteLine("Running workflow for sample queries...");
Console.WriteLine("(Each query simulates user feedback to update Thompson Sampling beliefs)");
Console.WriteLine();

var modelStats = new Dictionary<string, int>();

for (int i = 0; i < queries.Length; i++)
{
    var (query, expectedCategory, rating) = queries[i];

    Console.WriteLine($"--- Query {i + 1}/{queries.Length} ---");
    Console.WriteLine($"Query: \"{query}\"");

    // Create initial state
    var workflowId = Guid.NewGuid();
    var state = new RouterState
    {
        WorkflowId = workflowId,
        UserQuery = query,
    };

    // Step 1: Classify Query
    var context = StepContext.Create(workflowId, nameof(ClassifyQuery), "Classify");
    var result = await classifyQuery.ExecuteAsync(state, context, CancellationToken.None);
    state = result.UpdatedState;
    Console.WriteLine($"Category: {state.Category}");

    // Step 2: Select Model
    context = StepContext.Create(workflowId, nameof(SelectModel), "Select");
    result = await selectModel.ExecuteAsync(state, context, CancellationToken.None);
    state = result.UpdatedState;
    Console.WriteLine($"Selected Model: {state.SelectedModel} (Confidence: {state.Confidence:P0})");

    // Track model usage
    if (!modelStats.TryGetValue(state.SelectedModel, out var count))
    {
        count = 0;
    }

    modelStats[state.SelectedModel] = count + 1;

    // Step 3: Generate Response
    context = StepContext.Create(workflowId, nameof(GenerateResponse), "Generate");
    result = await generateResponse.ExecuteAsync(state, context, CancellationToken.None);
    state = result.UpdatedState;
    Console.WriteLine($"Response: {state.Response[..Math.Min(60, state.Response.Length)]}...");

    // Simulate user feedback
    var feedback = new UserFeedback(rating, rating >= 4 ? "Good!" : "Could be better", DateTimeOffset.UtcNow);
    state = state with { Feedback = feedback };
    Console.WriteLine($"User Rating: {rating}/5");

    // Step 4: Record Feedback
    context = StepContext.Create(workflowId, nameof(RecordFeedback), "Record");
    result = await recordFeedback.ExecuteAsync(state, context, CancellationToken.None);

    Console.WriteLine();
}

// Print final statistics
Console.WriteLine("==============================================");
Console.WriteLine("  Final Statistics");
Console.WriteLine("==============================================");
Console.WriteLine();

Console.WriteLine("Model Selection Distribution:");
foreach (var (model, count) in modelStats.OrderByDescending(kvp => kvp.Value))
{
    var pct = (double)count / queries.Length * 100;
    Console.WriteLine($"  {model}: {count} times ({pct:F1}%)");
}

Console.WriteLine();
Console.WriteLine("Final Belief States (Success Rates):");
var beliefs = agentSelector.GetBeliefs();
var models = new[] { "gpt-4", "claude-3", "local-model" };
var categories = Enum.GetNames<TaskCategory>();

foreach (var model in models)
{
    Console.WriteLine($"\n  {model}:");
    foreach (var category in categories)
    {
        var key = $"{model}:{category}";
        if (beliefs.TryGetValue(key, out var belief))
        {
            var rate = (double)belief.Alpha / (belief.Alpha + belief.Beta) * 100;
            var observations = belief.Alpha + belief.Beta - 4; // Subtract prior
            Console.WriteLine($"    {category}: {rate:F1}% ({observations} observations)");
        }
    }
}

Console.WriteLine();
Console.WriteLine("Thompson Sampling learns over time which models perform best for each category.");
Console.WriteLine("Run multiple times to see how initial randomness affects exploration/exploitation.");
