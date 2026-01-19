# Design: Developer Adoption Strategy

## Problem Statement

Agentic.Workflow has strong technical capabilities (event-sourced audit trails, Thompson Sampling, compile-time validation, confidence routing) but faces adoption challenges:

1. **The pattern is new** — Developers don't immediately understand the value proposition
2. **Unclear differentiation** — "Why this instead of LangGraph or Temporal?"
3. **Unclear use cases** — "What would I actually build with this?"

### Target Outcome

- **Primary metric:** GitHub stars & community growth (target: 500+ stars)
- **Target user:** .NET developers building AI applications
- **Investment level:** Developer experience overhaul (~1 month)

## Chosen Approach

**Use-Case Gallery with Audit-First Positioning**

Lead with 3 compelling, production-relevant sample applications that each demonstrate a different core capability. Each sample answers "what would I build?" while naturally showcasing why auditability and the other features matter.

This approach was chosen over:
- **Zero-infrastructure mode** — Requires significant engineering; creates "lite" version confusion
- **Pure messaging refresh** — Doesn't address the "unclear use cases" gap directly

## Sample Applications

### Sample 1: Content Publishing Pipeline

**Domain:** Blog/CMS content workflow

**Story:** A content team uses AI to draft articles, but needs human approval before publishing. If published content causes issues, they need to unpublish and understand what went wrong.

**Workflow:**
```
Draft → AI Review → Human Approval → Publish
                         ↓ (rejected)
                    Request Revisions
                         ↓
                      Revise → (loop back to AI Review)
```

**Features Showcased:**
- `AwaitApproval<T>()` — Human-in-the-loop approval gate
- `Compensate<T>()` — Unpublish handler if post-publish issues arise
- Audit trail — "Who approved this? What did the AI review say?"

**State Shape:**
```csharp
[WorkflowState]
public record ContentState : IWorkflowState
{
    public Guid WorkflowId { get; init; }
    public string Title { get; init; }
    public string Draft { get; init; }
    public string? AiReviewFeedback { get; init; }
    public decimal AiQualityScore { get; init; }
    public ApprovalDecision? HumanDecision { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public string? PublishedUrl { get; init; }
}
```

**Steps:**
1. `GenerateDraft` — Creates initial content (mock LLM)
2. `AiReviewContent` — Quality/tone/accuracy check (mock LLM)
3. `AwaitHumanApproval` — Approval gate with timeout
4. `PublishContent` — Publishes to mock CMS
5. `UnpublishContent` — Compensation handler

**Estimated effort:** 3-4 hours

---

### Sample 2: Multi-Model Router

**Domain:** Cost-optimized LLM routing

**Story:** An application needs to answer user questions but wants to use expensive models (GPT-4, Claude) only when necessary. Over time, the system learns which model performs best for which query types.

**Workflow:**
```
Classify Query → Select Model (Thompson Sampling) → Generate Response → Record Feedback
                        ↓
              [GPT-4 | Claude | Local Model]
```

**Features Showcased:**
- Thompson Sampling — Learns optimal model selection over time
- `IAgentSelector` — Multi-armed bandit for model choice
- Confidence routing — Fall back to expensive model if cheap model is uncertain
- Audit trail — "Which model answered? What was its confidence? How did user rate it?"

**State Shape:**
```csharp
[WorkflowState]
public record RouterState : IWorkflowState
{
    public Guid WorkflowId { get; init; }
    public string UserQuery { get; init; }
    public QueryCategory Category { get; init; }
    public string SelectedModel { get; init; }
    public string Response { get; init; }
    public decimal Confidence { get; init; }
    public UserFeedback? Feedback { get; init; }
}

public enum QueryCategory { Factual, Creative, Technical, Conversational }
```

**Steps:**
1. `ClassifyQuery` — Categorize the query type
2. `SelectModel` — Thompson Sampling selection from candidate models
3. `GenerateResponse` — Call selected model (mock)
4. `RecordFeedback` — Capture user satisfaction for learning

**Estimated effort:** 3-4 hours

---

### Sample 3: Agentic Coder

**Domain:** AI-assisted code generation

**Story:** An AI coding assistant that plans an implementation, writes code, runs tests, and iterates until tests pass or a human intervenes. Demonstrates complex control flow with loops and checkpoints.

**Workflow:**
```
Analyze Task → Plan Implementation → [Code → Test → Review] (loop) → Human Checkpoint → Complete
                                           ↓ (tests fail)
                                      Revise Code (max 3 attempts)
                                           ↓ (max attempts)
                                      Escalate to Human
```

**Features Showcased:**
- `RepeatUntil()` — Iterative refinement loop
- `Fork()`/`Join()` — Parallel test execution (unit + integration)
- `AwaitApproval<T>()` — Human checkpoint before completion
- Loop detection — Prevents infinite revision cycles
- Audit trail — "What did the AI try? Why did each attempt fail?"

**State Shape:**
```csharp
[WorkflowState]
public record CoderState : IWorkflowState
{
    public Guid WorkflowId { get; init; }
    public string TaskDescription { get; init; }
    public string? Plan { get; init; }

    [Append]
    public IReadOnlyList<CodeAttempt> Attempts { get; init; } = [];

    public TestResults? LatestTestResults { get; init; }
    public int AttemptCount { get; init; }
    public bool HumanApproved { get; init; }
}

public record CodeAttempt(string Code, string Reasoning, DateTimeOffset Timestamp);
public record TestResults(bool Passed, IReadOnlyList<string> Failures);
```

