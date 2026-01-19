# ContentPipeline: Human-AI Collaboration with Trust and Accountability

## The Problem: AI Publishes Without Human Oversight

Your marketing team wants to automate content creation. AI generates drafts, and... then what?

**The dangerous approach**: AI generates → Auto-publish. Problems:
- No human review of AI-generated content
- Legal liability for factual errors or inappropriate content
- No audit trail showing who approved what
- No way to undo if something goes wrong post-publication

**What you need**: A pipeline that:
1. Lets AI do the heavy lifting (draft generation, quality assessment)
2. Requires human approval before anything goes public
3. Tracks exactly who approved what and when (audit trail)
4. Can undo publication if issues arise later (compensation)

This is human-AI collaboration with accountability—exactly what ContentPipeline demonstrates.

---

## Learning Objectives

After working through this sample, you will understand:

- **Human approval gates** using `AwaitApproval<T>()` pattern
- **Compensation (rollback)** for undoing completed steps
- **Audit trails** that capture "who did what, when, and why"
- **AI review as a step** (not a replacement for human judgment)
- **State design** for accountability-focused workflows

---

## Conceptual Foundation

### The Human-AI Collaboration Spectrum

| Approach | Human Role | Risk Level |
|----------|------------|------------|
| **Human-only** | Does everything | Low risk, high cost, slow |
| **AI-assisted** | Reviews AI output before action | Balanced |
| **AI-autonomous** | Notified after AI acts | High risk, low cost, fast |

ContentPipeline demonstrates **AI-assisted**: The AI generates and reviews, but humans approve before publication.

### Why Human Approval Gates?

Human approval isn't bureaucracy—it's a risk management strategy:

1. **Liability**: Who's responsible when AI-generated content contains errors?
2. **Brand safety**: Can AI reliably detect tone-deaf or inappropriate content?
3. **Context**: Does AI understand the political/social context of publishing today?
4. **Compliance**: Regulations may require human review of certain content types

The `AwaitApproval` step creates a **pause point** where a human can:
- Review the AI's work
- See the AI's confidence and reasoning
- Make the final call with full context

### The Saga Pattern and Compensation

Traditional transactions are all-or-nothing: success or rollback. But distributed workflows can't use database transactions across services. The **saga pattern** offers an alternative:

```text
Traditional Transaction:
  BEGIN → Step 1 → Step 2 → Step 3 → COMMIT
                       ↓
                   ROLLBACK (all steps undone atomically)

Saga Pattern:
  Step 1 → Step 2 → Step 3 (failure!)
                       ↓
              Compensate Step 2 (undo what was done)
              Compensate Step 1 (undo what was done)
```

In ContentPipeline:
- **Publish** creates content at a URL
- **Unpublish** (compensation) removes content from that URL

Compensation isn't automatic—you must design each step's "undo" action.

### Audit Trails: Why Record Everything?

AI systems need more logging than traditional software:

| What to Log | Why |
|-------------|-----|
| **Input to AI** | Reproduce the decision if questioned |
| **AI's output** | See exactly what the AI said |
| **AI's confidence** | Understand certainty level |
| **Human decision** | Prove who approved what |
| **Timestamp** | Establish timeline for incidents |

The audit trail answers: "Last month, why did we publish that article?"

```text
2024-03-15 10:00:00 - Draft Generated (AI)
  Details: Generated from prompt "Write about quantum computing"

2024-03-15 10:00:05 - AI Review Completed (AI)
  Details: Quality score: 85%. "Minor clarity issues in paragraph 3"

2024-03-15 11:30:00 - Human Approval Received (editor-jane)
  Details: "Addressed clarity issues, approved for publication"

2024-03-15 11:35:00 - Content Published (System)
  Details: Published to https://example.com/quantum-computing
```

---

## Design Decisions

| Decision | Why This Approach | Alternative | Trade-off |
|----------|-------------------|-------------|-----------|
| **Human approval before publish** | Legal/brand protection | Auto-publish with monitoring | Slower, but safer |
| **AI review step** | Quality gate before human sees it | Human reviews raw draft | Saves human time, AI catches obvious issues |
| **Immutable audit entries** | Can't tamper with history | Mutable log | Storage cost, but compliance-ready |
| **Compensation step** | Graceful rollback | Delete-and-hope | Explicit undo logic, auditable |

### When to Use This Pattern

**Good fit when**:
- Content reaches external audiences (customers, public)
- Legal or compliance requirements exist
- Brand reputation is at stake
- Undo capability is needed post-completion
- Audit trails are required

**Poor fit when**:
- Internal-only content with low stakes
- Speed matters more than review quality
- No regulatory requirements
- Single-use, non-persistent outputs

### Anti-Patterns to Avoid

