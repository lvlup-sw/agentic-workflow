# Iterative Refinement Example

This example demonstrates loop-based workflows using `RepeatUntil` for quality-driven iteration.

## Overview

Iterative refinement workflows repeat a sequence of steps until a quality threshold is met or a maximum iteration count is reached. This pattern is common in AI workflows where output quality improves through successive refinement.

## State Definition

```csharp
[WorkflowState]
public record RefinementState : IWorkflowState
{
    public Guid WorkflowId { get; init; }
    public string Topic { get; init; } = string.Empty;
    public string? CurrentDraft { get; init; }
    public decimal QualityScore { get; init; }
    public int IterationCount { get; init; }

    [Append]
    public ImmutableList<CritiqueResult> CritiqueHistory { get; init; } = [];

    [Append]
    public ImmutableList<RefinementAttempt> RefinementHistory { get; init; } = [];

    public string? FinalContent { get; init; }
}

public record CritiqueResult(
    decimal Score,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses,
    IReadOnlyList<string> Suggestions);

public record RefinementAttempt(
    int Iteration,
    string BeforeContent,
    string AfterContent,
    IReadOnlyList<string> ChangesApplied);
```

## Workflow Definition

```csharp
var workflow = Workflow<RefinementState>
    .Create("iterative-refinement")
    .StartWith<GenerateDraft>()
    .RepeatUntil(
        condition: state => state.QualityScore >= 0.9m,
        maxIterations: 5,
        body: flow => flow
            .Then<Critique>()
            .Then<Refine>())
    .Finally<Publish>();
```

The loop continues until either:
1. `QualityScore >= 0.9` (quality threshold met)
2. 5 iterations have been completed (maximum reached)

## Loop Patterns

### Quality Threshold

```csharp
.RepeatUntil(
    condition: state => state.QualityScore >= 0.9m,
    maxIterations: 10,
    body: flow => flow
        .Then<Evaluate>()
        .Then<Improve>())
```

### Convergence Detection

```csharp
.RepeatUntil(
    condition: state => state.ImprovementDelta < 0.01m,  // Converged
    maxIterations: 20,
    body: flow => flow
        .Then<Iterate>()
        .Then<MeasureDelta>())
```

### Error Correction

```csharp
.RepeatUntil(
    condition: state => state.Errors.Count == 0,
    maxIterations: 3,
    body: flow => flow
        .Then<Validate>()
        .Then<FixErrors>())
```

### Multi-Step Loop Body

```csharp
.RepeatUntil(
    condition: state => state.AllTestsPassing,
    maxIterations: 5,
    body: flow => flow
        .Then<GenerateCode>()
        .Then<RunTests>()
        .Then<AnalyzeFailures>()
        .Then<RefineCode>())
```

## Step Implementations

### GenerateDraft

```csharp
public class GenerateDraft : IWorkflowStep<RefinementState>
{
    private readonly IContentGenerator _generator;

    public GenerateDraft(IContentGenerator generator)
    {
        _generator = generator;
    }

    public async Task<StepResult<RefinementState>> ExecuteAsync(
        RefinementState state,
        StepContext context,
        CancellationToken ct)
    {
        var draft = await _generator.GenerateAsync(state.Topic, ct);

        return state
            .With(s => s.CurrentDraft, draft)
            .With(s => s.IterationCount, 0)
            .AsResult();
    }
}
```

### Critique

```csharp
public class Critique : IWorkflowStep<RefinementState>
{
    private readonly IContentCritic _critic;

    public Critique(IContentCritic critic)
    {
        _critic = critic;
    }

    public async Task<StepResult<RefinementState>> ExecuteAsync(
        RefinementState state,
        StepContext context,
        CancellationToken ct)
    {
        var critique = await _critic.CritiqueAsync(state.CurrentDraft!, ct);

        return state
            .With(s => s.QualityScore, critique.Score)
            .With(s => s.CritiqueHistory, state.CritiqueHistory.Add(critique))
            .AsResult();
    }
}
```

### Refine

