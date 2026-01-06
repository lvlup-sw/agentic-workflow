# Branching Example

This example demonstrates conditional routing in workflows using the `Branch` DSL.

## Overview

Branches route workflow execution based on state values. Different paths execute different steps, then automatically rejoin at the next step after the branch block.

## State Definition

```csharp
[WorkflowState]
public record ClaimState : IWorkflowState
{
    public Guid WorkflowId { get; init; }
    public InsuranceClaim Claim { get; init; } = null!;
    public ClaimType ClaimType { get; init; }
    public ClaimAssessment? Assessment { get; init; }
    public InspectionReport? Inspection { get; init; }
    public ClaimDecision? Decision { get; init; }
    public bool ClaimantNotified { get; init; }
}

public record InsuranceClaim(
    string ClaimantId,
    string PolicyNumber,
    decimal Amount,
    string Description);

public record ClaimAssessment(
    ClaimType RecommendedType,
    decimal Confidence,
    string Rationale);

public record InspectionReport(
    string InspectorId,
    DateOnly InspectionDate,
    string Findings);

public record ClaimDecision(
    bool Approved,
    decimal ApprovedAmount,
    string Reason);

public enum ClaimType { Auto, Property, Health, Other }
```

## Workflow Definition

```csharp
var workflow = Workflow<ClaimState>
    .Create("process-claim")
    .StartWith<AssessClaim>()
    .Branch(state => state.ClaimType,
        when: ClaimType.Auto, then: flow => flow
            .Then<AutoClaimProcessor>(),
        when: ClaimType.Property, then: flow => flow
            .Then<PropertyInspection>()
            .Then<PropertyClaimProcessor>(),
        otherwise: flow => flow
            .Then<ManualReview>())
    .Finally<NotifyClaimant>();
```

The branch selector (`state => state.ClaimType`) extracts a value from state. Each `when` clause handles a specific case. The `otherwise` clause handles any unmatched values.

## Branch Patterns

### Simple Value Matching

```csharp
.Branch(state => state.ClaimType,
    when: ClaimType.Auto, then: flow => flow.Then<AutoProcessor>(),
    when: ClaimType.Property, then: flow => flow.Then<PropertyProcessor>(),
    otherwise: flow => flow.Then<DefaultProcessor>())
```

### Boolean Branching

For simple true/false decisions:

```csharp
.Branch(state => state.Amount > 10000m,
    whenTrue: flow => flow
        .AwaitApproval<SeniorAdjuster>()
        .Then<HighValueProcessor>(),
    whenFalse: flow => flow
        .Then<StandardProcessor>())
```

### Multi-Condition Branching

For complex routing logic:

```csharp
.Branch(state => ClassifyRisk(state),
    when: RiskLevel.Low, then: flow => flow.Then<AutoApprove>(),
    when: RiskLevel.Medium, then: flow => flow.Then<StandardReview>(),
    when: RiskLevel.High, then: flow => flow
        .Then<DetailedAnalysis>()
        .AwaitApproval<RiskCommittee>(),
    otherwise: flow => flow.Then<EscalateToManagement>())

private static RiskLevel ClassifyRisk(ClaimState state)
{
    if (state.Amount < 1000m) return RiskLevel.Low;
    if (state.Amount < 10000m) return RiskLevel.Medium;
    return RiskLevel.High;
}
```

## Step Implementations

### AssessClaim

```csharp
public class AssessClaim : IWorkflowStep<ClaimState>
{
    private readonly IClaimAssessor _assessor;

    public AssessClaim(IClaimAssessor assessor)
    {
        _assessor = assessor;
    }

    public async Task<StepResult<ClaimState>> ExecuteAsync(
        ClaimState state,
        StepContext context,
        CancellationToken ct)
    {
        var assessment = await _assessor.AssessAsync(state.Claim, ct);

        return state
            .With(s => s.Assessment, assessment)
            .With(s => s.ClaimType, assessment.RecommendedType)
            .AsResult();
    }
}
```

### AutoClaimProcessor

```csharp
public class AutoClaimProcessor : IWorkflowStep<ClaimState>
{
    private readonly IAutoClaimEngine _engine;

    public AutoClaimProcessor(IAutoClaimEngine engine)
    {
        _engine = engine;
    }

    public async Task<StepResult<ClaimState>> ExecuteAsync(
        ClaimState state,
        StepContext context,
        CancellationToken ct)
    {
        var decision = await _engine.ProcessAsync(state.Claim, ct);

        return state
            .With(s => s.Decision, decision)
            .AsResult();
    }
}
```

