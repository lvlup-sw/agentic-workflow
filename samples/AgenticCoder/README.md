# AgenticCoder Sample

This sample demonstrates an AI-powered code generation workflow using Agentic.Workflow. It showcases iterative refinement, test-driven development, human checkpoints, and audit trails.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **RepeatUntil** | Iterative refinement loop that retries code generation until tests pass |
| **AwaitApproval** | Human-in-the-loop checkpoint requiring developer approval |
| **Loop Detection** | Maximum 3 iterations prevents infinite refinement cycles |
| **Audit Trail** | All code attempts preserved with reasoning and timestamps |
| **[Append] Attribute** | Attempts collection accumulates history across iterations |

## Workflow Structure

```
AnalyzeTask -> PlanImplementation -> [GenerateCode -> RunTests -> ReviewResults] -> HumanCheckpoint -> Complete
                                             ^                    |
                                             |-- tests fail ------+
                                             (max 3 attempts)
```

## State Design

```csharp
[WorkflowState]
public record CoderState : IWorkflowState
{
    public Guid WorkflowId { get; init; }
    public string TaskDescription { get; init; } = string.Empty;
    public string? Plan { get; init; }

    [Append]  // History accumulates across iterations
    public IReadOnlyList<CodeAttempt> Attempts { get; init; } = [];

    public TestResults? LatestTestResults { get; init; }
    public int AttemptCount { get; init; }
    public bool HumanApproved { get; init; }
}
```

## Running the Sample

```bash
dotnet run --project samples/AgenticCoder
```

### Expected Output

The demo simulates generating a FizzBuzz function with intentional early failures to demonstrate the iteration loop:

1. **Iteration 1**: Code missing FizzBuzz check - tests fail
2. **Iteration 2**: Wrong check order - tests fail
3. **Iteration 3**: Correct implementation - tests pass
4. **Human Checkpoint**: Developer approves final code
5. **Completion**: Workflow finishes with audit trail

## Running Tests

```bash
dotnet test --project samples/AgenticCoder.Tests
```

## Key Components

### Workflow Definition

```csharp
public static WorkflowDefinition<CoderState> Create() =>
    Workflow<CoderState>
        .Create("agentic-coder")
        .StartWith<AnalyzeTask>()
        .Then<PlanImplementation>()
        .RepeatUntil(
            condition: state => state.LatestTestResults?.Passed == true,
            loopName: "Refinement",
            body: loop => loop
                .Then<GenerateCode>()
                .Then<RunTests>()
                .Then<ReviewResults>(),
            maxIterations: 3)
        .AwaitApproval<HumanDeveloper>(approval => approval
            .WithContext("Please review the generated code.")
            .WithOption("approve", "Approve", "Accept the implementation")
            .WithOption("reject", "Reject", "Request changes"))
        .Finally<Complete>();
```

### Steps

| Step | Purpose |
|------|---------|
| `AnalyzeTask` | Validates task description and extracts requirements |
| `PlanImplementation` | Creates step-by-step implementation plan |
| `GenerateCode` | Generates code based on task, plan, and feedback |
| `RunTests` | Executes tests against generated code |
| `ReviewResults` | Decision point for loop continuation |
| `Complete` | Terminal step marking workflow completion |

### Mock Services

The sample uses mock implementations for demonstration:

- `MockTaskAnalyzer` - Simulates task analysis
- `MockPlanner` - Creates simple implementation plans
- `MockCodeGenerator` - Generates code with configurable failure count
- `MockTestRunner` - Validates code against known patterns

## Extending the Sample

To use real AI services:

1. Implement `ICodeGenerator` with your LLM provider (e.g., Claude, GPT-4)
2. Implement `ITestRunner` to compile and execute actual tests
3. Wire up dependency injection in your application
4. The workflow definition remains unchanged

## License

MIT
