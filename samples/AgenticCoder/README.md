# AgenticCoder: Iterative Code Generation with Test-Driven Refinement

## The Problem: AI Gets It Wrong on the First Try

You've connected an LLM to your codebase for automated code generation. The first attempt looks promising, but:
- It misses edge cases
- It uses the wrong API patterns
- Tests fail for subtle reasons the AI didn't anticipate

**The naive solution**: Retry with "please fix the errors." This rarely works because:
- The AI doesn't know WHAT failed (just that something did)
- Each retry is independent—no accumulated learning
- You have no visibility into what's been tried
- Eventually you hit rate limits or give up

**What you need**: A structured iteration loop that:
1. Captures what went wrong (test failures, not just "error")
2. Feeds that information back into the next attempt
3. Accumulates history for debugging and audit
4. Has a hard limit to prevent infinite loops
5. Requires human sign-off before deployment

This is iterative refinement with test-driven feedback—exactly what AgenticCoder demonstrates.

---

## Learning Objectives

After working through this sample, you will understand:

- **RepeatUntil loops** with condition-based termination
- **The `[Append]` attribute** for accumulating history across iterations
- **Why loops need bounds** (max iterations as a safety mechanism)
- **Human checkpoints** for final approval before completion
- **State design** that supports iterative refinement

---

## Conceptual Foundation

### Iterative Refinement vs. Random Retry

| Approach | How It Works | Result |
|----------|--------------|--------|
| **Random Retry** | "Try again" with no context | Same mistakes repeated |
| **Iterative Refinement** | Feed failure details into next attempt | Each attempt is informed by previous failures |

The key difference is **what you do with failure information**:

```text
Random Retry:
  Attempt 1 → Fail → "Try again" → Attempt 2 → Same failure

Iterative Refinement:
  Attempt 1 → Fail("Missing null check") → "Fix: add null check" → Attempt 2 → Better
```

### Why Loops Need Bounds

An unbounded loop is a bug waiting to happen:

```text
Loop forever until tests pass:
  - What if the task is impossible?
  - What if the tests are flaky?
  - What if the AI is stuck in a local minimum?
  - What if you run out of budget/tokens?
```

**The solution**: `maxIterations` as a circuit breaker. After N attempts, escalate to a human rather than burning resources on a lost cause.

```csharp
.RepeatUntil(
    condition: state => state.LatestTestResults?.Passed == true,
    maxIterations: 3)  // Circuit breaker
```

### The [Append] Attribute: Why Accumulate History?

Consider two approaches to tracking attempts:

**Replace (default)**:
```csharp
public CodeAttempt? LatestAttempt { get; init; }  // Only keeps last
```

**Append**:
```csharp
[Append]
public IReadOnlyList<CodeAttempt> Attempts { get; init; } = [];  // Keeps all
```

Why append?

1. **Debugging**: See what was tried before the solution
2. **Audit trail**: Compliance requires knowing the AI's journey
3. **Feedback loop**: Next attempt can see what failed
4. **Learning**: Patterns emerge across many workflows

### Human Checkpoints: Trust but Verify

AI-generated code should never auto-deploy to production:

```text
Without human checkpoint:
  AI generates code → Tests pass → Auto-deploy → 😱

With human checkpoint:
  AI generates code → Tests pass → Human reviews → Approve → Deploy ✓
                                                → Reject → Back to loop
```

The `AwaitApproval` step creates a pause point where a human can:
- Review the generated code
- Check for subtle issues tests don't catch
- Approve or send back for refinement

---

## Design Decisions

| Decision | Why This Approach | Alternative | Trade-off |
|----------|-------------------|-------------|-----------|
| **RepeatUntil** | Condition-based exit | Fixed iteration count | More flexible but needs good condition |
| **maxIterations: 3** | Typical AI improvement plateaus | 5, 10, unlimited | More attempts = more cost, diminishing returns |
| **[Append] on Attempts** | Need full history | Replace with latest | Memory usage, but essential for audit |
| **Human checkpoint after loop** | Quality gate before completion | Auto-complete | Slower, but safer for production code |

### When to Use This Pattern

**Good fit when**:
- Output quality can be objectively measured (tests, linting, type checking)
- Failures provide actionable feedback (not just "wrong")
- Multiple attempts typically improve results
- Human oversight is required before deployment

**Poor fit when**:
- No objective quality measure exists
- Failures don't provide useful feedback
- First attempt is typically final (no iteration needed)
- Fully autonomous operation required

### Anti-Patterns to Avoid

| Anti-Pattern | Problem | Correct Approach |
|--------------|---------|------------------|
| **No max iterations** | Infinite loops burn budget | Always set `maxIterations` |
| **Retry without feedback** | Same mistakes repeated | Feed test failures to next attempt |
| **Replace instead of Append** | Lose audit trail | Use `[Append]` for history |
| **Skip human checkpoint** | Unsafe auto-deployment | Always require approval for production |
| **Too many iterations** | Diminishing returns after ~3 | Escalate to human sooner |