| Anti-Pattern | Problem | Correct Approach |
|--------------|---------|------------------|
| **Skip human approval** | No accountability for AI errors | Always require human sign-off for external content |
| **Mutable audit log** | Can't prove what happened | Append-only audit entries |
| **No AI review** | Humans review everything | Let AI catch obvious issues first |
| **No compensation** | Can't undo completed actions | Design explicit undo steps |
| **Overwrite instead of append** | Lose history | Use `[Append]` or manual append patterns |

---

## Building the Workflow

### The Shape First

```text
┌──────────────┐    ┌────────────────┐    ┌─────────────────────┐    ┌────────────────┐
│ GenerateDraft│───▶│ AiReviewContent│───▶│ AwaitHumanApproval  │───▶│ PublishContent │
│              │    │                │    │                     │    │                │
│ AI writes    │    │ AI scores      │    │ Human reviews       │    │ System         │
│ draft        │    │ quality        │    │ and approves        │    │ publishes      │
│              │    │                │    │                     │    │                │
│ + Audit      │    │ + Audit        │    │ + Audit             │    │ + Audit        │
└──────────────┘    └────────────────┘    └─────────────────────┘    └────────────────┘
                                                                             │
                                                                             │ (if issues later)
                                                                             ▼
                                                                    ┌────────────────┐
                                                                    │UnpublishContent│
                                                                    │                │
                                                                    │ Compensation   │
                                                                    │ (rollback)     │
                                                                    │                │
                                                                    │ + Audit        │
                                                                    └────────────────┘
```

### State: What We Track

```csharp
[WorkflowState]
public sealed record ContentState : IWorkflowState
{
    // Identity
    public Guid WorkflowId { get; init; }

    // Input
    public string Title { get; init; } = string.Empty;

    // Draft generation output
    public string? Draft { get; init; }

    // AI review output
    public int? AiQualityScore { get; init; }
    public string? AiFeedback { get; init; }

    // Human approval output
    public ApprovalDecision? HumanApproval { get; init; }

    // Publication output
    public string? PublishedUrl { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }

    // Audit trail (accumulates across all steps)
    public IReadOnlyList<AuditEntry> AuditEntries { get; init; } = [];
}
```

**Why this design?**

- `AiQualityScore` + `AiFeedback`: Human reviewer sees AI's assessment
- `HumanApproval`: Captures who approved, when, and their feedback
- `PublishedUrl`: Needed for compensation (unpublish requires knowing where)
- `AuditEntries`: Accumulates, never replaces—complete history

### The Audit Entry Record

```csharp
public sealed record AuditEntry(
    DateTimeOffset Timestamp,    // When
    string Action,               // What happened
    string Actor,                // Who (AI, System, or human ID)
    string Details);             // Context and reasoning
```

**Every step adds an audit entry**. This is non-negotiable for accountability.

### The Workflow Definition

```csharp
public static WorkflowDefinition<ContentState> Create() =>
    Workflow<ContentState>
        .Create("content-pipeline")
        .StartWith<GenerateDraft>()      // AI generates initial draft
        .Then<AiReviewContent>()         // AI scores quality
        .Then<AwaitHumanApproval>()      // Human approves or rejects
        .Finally<PublishContent>();       // System publishes
```

**Reading this definition**:
1. AI generates draft from title
2. AI reviews and scores the draft
3. Human reviews AI's work and approves
4. System publishes approved content

### The Human Approval Step

```csharp
public async Task<StepResult<ContentState>> ExecuteAsync(
    ContentState state,
    StepContext context,
    CancellationToken cancellationToken)
{
    // Get human decision (may involve external service, queue, UI, etc.)
    var decision = await _approvalService.GetApprovalAsync(
        state.Draft!,
        state.AiFeedback,
        state.AiQualityScore,
        cancellationToken);

    // Create audit entry with human's identity
    var auditEntry = new AuditEntry(
        Timestamp: _timeProvider.GetUtcNow(),
        Action: "Human Approval Received",
        Actor: decision.ReviewerId,  // Who approved
        Details: decision.Approved
            ? "Content approved for publication"
            : $"Content rejected: {decision.Feedback}");

    return state with
    {
        HumanApproval = decision,
        AuditEntries = [.. state.AuditEntries, auditEntry],
    };
}
```

**Key points**:
- Human sees the AI's feedback and score before deciding
- Reviewer's identity is captured in the audit trail
- Both approval and rejection create audit entries

### The Compensation Step

