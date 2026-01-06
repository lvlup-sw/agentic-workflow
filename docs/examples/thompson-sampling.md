# Thompson Sampling Example

This example demonstrates intelligent agent selection using Thompson Sampling, a contextual multi-armed bandit algorithm.

## Overview

Thompson Sampling enables online learning of agent performance across different task categories. The algorithm balances exploration (trying different agents) with exploitation (using known-good agents) to optimize task outcomes over time.

## Concept

Each agent maintains a Beta distribution belief for each task category:

```
Agent "analyst" for "Analysis" tasks:
  Beta(alpha=15, beta=3)  ->  High success rate, confident

Agent "coder" for "Coding" tasks:
  Beta(alpha=8, beta=2)   ->  Good success rate, less experience
```

When selecting an agent:
1. Sample a value from each agent's Beta distribution for the task category
2. Select the agent with the highest sampled value
3. After execution, update the belief based on outcome

## Setup

### Configure Services

```csharp
services.AddAgentSelection(options => options
    .WithPrior(alpha: 2, beta: 2)  // Uninformative prior
    .WithCategories(
        TaskCategory.Analysis,
        TaskCategory.Coding,
        TaskCategory.Research,
        TaskCategory.Writing));
```

### Register Agents

```csharp
services.AddAgent("analyst", new AgentConfig
{
    Name = "Data Analyst",
    Capabilities = ["data-analysis", "visualization", "statistics"]
});

services.AddAgent("coder", new AgentConfig
{
    Name = "Software Developer",
    Capabilities = ["code-generation", "debugging", "refactoring"]
});

services.AddAgent("researcher", new AgentConfig
{
    Name = "Research Specialist",
    Capabilities = ["literature-review", "synthesis", "citations"]
});
```

## Basic Usage

### Select Agent for Task

```csharp
public class TaskRouter
{
    private readonly IAgentSelector _selector;
    private readonly IAgentRegistry _agents;

    public TaskRouter(IAgentSelector selector, IAgentRegistry agents)
    {
        _selector = selector;
        _agents = agents;
    }

    public async Task<AgentResult> RouteTaskAsync(
        string taskDescription,
        CancellationToken ct)
    {
        // 1. Select agent via Thompson Sampling
        var selection = await _selector.SelectAgentAsync(new AgentSelectionContext
        {
            AvailableAgentIds = ["analyst", "coder", "researcher"],
            TaskDescription = taskDescription
        }, ct);

        // 2. Execute with selected agent
        var agent = _agents.Get(selection.SelectedAgentId);
        var result = await agent.ExecuteAsync(taskDescription, ct);

        // 3. Record outcome for learning
        await _selector.RecordOutcomeAsync(
            selection.SelectedAgentId,
            selection.TaskCategory,
            result.Success
                ? AgentOutcome.Succeeded(result.ConfidenceScore)
                : AgentOutcome.Failed(result.ErrorMessage),
            ct);

        return result;
    }
}
```

## Task Categories

The library includes 7 predefined task categories:

| Category | Keywords | Example Tasks |
|----------|----------|---------------|
| Analysis | analyze, examine, evaluate, assess | "Analyze sales trends" |
| Coding | code, implement, debug, refactor | "Implement OAuth flow" |
| Research | research, investigate, explore | "Research competitor pricing" |
| Writing | write, draft, compose, document | "Write API documentation" |
| Data | data, transform, migrate, etl | "Transform CSV to JSON" |
| Integration | integrate, connect, api, webhook | "Connect Stripe API" |
| General | (fallback) | "Help with this task" |

### Custom Category Classification

```csharp
public class DomainFeatureExtractor : ITaskFeatureExtractor
{
    public TaskFeatures Extract(string taskDescription)
    {
        var lower = taskDescription.ToLowerInvariant();

        // Domain-specific classification
        if (lower.Contains("compliance") || lower.Contains("regulation"))
        {
            return new TaskFeatures(
                Category: TaskCategory.Analysis,
                Complexity: TaskComplexity.High,
                MatchedKeywords: ["compliance", "regulation"]);
        }

        if (lower.Contains("migration") || lower.Contains("upgrade"))
        {
            return new TaskFeatures(
                Category: TaskCategory.Data,
                Complexity: TaskComplexity.High,
                MatchedKeywords: ["migration"]);
        }

        // Fall back to default extraction
        return DefaultFeatureExtractor.Extract(taskDescription);
    }
}

// Register custom extractor
services.AddSingleton<ITaskFeatureExtractor, DomainFeatureExtractor>();
```

## Belief Persistence

### In-Memory (Development)