```csharp
public class Refine : IWorkflowStep<RefinementState>
{
    private readonly IContentRefiner _refiner;

    public Refine(IContentRefiner refiner)
    {
        _refiner = refiner;
    }

    public async Task<StepResult<RefinementState>> ExecuteAsync(
        RefinementState state,
        StepContext context,
        CancellationToken ct)
    {
        var latestCritique = state.CritiqueHistory[^1];

        var refinedContent = await _refiner.RefineAsync(
            state.CurrentDraft!,
            latestCritique.Suggestions,
            ct);

        var attempt = new RefinementAttempt(
            Iteration: state.IterationCount + 1,
            BeforeContent: state.CurrentDraft!,
            AfterContent: refinedContent,
            ChangesApplied: latestCritique.Suggestions);

        return state
            .With(s => s.CurrentDraft, refinedContent)
            .With(s => s.IterationCount, state.IterationCount + 1)
            .With(s => s.RefinementHistory, state.RefinementHistory.Add(attempt))
            .AsResult();
    }
}
```

### Publish

```csharp
public class Publish : IWorkflowStep<RefinementState>
{
    private readonly IPublisher _publisher;

    public Publish(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<StepResult<RefinementState>> ExecuteAsync(
        RefinementState state,
        StepContext context,
        CancellationToken ct)
    {
        await _publisher.PublishAsync(state.CurrentDraft!, ct);

        return state
            .With(s => s.FinalContent, state.CurrentDraft)
            .AsResult();
    }
}
```

## Generated Phase Enum

Loop steps are prefixed with the loop name for uniqueness:

```csharp
public enum IterativeRefinementPhase
{
    NotStarted,
    GenerateDraft,
    Refinement_Critique,      // Loop "Refinement" contains "Critique"
    Refinement_Refine,        // Loop "Refinement" contains "Refine"
    Publish,
    Completed,
    Failed
}
```

## Loop Control Flow

The generated saga handles loop logic:

```csharp
// After Refine step completes
public async Task<object> Handle(
    ExecuteRefinement_RefineCommand command,
    Refine step,
    CancellationToken ct)
{
    var result = await step.ExecuteAsync(State, ct);
    State = RefinementStateReducer.Reduce(State, result.StateUpdate);

    // Check loop condition
    if (State.QualityScore >= 0.9m)
    {
        // Exit loop - proceed to Publish
        return new ExecutePublishCommand(WorkflowId);
    }

    if (State.IterationCount >= 5)
    {
        // Max iterations reached - proceed to Publish
        return new ExecutePublishCommand(WorkflowId);
    }

    // Continue loop - back to Critique
    return new ExecuteRefinement_CritiqueCommand(WorkflowId);
}
```

## Tracking Progress

The `[Append]` reducer attribute accumulates history across iterations:

```csharp
// After 3 iterations, CritiqueHistory contains:
// [Critique1, Critique2, Critique3]

// RefinementHistory contains:
// [Attempt1, Attempt2, Attempt3]
```

## Nested Loops

Loops can be nested:

```csharp
.RepeatUntil(
    condition: state => state.ChapterCount >= 5,
    maxIterations: 10,
    body: flow => flow
        .Then<GenerateChapter>()
        .RepeatUntil(
            condition: state => state.ChapterQuality >= 0.8m,
            maxIterations: 3,
            body: inner => inner
                .Then<CritiqueChapter>()
                .Then<RefineChapter>()))
```

Phase names preserve the hierarchy:

```csharp
public enum BookWritingPhase
{
    Chapters_GenerateChapter,
    Chapters_ChapterRefinement_CritiqueChapter,
    Chapters_ChapterRefinement_RefineChapter,
    // ...
}
```

## Key Points

- `RepeatUntil` continues until condition is true OR max iterations reached
- Loop body can contain multiple steps
- Use `[Append]` reducers to accumulate history across iterations
- Phase names include loop prefix for uniqueness
- Built-in protection against infinite loops via `maxIterations`
- Loops can be nested for complex refinement patterns
- Each iteration is persisted, enabling recovery mid-loop