```csharp
public async Task<StepResult<ContentState>> ExecuteAsync(
    ContentState state,
    StepContext context,
    CancellationToken cancellationToken)
{
    // Skip if nothing to unpublish
    if (string.IsNullOrEmpty(state.PublishedUrl))
    {
        return StepResult<ContentState>.FromState(state);
    }

    // Attempt to unpublish
    var unpublished = await _publishingService.UnpublishAsync(
        state.PublishedUrl,
        cancellationToken);

    if (!unpublished)
    {
        // Compensation failed—record but don't throw
        var failedAudit = new AuditEntry(
            Timestamp: _timeProvider.GetUtcNow(),
            Action: "Content Unpublish Failed (Compensation)",
            Actor: "System",
            Details: $"Failed to remove: {state.PublishedUrl}");

        return state with
        {
            AuditEntries = [.. state.AuditEntries, failedAudit],
        };
    }

    // Compensation succeeded—clear published state
    var auditEntry = new AuditEntry(
        Timestamp: _timeProvider.GetUtcNow(),
        Action: "Content Unpublished (Compensation)",
        Actor: "System",
        Details: $"Removed from: {state.PublishedUrl}");

    return state with
    {
        PublishedUrl = null,
        PublishedAt = null,
        AuditEntries = [.. state.AuditEntries, auditEntry],
    };
}
```

**Compensation design principles**:
- Idempotent: Safe to call multiple times
- Graceful degradation: Records failure, doesn't throw
- Full audit: Both success and failure are logged

---

## The "Aha Moment"

> **Trust in AI systems comes from transparency and control, not from the AI being perfect.**
>
> The audit trail isn't bureaucratic overhead—it's the answer to "How did this get published?" when something goes wrong at 2 AM. The human approval gate isn't a bottleneck—it's the difference between "AI error" and "approved decision with known risk."
>
> ContentPipeline shows that **velocity with accountability** is possible: AI does the heavy lifting while humans remain in control of final outcomes.

---

## Running the Sample

```bash
dotnet run --project samples/ContentPipeline
```

### What You'll See

```text
===========================================
  Content Pipeline Sample Application
===========================================

Workflow: content-pipeline
Steps: GenerateDraft -> AiReviewContent -> AwaitHumanApproval -> PublishContent

Starting workflow: 3f2504e0-4f89-11d3-9a0c-0305e82c3301
Content title: Introduction to AI-Powered Content Generation

--- Step 1: Generate Draft ---
Draft generated: 1277 characters

--- Step 2: AI Review Content ---
AI Quality Score: 90%
AI Feedback: Excellent content with comprehensive coverage of the topic.
             Clear structure and engaging introduction.

--- Step 3: Await Human Approval ---
Reviewer: editor-jane
Decision: APPROVED
Feedback: Great article! Ready for publication.

--- Step 4: Publish Content ---
Published URL: https://example.com/articles/3f2504e0-4f89-11d3-9a0c-0305e82c3301

===========================================
             AUDIT TRAIL
===========================================
[2024-03-15T10:00:00Z] Draft Generated
  Actor: AI
  Details: Generated initial draft for: Introduction to AI-Powered Content Generation

[2024-03-15T10:00:05Z] AI Review Completed
  Actor: AI
  Details: Quality score: 90%. Excellent content with comprehensive coverage...

[2024-03-15T11:30:00Z] Human Approval Received
  Actor: editor-jane
  Details: Content approved for publication

[2024-03-15T11:35:00Z] Content Published
  Actor: System
  Details: Published to: https://example.com/articles/3f2504e0-4f89-11d3-9a0c-0305e82c3301
```

Notice how:
- Each step adds to the audit trail
- The human reviewer's identity is captured
- AI's assessment is visible before human decision
- Timeline shows the full journey from draft to publication

---

## Extension Exercises

### Exercise 1: Add Rejection Handling

When a human rejects content, route to a revision step:

1. Check `HumanApproval.Approved` after the approval step
2. If rejected, route to a `ReviseDraft` step that uses rejection feedback
3. Loop back to `AiReviewContent`

### Exercise 2: Add Quality Threshold

Auto-reject drafts below a quality threshold:

1. Add a `MinimumQualityScore` constant (e.g., 70)
2. After `AiReviewContent`, check if score meets threshold
3. If below threshold, skip human approval and route to revision

### Exercise 3: Multi-Approver Workflow

Require multiple approvers for high-stakes content:

1. Add an `Approvals` collection to state (with roles)
2. Create approval steps for different roles (legal, brand, editor)
3. Only proceed to publish when all required approvals are received

---

## Running Tests

```bash
dotnet test samples/ContentPipeline.Tests
```

The tests verify:
- Draft generation produces valid content
- AI review produces score and feedback
- Human approval is captured correctly
- Publication records URL and timestamp
- Compensation (unpublish) clears published state
- Audit entries accumulate correctly

---

## Key Takeaways

1. **Human approval gates create accountability**—you know who approved what
2. **AI review is a helper, not a replacement**—it flags issues for humans to evaluate
3. **Audit trails are non-negotiable**—every step records what happened
4. **Compensation enables graceful rollback**—design explicit undo actions
5. **Immutable state + append patterns = complete history**—nothing is lost

---

## Learn More

- [Approval Flow Pattern](../../docs/examples/approval-flow.md) - Human-in-the-loop patterns
- [Order Processing Example](../../docs/examples/order-processing.md) - Compensation in e-commerce
- [Code Review Example](../../docs/examples/code-review.md) - Multi-reviewer workflows