---

## Building the Workflow

### The Shape First

```text
┌─────────────┐    ┌───────────────────┐    ┌──────────────────────────────────────────┐
│ AnalyzeTask │───▶│ PlanImplementation│───▶│              Refinement Loop             │
│             │    │                   │    │  ┌────────────┐  ┌─────────┐  ┌────────┐ │
│ "FizzBuzz"  │    │ "1. Handle 15     │    │  │GenerateCode│─▶│RunTests │─▶│Review  │ │
│ → Valid     │    │  2. Handle 3/5    │    │  └────────────┘  └─────────┘  └────────┘ │
│             │    │  3. Handle other" │    │         ▲              │                 │
└─────────────┘    └───────────────────┘    │         └──────────────┘                 │
                                            │            (if tests fail)               │
                                            │            (max 3 iterations)            │
                                            └──────────────────────────────────────────┘
                                                              │
                                                              ▼
                                            ┌──────────────────────────────────────────┐
                                            │         Human Checkpoint                 │
                                            │  "Review code before completion"         │
                                            │  [Approve] [Reject]                      │
                                            └──────────────────────────────────────────┘
                                                              │
                                                              ▼
                                                        ┌──────────┐
                                                        │ Complete │
                                                        └──────────┘
```

### State: What We Track

```csharp
[WorkflowState]
public sealed record CoderState : IWorkflowState
{
    // Identity
    public Guid WorkflowId { get; init; }

    // Input
    public string TaskDescription { get; init; } = string.Empty;

    // Planning output (set once)
    public string? Plan { get; init; }

    // Iteration history (accumulates with [Append])
    [Append]
    public IReadOnlyList<CodeAttempt> Attempts { get; init; } = [];

    // Latest test results (replaced each iteration)
    public TestResults? LatestTestResults { get; init; }

    // Loop counter
    public int AttemptCount { get; init; }

    // Human approval flag
    public bool HumanApproved { get; init; }
}
```

**Why this design?**

- `Plan`: Set once by `PlanImplementation`, never changes
- `Attempts`: Accumulates via `[Append]`—each iteration adds, never replaces
- `LatestTestResults`: Replaced each iteration—we only need the most recent
- `AttemptCount`: Simple counter for logging and max iteration tracking
- `HumanApproved`: Set by `AwaitApproval` checkpoint

### The CodeAttempt Record

```csharp
public sealed record CodeAttempt(
    string Code,        // The generated code
    string Reasoning,   // Why the AI made these choices
    DateTimeOffset Timestamp);  // When this attempt happened
```

**Why include Reasoning?**
- Debugging: Understand the AI's logic when something goes wrong
- Feedback: The reasoning from failed attempts informs the next attempt
- Audit: Compliance may require understanding the AI's decision process

### The Workflow Definition

```csharp
public static WorkflowDefinition<CoderState> Create() =>
    Workflow<CoderState>
        .Create("agentic-coder")

        // Phase 1: Analysis and Planning
        .StartWith<AnalyzeTask>()          // Validate and extract requirements
        .Then<PlanImplementation>()        // Create step-by-step plan

        // Phase 2: Iterative Refinement Loop
        .RepeatUntil(
            condition: state => state.LatestTestResults?.Passed == true,
            loopName: "Refinement",
            body: loop => loop
                .Then<GenerateCode>()      // Generate based on plan + feedback
                .Then<RunTests>()          // Execute tests
                .Then<ReviewResults>(),    // Checkpoint for metrics/logging
            maxIterations: 3)              // Circuit breaker

        // Phase 3: Human Approval
        .AwaitApproval<HumanDeveloper>(approval => approval
            .WithContext("Please review the generated code before marking as complete.")
            .WithOption("approve", "Approve", "Accept the implementation")
            .WithOption("reject", "Reject", "Request changes"))

        // Phase 4: Completion
        .Finally<Complete>();
```

**Reading this definition**:
1. Start by analyzing the task
2. Create an implementation plan
3. Loop: generate → test → review (until tests pass OR 3 iterations)
4. Human approves or rejects
5. Mark complete

### The Key Step: GenerateCode

```csharp
public async Task<StepResult<CoderState>> ExecuteAsync(
    CoderState state,
    StepContext context,
    CancellationToken cancellationToken)
{
    // Build context from previous attempts (if any)
    var feedback = state.Attempts.Count > 0
        ? $"Previous attempt failed: {string.Join(", ", state.LatestTestResults?.Failures ?? [])}"
        : null;

    // Generate code with context
    var generatedCode = await _codeGenerator.GenerateAsync(
        state.TaskDescription,
        state.Plan!,
        feedback,  // This is the key: feed failure info to next attempt
        cancellationToken);

    // Create attempt record
    var attempt = new CodeAttempt(
        Code: generatedCode.Code,
        Reasoning: generatedCode.Reasoning,
        Timestamp: _timeProvider.GetUtcNow());

    // Return state with new attempt appended
    return state with
    {
        Attempts = [attempt],  // [Append] attribute adds this to existing list
        AttemptCount = state.AttemptCount + 1,
    };
}
```