**Steps:**
1. `AnalyzeTask` — Understand requirements (mock LLM)
2. `PlanImplementation` — Create implementation plan
3. `GenerateCode` — Write code based on plan
4. `RunTests` — Execute tests (mock)
5. `ReviewResults` — Decide: pass, revise, or escalate
6. `AwaitHumanCheckpoint` — Final human approval
7. `Complete` — Mark as done

**Estimated effort:** 4-5 hours

---

## VitePress Documentation Updates

### New Structure

```
docs/
├── examples/
│   ├── index.md                    # Examples overview (UPDATE)
│   ├── content-pipeline.md         # NEW - Content Publishing walkthrough
│   ├── multi-model-router.md       # NEW - Router walkthrough
│   ├── agentic-coder.md            # NEW - Coder walkthrough
│   └── ... (existing examples)
├── learn/
│   └── index.md                    # UPDATE - Add "See it in action" section
└── index.md                        # UPDATE - Hero section use cases
```

### Homepage Updates (`docs/index.md`)

Add a "See It In Action" section after the hero:

```markdown
## See It In Action

Real-world samples demonstrating core capabilities:

| Sample | What It Shows | Key Feature |
|--------|---------------|-------------|
| [Content Pipeline](/examples/content-pipeline) | Draft → Review → Approve → Publish | Human-in-loop + Compensation |
| [Multi-Model Router](/examples/multi-model-router) | Smart LLM selection that learns | Thompson Sampling |
| [Agentic Coder](/examples/agentic-coder) | Plan → Code → Test → Iterate | Loops + Checkpoints |
```

### Example Page Template

Each example page follows this structure:

1. **The Problem** — What real-world challenge this solves
2. **Why Agentic.Workflow** — What feature makes this easier here
3. **The Workflow** — Visual diagram + DSL code
4. **Key Implementation Details** — Interesting code snippets
5. **The Audit Trail** — Example of what events get captured
6. **Try It** — Link to full source + instructions to run

### Estimated Effort

| Task | Hours |
|------|-------|
| Update `examples/index.md` | 0.5 |
| Create `content-pipeline.md` | 1.5 |
| Create `multi-model-router.md` | 1.5 |
| Create `agentic-coder.md` | 1.5 |
| Update homepage | 0.5 |
| Update learn/index.md | 0.5 |
| Navigation/sidebar updates | 0.5 |
| **Total** | **6.5 hours** |

---

## Sample Code Location

```
samples/
├── ContentPipeline/
│   ├── ContentPipeline.csproj
│   ├── ContentState.cs
│   ├── ContentWorkflow.cs
│   ├── Steps/
│   │   ├── GenerateDraft.cs
│   │   ├── AiReviewContent.cs
│   │   ├── PublishContent.cs
│   │   └── UnpublishContent.cs
│   ├── Program.cs                  # Runnable demo
│   └── README.md
├── MultiModelRouter/
│   ├── ...
└── AgenticCoder/
    ├── ...
```

Each sample is a standalone, runnable project that can be executed with `dotnet run`.

---

## Messaging Updates

### Tagline Options

Current: "Deterministic orchestration for probabilistic AI agents"

Alternatives to consider:
- "AI workflows you can audit, debug, and trust"
- "Event-sourced AI orchestration for .NET"
- "The AI workflow engine with receipts"

### README Positioning

Update the "Problem" section to lead with use cases:

> Building AI-powered automation? You need more than just "call the LLM":
> - **Content pipelines** need human approval gates and rollback
> - **Multi-model systems** need intelligent routing that learns
> - **Agentic coding** needs iteration loops with guardrails
>
> Agentic.Workflow provides these patterns out of the box, with complete audit trails.

---

## Testing Strategy

Each sample includes:
1. **Compilation test** — Verifies the workflow definition compiles
2. **Happy path test** — Runs workflow to completion with mocks
3. **Compensation test** (where applicable) — Verifies rollback works

Tests use in-memory implementations to avoid PostgreSQL dependency for CI.

---

## Implementation Plan

| Phase | Tasks | Effort |
|-------|-------|--------|
| **1. Samples** | Build 3 sample applications | 10-12 hrs |
| **2. Docs** | VitePress pages for each sample | 6-7 hrs |
| **3. Polish** | README updates, homepage refresh | 2-3 hrs |
| **Total** | | **~20 hours** |

### Task Breakdown

1. Content Publishing Pipeline sample
2. Multi-Model Router sample
3. Agentic Coder sample
4. VitePress: content-pipeline.md
5. VitePress: multi-model-router.md
6. VitePress: agentic-coder.md
7. VitePress: examples/index.md update
8. VitePress: homepage update
9. VitePress: learn/index.md update
10. README.md messaging refresh

---

## Open Questions

1. **Sample complexity level** — Should samples use real LLM calls (requires API keys) or pure mocks? Recommendation: Pure mocks for zero-friction running.

2. **Separate repo vs monorepo** — Should samples live in `samples/` in this repo or a separate `agentic-workflow-samples` repo? Recommendation: Same repo for discoverability.

3. **In-memory mode** — Should we create a simple in-memory persistence option for samples to avoid PostgreSQL requirement? This is scope creep but would help adoption.

---

## Success Criteria

- [ ] 3 sample applications compile and run
- [ ] Each sample has comprehensive VitePress documentation
- [ ] Homepage updated with "See It In Action" section
- [ ] README messaging refreshed with use-case focus
- [ ] Samples are discoverable from docs site navigation
