# Fork/Join Example

This example demonstrates parallel execution using `Fork` and `Join` for concurrent workflow paths.

## Overview

Fork executes multiple paths concurrently. Join synchronizes the results, merging state from all paths before continuing. This pattern is useful when independent analyses or operations can run in parallel to reduce total execution time.

## State Definition

```csharp
[WorkflowState]
public record AnalysisState : IWorkflowState
{
    public Guid WorkflowId { get; init; }
    public Company Company { get; init; } = null!;
    public MarketData? MarketData { get; init; }
    public FinancialAnalysis? FinancialAnalysis { get; init; }
    public TechnicalAnalysis? TechnicalAnalysis { get; init; }
    public MarketAnalysis? MarketAnalysis { get; init; }
    public SynthesizedReport? Report { get; init; }
    public string? FinalReport { get; init; }
}

public record Company(string Ticker, string Name, string Sector);

public record MarketData(
    decimal CurrentPrice,
    decimal Volume,
    IReadOnlyList<decimal> HistoricalPrices);

public record FinancialAnalysis(
    decimal RevenueGrowth,
    decimal ProfitMargin,
    decimal DebtToEquity,
    string Outlook);

public record TechnicalAnalysis(
    string Trend,
    decimal SupportLevel,
    decimal ResistanceLevel,
    IReadOnlyList<string> Signals);

public record MarketAnalysis(
    string SectorOutlook,
    IReadOnlyList<string> Competitors,
    decimal MarketShare,
    string CompetitivePosition);

public record SynthesizedReport(
    string Recommendation,
    decimal TargetPrice,
    string Rationale,
    IReadOnlyList<string> KeyRisks);
```

## Workflow Definition

```csharp
var workflow = Workflow<AnalysisState>
    .Create("comprehensive-analysis")
    .StartWith<GatherData>()
    .Fork(
        flow => flow.Then<FinancialAnalysisStep>(),
        flow => flow.Then<TechnicalAnalysisStep>(),
        flow => flow.Then<MarketAnalysisStep>())
    .Join<SynthesizeResults>()
    .Finally<GenerateReport>();
```

All three analysis steps execute in parallel. The `Join<SynthesizeResults>` step waits for all paths to complete, then executes with the merged state.

## Step Implementations

### GatherData

```csharp
public class GatherData : IWorkflowStep<AnalysisState>
{
    private readonly IMarketDataService _marketData;

    public GatherData(IMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public async Task<StepResult<AnalysisState>> ExecuteAsync(
        AnalysisState state,
        StepContext context,
        CancellationToken ct)
    {
        var data = await _marketData.GetDataAsync(state.Company.Ticker, ct);

        return state
            .With(s => s.MarketData, data)
            .AsResult();
    }
}
```

### FinancialAnalysisStep

```csharp
public class FinancialAnalysisStep : IWorkflowStep<AnalysisState>
{
    private readonly IFinancialAnalyzer _analyzer;

    public FinancialAnalysisStep(IFinancialAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    public async Task<StepResult<AnalysisState>> ExecuteAsync(
        AnalysisState state,
        StepContext context,
        CancellationToken ct)
    {
        var analysis = await _analyzer.AnalyzeAsync(
            state.Company,
            state.MarketData!,
            ct);

        return state
            .With(s => s.FinancialAnalysis, analysis)
            .AsResult();
    }
}
```

### TechnicalAnalysisStep

```csharp
public class TechnicalAnalysisStep : IWorkflowStep<AnalysisState>
{
    private readonly ITechnicalAnalyzer _analyzer;

    public TechnicalAnalysisStep(ITechnicalAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    public async Task<StepResult<AnalysisState>> ExecuteAsync(
        AnalysisState state,
        StepContext context,
        CancellationToken ct)
    {
        var analysis = await _analyzer.AnalyzeAsync(
            state.Company.Ticker,
            state.MarketData!.HistoricalPrices,
            ct);

        return state
            .With(s => s.TechnicalAnalysis, analysis)
            .AsResult();
    }
}
```

### MarketAnalysisStep

```csharp
public class MarketAnalysisStep : IWorkflowStep<AnalysisState>
{
    private readonly IMarketAnalyzer _analyzer;

    public MarketAnalysisStep(IMarketAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    public async Task<StepResult<AnalysisState>> ExecuteAsync(
        AnalysisState state,
        StepContext context,
        CancellationToken ct)
    {
        var analysis = await _analyzer.AnalyzeAsync(
            state.Company.Sector,
            state.Company.Ticker,
            ct);

        return state
            .With(s => s.MarketAnalysis, analysis)
            .AsResult();
    }
}
```

