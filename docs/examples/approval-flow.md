# Approval Flow Example

This example demonstrates human-in-the-loop workflows using `AwaitApproval` for document review and sign-off.

## Overview

Approval workflows pause execution until a human provides input. The workflow persists its state, allowing hours or days to pass before resumption. Timeouts and escalation paths handle cases where approval is not received.

## State Definition

```csharp
[WorkflowState]
public record DocumentState : IWorkflowState
{
    public Guid WorkflowId { get; init; }
    public Document Document { get; init; } = null!;
    public string? DraftContent { get; init; }
    public LegalReviewResult? LegalReview { get; init; }
    public ApprovalDecision? Approval { get; init; }
    public bool IsPublished { get; init; }
    public bool StakeholdersNotified { get; init; }
    public bool IsEscalated { get; init; }
}

public record Document(
    string Title,
    string Author,
    DocumentType Type,
    string Content);

public record LegalReviewResult(
    bool HasIssues,
    IReadOnlyList<string> Issues,
    string ReviewerComments);

public record ApprovalDecision(
    bool Approved,
    string ApproverId,
    DateTimeOffset DecisionTime,
    string? Comments);

public enum DocumentType { Contract, Policy, Procedure, Marketing }
```

## Workflow Definition

```csharp
var workflow = Workflow<DocumentState>
    .Create("document-approval")
    .StartWith<DraftDocument>()
    .Then<LegalReview>()
    .AwaitApproval<LegalTeam>(options => options
        .WithTimeout(TimeSpan.FromDays(2))
        .OnTimeout(flow => flow.Then<EscalateToManager>())
        .OnRejection(flow => flow
            .Then<AddressLegalConcerns>()
            .Then<LegalReview>()))
    .Then<PublishDocument>()
    .Finally<NotifyStakeholders>();
```

## Approval Options

### Basic Approval

```csharp
.AwaitApproval<LegalTeam>()
```

Waits indefinitely for approval from the `LegalTeam` role.

### With Timeout

```csharp
.AwaitApproval<LegalTeam>(options => options
    .WithTimeout(TimeSpan.FromDays(2)))
```

Fails the workflow if no approval is received within 2 days.

### Timeout with Escalation

```csharp
.AwaitApproval<LegalTeam>(options => options
    .WithTimeout(TimeSpan.FromDays(2))
    .OnTimeout(flow => flow.Then<EscalateToManager>()))
```

Routes to escalation path if timeout occurs, then continues to the next step.

### With Rejection Handling

```csharp
.AwaitApproval<LegalTeam>(options => options
    .OnRejection(flow => flow
        .Then<AddressLegalConcerns>()
        .Then<LegalReview>()))
```

If rejected, executes the rejection path and re-requests approval.

### Multiple Approvers

```csharp
.AwaitApproval<LegalTeam>(options => options
    .RequireAll()  // All team members must approve
    .WithQuorum(2))  // Or: at least 2 must approve
```

## Step Implementations

### DraftDocument

```csharp
public class DraftDocument : IWorkflowStep<DocumentState>
{
    private readonly IDocumentDrafter _drafter;

    public DraftDocument(IDocumentDrafter drafter)
    {
        _drafter = drafter;
    }

    public async Task<StepResult<DocumentState>> ExecuteAsync(
        DocumentState state,
        StepContext context,
        CancellationToken ct)
    {
        var draft = await _drafter.CreateDraftAsync(state.Document, ct);

        return state
            .With(s => s.DraftContent, draft)
            .AsResult();
    }
}
```

### LegalReview

```csharp
public class LegalReview : IWorkflowStep<DocumentState>
{
    private readonly ILegalReviewService _legalService;

    public LegalReview(ILegalReviewService legalService)
    {
        _legalService = legalService;
    }

    public async Task<StepResult<DocumentState>> ExecuteAsync(
        DocumentState state,
        StepContext context,
        CancellationToken ct)
    {
        var review = await _legalService.ReviewAsync(state.DraftContent!, ct);

        return state
            .With(s => s.LegalReview, review)
            .AsResult();
    }
}
```

### LegalTeam Approver

```csharp
public class LegalTeam : IApprover<DocumentState>
{
    public string Role => "legal-team";

    public ApprovalRequest CreateRequest(DocumentState state)
    {
        return new ApprovalRequest
        {
            Title = $"Legal Approval Required: {state.Document.Title}",
            Description = "Please review the document and legal analysis.",
            Context = new Dictionary<string, object>
            {
                ["DocumentTitle"] = state.Document.Title,
                ["DocumentType"] = state.Document.Type.ToString(),
                ["LegalIssues"] = state.LegalReview?.Issues ?? [],
                ["ReviewerComments"] = state.LegalReview?.ReviewerComments ?? ""
            }
        };
    }

    public DocumentState ApplyApproval(DocumentState state, ApprovalDecision decision)
    {
        return state.With(s => s.Approval, decision);
    }
}
```