### PropertyInspection

```csharp
public class PropertyInspection : IWorkflowStep<ClaimState>
{
    private readonly IInspectionService _inspectionService;

    public PropertyInspection(IInspectionService inspectionService)
    {
        _inspectionService = inspectionService;
    }

    public async Task<StepResult<ClaimState>> ExecuteAsync(
        ClaimState state,
        StepContext context,
        CancellationToken ct)
    {
        var report = await _inspectionService.ScheduleAndCompleteAsync(
            state.Claim,
            ct);

        return state
            .With(s => s.Inspection, report)
            .AsResult();
    }
}
```

### PropertyClaimProcessor

```csharp
public class PropertyClaimProcessor : IWorkflowStep<ClaimState>
{
    private readonly IPropertyClaimEngine _engine;

    public PropertyClaimProcessor(IPropertyClaimEngine engine)
    {
        _engine = engine;
    }

    public async Task<StepResult<ClaimState>> ExecuteAsync(
        ClaimState state,
        StepContext context,
        CancellationToken ct)
    {
        var decision = await _engine.ProcessAsync(
            state.Claim,
            state.Inspection!,
            ct);

        return state
            .With(s => s.Decision, decision)
            .AsResult();
    }
}
```

### ManualReview

```csharp
public class ManualReview : IWorkflowStep<ClaimState>
{
    private readonly IManualReviewQueue _queue;

    public ManualReview(IManualReviewQueue queue)
    {
        _queue = queue;
    }

    public async Task<StepResult<ClaimState>> ExecuteAsync(
        ClaimState state,
        StepContext context,
        CancellationToken ct)
    {
        // Queue for manual processing and await decision
        var decision = await _queue.SubmitAndAwaitDecisionAsync(
            state.Claim,
            ct);

        return state
            .With(s => s.Decision, decision)
            .AsResult();
    }
}
```

### NotifyClaimant

```csharp
public class NotifyClaimant : IWorkflowStep<ClaimState>
{
    private readonly INotificationService _notifications;

    public NotifyClaimant(INotificationService notifications)
    {
        _notifications = notifications;
    }

    public async Task<StepResult<ClaimState>> ExecuteAsync(
        ClaimState state,
        StepContext context,
        CancellationToken ct)
    {
        await _notifications.SendClaimDecisionAsync(
            state.Claim.ClaimantId,
            state.Decision!,
            ct);

        return state
            .With(s => s.ClaimantNotified, true)
            .AsResult();
    }
}
```

## Generated Phase Enum

```csharp
public enum ProcessClaimPhase
{
    NotStarted,
    AssessClaim,
    AutoClaimProcessor,
    PropertyInspection,
    PropertyClaimProcessor,
    ManualReview,
    NotifyClaimant,
    Completed,
    Failed
}
```

## Generated Transition Table

```csharp
public static class ProcessClaimTransitions
{
    public static readonly IReadOnlyDictionary<ProcessClaimPhase, ProcessClaimPhase[]> Valid =
        new Dictionary<ProcessClaimPhase, ProcessClaimPhase[]>
        {
            [ProcessClaimPhase.AssessClaim] = [
                ProcessClaimPhase.AutoClaimProcessor,
                ProcessClaimPhase.PropertyInspection,
                ProcessClaimPhase.ManualReview
            ],
            [ProcessClaimPhase.AutoClaimProcessor] = [ProcessClaimPhase.NotifyClaimant],
            [ProcessClaimPhase.PropertyInspection] = [ProcessClaimPhase.PropertyClaimProcessor],
            [ProcessClaimPhase.PropertyClaimProcessor] = [ProcessClaimPhase.NotifyClaimant],
            [ProcessClaimPhase.ManualReview] = [ProcessClaimPhase.NotifyClaimant],
            [ProcessClaimPhase.NotifyClaimant] = [ProcessClaimPhase.Completed],
        };
}
```

## Key Points

- Branches automatically rejoin at the next step after the branch block
- Use `otherwise` to handle unmatched values (avoids runtime exceptions)
- Branch paths can have multiple steps
- Branch conditions are evaluated against current state
- The generated transition table shows all valid paths