### SynthesizeResults

```csharp
public class SynthesizeResults : IWorkflowStep<AnalysisState>
{
    private readonly IReportSynthesizer _synthesizer;

    public SynthesizeResults(IReportSynthesizer synthesizer)
    {
        _synthesizer = synthesizer;
    }

    public async Task<StepResult<AnalysisState>> ExecuteAsync(
        AnalysisState state,
        StepContext context,
        CancellationToken ct)
    {
        // All three analyses are available here after Join
        var report = await _synthesizer.SynthesizeAsync(
            state.FinancialAnalysis!,
            state.TechnicalAnalysis!,
            state.MarketAnalysis!,
            ct);

        return state
            .With(s => s.Report, report)
            .AsResult();
    }
}
```

### GenerateReport

```csharp
public class GenerateReport : IWorkflowStep<AnalysisState>
{
    private readonly IReportGenerator _generator;

    public GenerateReport(IReportGenerator generator)
    {
        _generator = generator;
    }

    public async Task<StepResult<AnalysisState>> ExecuteAsync(
        AnalysisState state,
        StepContext context,
        CancellationToken ct)
    {
        var markdown = await _generator.GenerateMarkdownAsync(
            state.Company,
            state.Report!,
            ct);

        return state
            .With(s => s.FinalReport, markdown)
            .AsResult();
    }
}
```

## State Merging

When fork paths complete, their states are merged using reducer semantics:

```csharp
[WorkflowState]
public record AnalysisState : IWorkflowState
{
    // Scalar properties use Overwrite (default) - last value wins
    public FinancialAnalysis? FinancialAnalysis { get; init; }
    public TechnicalAnalysis? TechnicalAnalysis { get; init; }
    public MarketAnalysis? MarketAnalysis { get; init; }

    // Collection properties can use [Append] for accumulation
    [Append]
    public ImmutableList<AnalysisWarning> Warnings { get; init; } = [];
}
```

Since each fork path sets a different property, there are no conflicts. If multiple paths set the same property, the last one wins (overwrite semantics).

## Instance Names for Duplicate Steps

If you need the same step type in multiple fork paths, use instance names:

```csharp
.Fork(
    flow => flow.Then<AnalyzeStep>("Technical"),
    flow => flow.Then<AnalyzeStep>("Fundamental"))
.Join<SynthesizeStep>()
```

This generates distinct phases (`Technical`, `Fundamental`) but shares the step handler.

## Generated Artifacts

### Phase Enum

```csharp
public enum ComprehensiveAnalysisPhase
{
    NotStarted,
    GatherData,
    FinancialAnalysisStep,
    TechnicalAnalysisStep,
    MarketAnalysisStep,
    SynthesizeResults,
    GenerateReport,
    Completed,
    Failed
}
```

### Saga Fork Handler

```csharp
// Generated handler for GatherData - cascades to all fork paths
public async Task<object[]> Handle(
    ExecuteGatherDataCommand command,
    GatherData step,
    IDocumentSession session,
    TimeProvider time,
    CancellationToken ct)
{
    var result = await step.ExecuteAsync(State, ct);
    State = AnalysisStateReducer.Reduce(State, result.StateUpdate);

    // Return commands for all fork paths (executed in parallel)
    return [
        new ExecuteFinancialAnalysisStepCommand(WorkflowId),
        new ExecuteTechnicalAnalysisStepCommand(WorkflowId),
        new ExecuteMarketAnalysisStepCommand(WorkflowId)
    ];
}
```

## Error Handling in Fork Paths

If any fork path fails:

1. Other paths continue executing (fail-fast is not enabled by default)
2. The Join step receives whatever state is available
3. Configure fail-fast behavior if needed:

```csharp
.Fork(
    options => options.FailFast(),  // Stop all paths on first failure
    flow => flow.Then<Analysis1>(),
    flow => flow.Then<Analysis2>(),
    flow => flow.Then<Analysis3>())
.Join<Synthesize>()
```

## Key Points

- Fork paths execute concurrently for faster completion
- Join waits for all paths before continuing
- State from all paths is merged using reducer semantics
- Use instance names when reusing the same step type
- Each fork path can have multiple steps
- Error handling can be configured for fail-fast or continue-on-error