### EscalateToManager

```csharp
public class EscalateToManager : IWorkflowStep<DocumentState>
{
    private readonly IEscalationService _escalation;

    public EscalateToManager(IEscalationService escalation)
    {
        _escalation = escalation;
    }

    public async Task<StepResult<DocumentState>> ExecuteAsync(
        DocumentState state,
        StepContext context,
        CancellationToken ct)
    {
        await _escalation.EscalateAsync(
            $"Document approval timeout: {state.Document.Title}",
            state.WorkflowId,
            ct);

        return state
            .With(s => s.IsEscalated, true)
            .AsResult();
    }
}
```

### AddressLegalConcerns

```csharp
public class AddressLegalConcerns : IWorkflowStep<DocumentState>
{
    private readonly IDocumentReviser _reviser;

    public AddressLegalConcerns(IDocumentReviser reviser)
    {
        _reviser = reviser;
    }

    public async Task<StepResult<DocumentState>> ExecuteAsync(
        DocumentState state,
        StepContext context,
        CancellationToken ct)
    {
        var revisedContent = await _reviser.AddressIssuesAsync(
            state.DraftContent!,
            state.LegalReview!.Issues,
            state.Approval?.Comments,
            ct);

        return state
            .With(s => s.DraftContent, revisedContent)
            .With(s => s.LegalReview, null)  // Clear for re-review
            .With(s => s.Approval, null)
            .AsResult();
    }
}
```

### PublishDocument

```csharp
public class PublishDocument : IWorkflowStep<DocumentState>
{
    private readonly IPublishingService _publishing;

    public PublishDocument(IPublishingService publishing)
    {
        _publishing = publishing;
    }

    public async Task<StepResult<DocumentState>> ExecuteAsync(
        DocumentState state,
        StepContext context,
        CancellationToken ct)
    {
        await _publishing.PublishAsync(
            state.Document.Title,
            state.DraftContent!,
            ct);

        return state
            .With(s => s.IsPublished, true)
            .AsResult();
    }
}
```

### NotifyStakeholders

```csharp
public class NotifyStakeholders : IWorkflowStep<DocumentState>
{
    private readonly INotificationService _notifications;

    public NotifyStakeholders(INotificationService notifications)
    {
        _notifications = notifications;
    }

    public async Task<StepResult<DocumentState>> ExecuteAsync(
        DocumentState state,
        StepContext context,
        CancellationToken ct)
    {
        await _notifications.NotifyDocumentPublishedAsync(
            state.Document.Title,
            state.Document.Author,
            ct);

        return state
            .With(s => s.StakeholdersNotified, true)
            .AsResult();
    }
}
```

## Submitting Approvals

```csharp
public class ApprovalController : ControllerBase
{
    private readonly IApprovalService _approvals;

    public ApprovalController(IApprovalService approvals)
    {
        _approvals = approvals;
    }

    [HttpPost("{workflowId}/approve")]
    public async Task<IActionResult> Approve(
        Guid workflowId,
        [FromBody] ApprovalRequest request)
    {
        await _approvals.SubmitDecisionAsync(workflowId, new ApprovalDecision(
            Approved: true,
            ApproverId: User.Identity!.Name!,
            DecisionTime: DateTimeOffset.UtcNow,
            Comments: request.Comments));

        return Ok();
    }

    [HttpPost("{workflowId}/reject")]
    public async Task<IActionResult> Reject(
        Guid workflowId,
        [FromBody] RejectionRequest request)
    {
        await _approvals.SubmitDecisionAsync(workflowId, new ApprovalDecision(
            Approved: false,
            ApproverId: User.Identity!.Name!,
            DecisionTime: DateTimeOffset.UtcNow,
            Comments: request.Reason));

        return Ok();
    }
}
```

## Generated Phase Enum

```csharp
public enum DocumentApprovalPhase
{
    NotStarted,
    DraftDocument,
    LegalReview,
    AwaitingApproval,
    EscalateToManager,
    AddressLegalConcerns,
    PublishDocument,
    NotifyStakeholders,
    Completed,
    Failed
}
```

## Querying Pending Approvals

```csharp
// Find all documents awaiting legal approval
var pending = await session
    .Query<DocumentApprovalReadModel>()
    .Where(d => d.CurrentPhase == DocumentApprovalPhase.AwaitingApproval)
    .ToListAsync();
```

## Key Points

- Approval steps persist workflow state and wait for external input
- Timeouts prevent workflows from waiting indefinitely
- Escalation paths handle timeout scenarios
- Rejection paths enable iterative review cycles
- The saga resumes exactly where it paused when approval arrives
- Approvals can require multiple approvers or quorum
