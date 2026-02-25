# Multi-Model Router: Intelligent Model Selection with Thompson Sampling

## The Problem: Which Model Should Answer This Query?

Your team has deployed multiple LLM models. GPT-4 excels at creative writing but costs $0.03 per 1K tokens. Claude-3 handles technical analysis well at $0.015 per 1K tokens. A local model runs for free but struggles with nuanced questions.

**The challenge**: Every request gets routed to the same model because no one knows which performs best for which task type. You're either:
- Overpaying by always using the expensive model, or
- Frustrating users by using cheap models for tasks they handle poorly

Worse, you have no systematic way to learn from experience. Even after 10,000 queries, you're still guessing.

**What you need**: A system that:
1. Learns which model performs best for each task category
2. Balances trying new combinations (exploration) with using proven ones (exploitation)
3. Improves automatically over time without manual tuning

This is exactly what Thompson Sampling provides.

---

## Learning Objectives

After working through this sample, you will understand:

- **The multi-armed bandit problem** and why it applies to model selection
- **Thompson Sampling** as an algorithm that naturally balances exploration and exploitation
- **Beta distributions** as representations of uncertainty about model performance
- **Workflow integration** of probabilistic agent selection
- **When to use** confidence-based fallbacks vs. trusting the selection

---

## Conceptual Foundation

### The Multi-Armed Bandit Problem

Imagine a casino with multiple slot machines ("one-armed bandits"). Each machine has an unknown payout probability. You have limited pulls. How do you maximize winnings?

- **Pure exploration**: Try each machine equally. You learn a lot but waste pulls on bad machines.
- **Pure exploitation**: Always use the machine that's paid out best so far. You might miss a better machine you haven't tried enough.

Model selection is the same problem:
- **Machines** = LLM models (GPT-4, Claude-3, local)
- **Pulls** = User queries
- **Payout** = User satisfaction (successful response)

### Why Thompson Sampling?

There are several approaches to the bandit problem:

| Approach | How It Works | Weakness |
|----------|--------------|----------|
| **Epsilon-Greedy** | Use best model 90% of time, random 10% | Wastes exploration on clearly bad models |
| **UCB (Upper Confidence Bound)** | Pick model with highest optimistic estimate | Deterministic—always picks same model given same state |
| **Thompson Sampling** | Sample from belief distribution, pick highest | Naturally reduces exploration as confidence grows |

**Thompson Sampling wins because**:
- Models with high uncertainty get more chances (might be hidden gems)
- Models with proven track records dominate over time
- Exploration decreases automatically as beliefs converge
- Randomness prevents deterministic exploitation of the system

### Beta Distributions: Representing Uncertainty

A Beta distribution represents uncertainty about a probability. It's parameterized by:
- **Alpha (α)**: Number of successes + prior
- **Beta (β)**: Number of failures + prior

```text
Model "gpt-4" for "Factual" queries:
  Beta(α=15, β=3) → 15 successes, 3 failures
  Mean = α/(α+β) = 15/18 = 83% success rate
  High confidence (18 total observations)

Model "local-model" for "Factual" queries:
  Beta(α=3, β=2) → 3 successes, 2 failures
  Mean = α/(α+β) = 3/5 = 60% success rate
  Low confidence (only 5 observations)
```

**The key insight**: When you sample from Beta(3, 2), you might get 0.8 by chance. When you sample from Beta(15, 3), you'll almost always get something near 0.83. This means uncertain models occasionally "win" the selection lottery, getting a chance to prove themselves.

### The Selection Algorithm

```text
For each query:
1. Classify the query → "Factual" / "Creative" / "Technical" / etc.
2. For each available model:
   a. Get the Beta(α, β) belief for this (model, category) pair
   b. Sample a random value θ from Beta(α, β)
3. Select the model with the highest sampled θ
4. After response, record success/failure → update beliefs
```

This is "optimistic uncertainty"—models we know less about get inflated chances because their samples have higher variance.

---

## Design Decisions