```csharp
services.AddSingleton<IBeliefStore, InMemoryBeliefStore>();
```

### PostgreSQL (Production)

```csharp
services.AddSingleton<IBeliefStore, PostgresBeliefStore>(sp =>
    new PostgresBeliefStore(sp.GetRequiredService<IDocumentSession>()));
```

### Belief Structure

```csharp
public record AgentBelief(
    string AgentId,
    TaskCategory Category,
    int Alpha,      // Success count + prior
    int Beta,       // Failure count + prior
    int TotalTrials,
    DateTimeOffset LastUpdated);

// Example beliefs after training:
// Agent: analyst
//   Analysis: Beta(45, 5)   = 90% success, 50 trials
//   Coding:   Beta(3, 7)    = 30% success, 10 trials
//   Research: Beta(20, 4)   = 83% success, 24 trials
```

## Selection Algorithm

```csharp
public class ThompsonSamplingSelector : IAgentSelector
{
    public async Task<AgentSelection> SelectAgentAsync(
        AgentSelectionContext context,
        CancellationToken ct)
    {
        // 1. Classify task
        var features = _featureExtractor.Extract(context.TaskDescription);

        // 2. Get beliefs for all available agents
        var beliefs = await _beliefStore.GetBeliefsAsync(
            context.AvailableAgentIds,
            features.Category,
            ct);

        // 3. Sample from each agent's Beta distribution
        var samples = beliefs.Select(b => new
        {
            b.AgentId,
            Sample = SampleBeta(b.Alpha, b.Beta)
        });

        // 4. Select agent with highest sample
        var selected = samples.MaxBy(s => s.Sample)!;

        return new AgentSelection(
            SelectedAgentId: selected.AgentId,
            TaskCategory: features.Category,
            SampledValue: selected.Sample,
            Features: features);
    }

    private double SampleBeta(int alpha, int beta)
    {
        // Beta distribution sampling
        return BetaDistribution.Sample(_random, alpha, beta);
    }
}
```

## Recording Outcomes

```csharp
// Success with confidence score
await selector.RecordOutcomeAsync(
    agentId: "analyst",
    category: TaskCategory.Analysis,
    outcome: AgentOutcome.Succeeded(confidenceScore: 0.92),
    ct);

// Failure with reason
await selector.RecordOutcomeAsync(
    agentId: "coder",
    category: TaskCategory.Coding,
    outcome: AgentOutcome.Failed("Syntax errors in generated code"),
    ct);

// Partial success
await selector.RecordOutcomeAsync(
    agentId: "researcher",
    category: TaskCategory.Research,
    outcome: AgentOutcome.Partial(completionRate: 0.7),
    ct);
```

## Integration with Workflows

```csharp
public class DelegateToAgent : IWorkflowStep<TaskState>
{
    private readonly IAgentSelector _selector;
    private readonly IAgentRegistry _agents;

    public async Task<StepResult<TaskState>> ExecuteAsync(
        TaskState state,
        StepContext context,
        CancellationToken ct)
    {
        // Select best agent for the task
        var selection = await _selector.SelectAgentAsync(new AgentSelectionContext
        {
            AvailableAgentIds = state.AvailableAgents,
            TaskDescription = state.CurrentTask.Description
        }, ct);

        // Execute task
        var agent = _agents.Get(selection.SelectedAgentId);
        var result = await agent.ExecuteAsync(state.CurrentTask, ct);

        // Record outcome for learning
        await _selector.RecordOutcomeAsync(
            selection.SelectedAgentId,
            selection.TaskCategory,
            result.ToOutcome(),
            ct);

        return state
            .With(s => s.SelectedAgent, selection.SelectedAgentId)
            .With(s => s.TaskResult, result)
            .AsResult();
    }
}
```

## Monitoring Selection Performance

```csharp
// Query agent performance by category
var performance = await beliefStore.GetPerformanceReportAsync(ct);

// Output:
// Agent: analyst
//   Analysis: 90.0% (45/50 trials)
//   Research: 83.3% (20/24 trials)
//   Coding:   30.0% (3/10 trials)
//
// Agent: coder
//   Coding:   88.0% (44/50 trials)
//   Analysis: 45.0% (9/20 trials)
```

## Key Points

- Thompson Sampling balances exploration vs. exploitation automatically
- Beliefs update after each task execution
- Prior (alpha=2, beta=2) provides uninformative starting point
- Task classification happens via keyword extraction
- Custom feature extractors enable domain-specific classification
- Beliefs persist across application restarts
- Performance improves as more tasks are executed
- Works with any number of agents and categories
