# Multi-Model Router Sample

This sample demonstrates intelligent model selection using Thompson Sampling within an Agentic.Workflow pipeline.

## Overview

The Multi-Model Router shows how to:

- **Thompson Sampling** - Learn optimal model selection over time by balancing exploration (trying different models) with exploitation (using known-good models)
- **IAgentSelector** - Use the multi-armed bandit pattern for model choice
- **Confidence Routing** - Fall back to expensive models when cheap models are uncertain
- **Audit Trail** - Track which model answered, confidence scores, and user feedback

## Workflow Architecture

```text
Classify Query -> Select Model (Thompson Sampling) -> Generate Response -> Record Feedback
                         |
              [GPT-4 | Claude-3 | Local Model]
```

1. **ClassifyQuery** - Categorizes user input (Factual, Creative, Technical, Conversational)
2. **SelectModel** - Uses Thompson Sampling to pick the optimal model for the category
3. **GenerateResponse** - Generates a response using the selected model
4. **RecordFeedback** - Updates beliefs based on user rating (enables learning)

## How Thompson Sampling Works

Each model maintains a Beta distribution for each task category:

```text
Model "gpt-4" for "Factual" tasks:
  Beta(alpha=15, beta=3)  ->  High success rate, confident

Model "local-model" for "Technical" tasks:
  Beta(alpha=5, beta=8)   ->  Lower success rate, needs exploration
```

When selecting a model:

1. Sample theta from each model's Beta distribution for the task category
2. Select the model with the highest sampled theta
3. After execution, update beliefs based on user feedback

This naturally balances:
- **Exploration** - Trying uncertain models to gather data
- **Exploitation** - Using models with proven track records

## Running the Sample

```bash
dotnet run --project samples/MultiModelRouter
```

### Sample Output

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
  -> Belief updated: gpt-4:Factual [SUCCESS] Alpha=3 Beta=2 Rate=60.0%
```

## Key Components

### State (RouterState)

```csharp
public sealed record RouterState : IWorkflowState
{
    public Guid WorkflowId { get; init; }
    public string UserQuery { get; init; }
    public QueryCategory Category { get; init; }
    public string SelectedModel { get; init; }
    public string Response { get; init; }
    public decimal Confidence { get; init; }
    public UserFeedback? Feedback { get; init; }
}
```

### Workflow Definition

```csharp
public static class RouterWorkflow
{
    public static WorkflowDefinition<RouterState> Create() =>
        Workflow<RouterState>
            .Create("multi-model-router")
            .StartWith<ClassifyQuery>()
            .Then<SelectModel>()
            .Then<GenerateResponse>()
            .Finally<RecordFeedback>();
}
```

### Confidence-Based Fallback

The SelectModel step falls back to GPT-4 when confidence is low:

```csharp
if (confidence < _confidenceThreshold)
{
    return state with { SelectedModel = FallbackModel };
}
```

## Running Tests

```bash
dotnet test samples/MultiModelRouter.Tests
```

## Learning More

- See `docs/examples/thompson-sampling.md` for detailed Thompson Sampling documentation
- See `src/Agentic.Workflow/Abstractions/IAgentSelector.cs` for the agent selection interface
- See `src/Agentic.Workflow.Infrastructure/Selection/ThompsonSamplingAgentSelector.cs` for production implementation