| Decision | Why This Approach | Alternative | Trade-off |
|----------|-------------------|-------------|-----------|
| **Beta distributions** | Conjugate prior for Bernoulli outcomes (success/fail) | Gaussian, categorical | Beta is simplest for binary outcomes |
| **Per-category beliefs** | Models perform differently on different tasks | Single belief per model | More parameters to learn, but better routing |
| **Prior α=2, β=2** | Uninformative—no bias toward success or failure | α=1, β=1 (uniform) | Slight regularization prevents extreme early beliefs |
| **Confidence fallback** | Low-confidence selections might be wrong | Always trust Thompson | Protects quality when uncertain |

### When to Use This Pattern

**Good fit when**:
- Multiple models/agents with overlapping capabilities
- Performance varies by task type or category
- You can collect outcome feedback (ratings, success/failure)
- Cost or latency varies between options

**Poor fit when**:
- Only one model available
- Performance is uniform across task types
- No feedback mechanism exists
- Selection latency matters more than quality

### Anti-Patterns to Avoid

| Anti-Pattern | Problem | Correct Approach |
|--------------|---------|------------------|
| **Hard-coded routing** | Never learns, never adapts | Thompson Sampling |
| **Ignoring feedback** | Beliefs never improve | Always call `RecordOutcome` |
| **Same prior for all** | Some models might deserve different starting beliefs | Customize priors based on known capabilities |
| **Infinite exploration** | Keeps trying bad models forever | Confidence threshold + fallback |

---

## Building the Workflow

### The Shape First

Before diving into implementation, understand the workflow structure:

```text
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  ClassifyQuery  │───▶│   SelectModel   │───▶│ GenerateResponse│───▶│ RecordFeedback  │
│                 │    │                 │    │                 │    │                 │
│ "What's 2+2?"   │    │ Thompson Sample │    │ Call selected   │    │ Update beliefs  │
│ → "Factual"     │    │ → "local-model" │    │ → "The answer   │    │ → Beta(α+1, β)  │
└─────────────────┘    └─────────────────┘    │    is 4."       │    └─────────────────┘
                                              └─────────────────┘
```

Each step has a single responsibility:
1. **ClassifyQuery**: Determine what KIND of task this is
2. **SelectModel**: Pick the best model for that task kind
3. **GenerateResponse**: Execute with the selected model
4. **RecordFeedback**: Learn from the outcome

### State: What We Track

```csharp
public sealed record RouterState : IWorkflowState
{
    // Identity
    public Guid WorkflowId { get; init; }

    // Input
    public string UserQuery { get; init; } = string.Empty;

    // Classification result (from ClassifyQuery)
    public QueryCategory Category { get; init; }

    // Selection result (from SelectModel)
    public string? SelectedModel { get; init; }
    public decimal Confidence { get; init; }

    // Response (from GenerateResponse)
    public string? Response { get; init; }

    // Feedback (for RecordFeedback)
    public UserFeedback? Feedback { get; init; }
}
```

**Why these fields?**
- `Category`: We need this to look up the right belief distribution
- `Confidence`: Enables fallback decisions and debugging
- `Feedback`: The learning signal that updates beliefs

### The Workflow Definition

```csharp
public static WorkflowDefinition<RouterState> Create() =>
    Workflow<RouterState>
        .Create("multi-model-router")
        .StartWith<ClassifyQuery>()   // What kind of task?
        .Then<SelectModel>()          // Which model for this task?
        .Then<GenerateResponse>()     // Generate the answer
        .Finally<RecordFeedback>();   // Learn from outcome
```

This reads as a sentence: "Create a multi-model-router workflow that starts by classifying the query, then selects a model, then generates a response, and finally records feedback."

### The Key Step: SelectModel