**The feedback loop**: Notice how `state.LatestTestResults?.Failures` from the previous iteration becomes input to the next `GenerateAsync` call. This is what makes it iterative refinement rather than random retry.

---

## The "Aha Moment"

> **The difference between "retry on failure" and "iterative refinement" is what you do with the failure information.**
>
> A retry says "try again." Iterative refinement says "try again, and here's exactly what went wrong last time."
>
> The `[Append]` attribute on `Attempts` isn't just for logging—it's the memory that enables learning across iterations. Each attempt can see the full history of what's been tried.

---

## Running the Sample

```bash
dotnet run --project samples/AgenticCoder
```

### What You'll See

The demo simulates generating a FizzBuzz function with intentional early failures:

```text
===========================================
   AgenticCoder Workflow Sample
   Iterative Code Generation with TDD
===========================================

Task: Implement a FizzBuzz function that returns 'Fizz' for multiples of 3...

--- Phase 1: Analysis ---
[AnalyzeTask]
Task Analysis:
  Valid: True
  Complexity: Low
  Requirements: 7

--- Phase 2: Planning ---
[PlanImplementation]
Implementation Plan created.

--- Phase 3: Refinement Loop ---

[Iteration 1]
[GenerateCode]
Generated code (Attempt #1):
  Length: 234 chars
  Reasoning: Basic implementation focusing on core logic

[RunTests]
Tests failed:
  - Expected 'FizzBuzz' for 15, got '15'

[ReviewResults]
Loop condition not met. Continuing refinement...

[Iteration 2]
[GenerateCode]
Generated code (Attempt #2):
  Length: 289 chars
  Reasoning: Fixed FizzBuzz case based on test feedback

[RunTests]
Tests failed:
  - Expected 'Fizz' for 9, got '9'

[ReviewResults]
Loop condition not met. Continuing refinement...

[Iteration 3]
[GenerateCode]
Generated code (Attempt #3):
  Length: 312 chars
  Reasoning: Fixed all modulo checks order

[RunTests]
All tests passed!

[ReviewResults]

----------------------------------------------------------------------
 Human Checkpoint: AwaitApproval<HumanDeveloper>
----------------------------------------------------------------------
Code has been approved for completion.

--- Phase 4: Completion ---
[Complete]
Workflow completed successfully!
```

Watch how:
- Each iteration receives feedback from the previous failure
- The reasoning changes based on what went wrong
- Tests eventually pass after incorporating feedback
- Human approval is required before completion

---

## Extension Exercises

### Exercise 1: Add Escalation on Max Iterations

When max iterations is reached without passing tests, escalate to a senior developer:

1. Add an `Escalated` flag to state
2. After the loop, check if `AttemptCount >= 3 && !LatestTestResults.Passed`
3. Add a different approval path for escalated cases

### Exercise 2: Track Improvement Metrics

Measure how much each iteration improves:

1. Add a `TestsPassedCount` to `TestResults`
2. Calculate improvement rate between iterations
3. Log when improvement stalls (might indicate stuck in local minimum)

### Exercise 3: Implement Rollback on Rejection

When a human rejects the code, go back to the refinement loop:

1. Handle the "reject" option in the approval step
2. Clear `HumanApproved` and route back to `GenerateCode`
3. Add the rejection reason to feedback for the next attempt

---

## Running Tests

```bash
dotnet test samples/AgenticCoder.Tests
```

The tests verify:
- Task analysis validation
- Plan generation
- Code generation with feedback incorporation
- Test execution and result capture
- Loop termination on success
- Loop termination on max iterations

---

## Key Takeaways

1. **Iterative refinement requires feeding failure information forward**—not just "try again"
2. **`[Append]` creates audit trails**—every attempt is preserved, not replaced
3. **maxIterations is a circuit breaker**—prevents infinite loops and budget overruns
4. **Human checkpoints before deployment**—AI code should never auto-deploy
5. **ReviewResults is a checkpoint, not a decision maker**—the loop condition handles termination

---

## Learn More

- [Iterative Refinement Pattern](../../docs/examples/iterative-refinement.md) - Detailed pattern documentation
- [Approval Flow Pattern](../../docs/examples/approval-flow.md) - Human-in-the-loop patterns
- [WorkflowState Attributes](../../docs/learn/index.md) - `[Append]`, `[Snapshot]`, and other reducers