```csharp
public async Task<StepResult<RouterState>> ExecuteAsync(
    RouterState state,
    StepContext context,
    CancellationToken cancellationToken)
{
    // Build context for the selector
    var selectionContext = new AgentSelectionContext
    {
        WorkflowId = state.WorkflowId,
        TaskDescription = state.UserQuery,
        AvailableAgents = ["gpt-4", "claude-3", "local-model"],
    };

    // Thompson Sampling happens here
    var result = await _agentSelector.SelectAgentAsync(selectionContext, cancellationToken);

    // Fallback if confidence is too low
    if (result.Value.SelectionConfidence < _confidenceThreshold)
    {
        return state with { SelectedModel = "gpt-4", Confidence = 0m };
    }

    return state with
    {
        SelectedModel = result.Value.SelectedAgentId,
        Confidence = (decimal)result.Value.SelectionConfidence,
    };
}
```

**The confidence fallback**: When we're uncertain, we fall back to the expensive-but-reliable model. This protects quality while we're still learning.

---

## The "Aha Moment"

> **Uncertainty is opportunity, not risk.**
>
> A model with 2 successes and 0 failures isn't better than one with 100 successes and 10 failures—it just has less evidence. Thompson Sampling embraces this by giving uncertain models inflated chances to prove themselves.
>
> This is "optimistic uncertainty"—we're optimistic that unknown models might be great, so we give them chances proportional to our uncertainty.

---

## Running the Sample

```bash
dotnet run --project samples/MultiModelRouter
```

### What You'll See

```text
==============================================
  Multi-Model Router Sample
  Thompson Sampling for Model Selection
==============================================

--- Query 1/15 ---
Query: "What is the capital of France?"
Category: Factual
Selected Model: gpt-4 (Confidence: 0%)
Response: [GPT-4] Paris is the capital of France...
User Rating: 5/5
  -> Belief updated: gpt-4:General [SUCCESS] Alpha=3 Beta=2 Rate=60.0%

--- Query 2/15 ---
Query: "Write a haiku about programming"
Category: Creative
Selected Model: claude-3 (Confidence: 50%)
Response: [Claude-3] Code flows like water...
User Rating: 4/5
  -> Belief updated: claude-3:General [SUCCESS] Alpha=3 Beta=2 Rate=60.0%
```

Watch how:
- Early queries show low confidence (we're still learning)
- Models get selected based on sampled values, not just means
- Beliefs update after each feedback signal
- Over time, the "right" models win more often for their categories

---

## Extension Exercises

### Exercise 1: Add a New Task Category

The current categories are Factual, Creative, Technical, and Conversational. Add a "Code" category:

1. Update `QueryCategory` enum
2. Add keyword detection in `ClassifyQuery`
3. Run and observe how beliefs develop for the new category

### Exercise 2: Implement Confidence Logging

Track and visualize how confidence changes over time:

1. Add a `ConfidenceHistory` collection to state (with `[Append]`)
2. Record each selection's confidence
3. Plot the convergence over queries

### Exercise 3: A/B Testing Mode

Implement a mode that forces exploration for A/B testing:

1. Add an `ExplorationMode` flag to state
2. When enabled, ignore Thompson Sampling and round-robin models
3. Compare belief convergence rates

---

## Running Tests

```bash
dotnet test samples/MultiModelRouter.Tests
```

The tests verify:
- Classification correctness for different query types
- Thompson Sampling selection behavior
- Belief updating after feedback
- Confidence fallback logic

---

## Key Takeaways

1. **Thompson Sampling balances exploration and exploitation automatically**—no tuning required
2. **Beta distributions represent uncertainty**—more data = tighter distribution = less exploration
3. **Per-category beliefs** allow specialized routing—creative queries go to creative models
4. **Confidence fallback** protects quality while learning—don't trust uncertain selections for important queries
5. **Feedback is essential**—without `RecordFeedback`, beliefs never improve

---

## Learn More

- [Thompson Sampling Pattern](../../docs/examples/thompson-sampling.md) - Detailed algorithm documentation
- [IAgentSelector Interface](../../src/Strategos/Abstractions/IAgentSelector.cs) - The abstraction this sample implements
- [Multi-Armed Bandits (Wikipedia)](https://en.wikipedia.org/wiki/Multi-armed_bandit) - Mathematical background
