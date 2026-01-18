# Content Pipeline Sample

A sample application demonstrating the Agentic.Workflow library with a content publishing workflow.

## Overview

This sample implements a content publishing pipeline that demonstrates:

- **AI-powered draft generation** - Using a mock LLM service to generate content
- **AI content review** - Automated quality assessment with scoring
- **Human-in-the-loop approval** - `AwaitApproval<T>()` pattern for human oversight
- **Compensation handlers** - `Compensate<T>()` pattern for rollback on post-publish issues
- **Audit trail** - Complete tracking of "Who approved this? What did the AI review say?"

## Workflow Flow

```text
Draft -> AI Review -> Human Approval -> Publish
                           |
                           v (compensation if issues)
                     Unpublish Content
```

## Project Structure

```text
ContentPipeline/
  State/
    ContentState.cs        # Workflow state record
    ApprovalDecision.cs    # Human approval decision record
    AuditEntry.cs          # Audit trail entry record
  Steps/
    GenerateDraft.cs       # AI draft generation step
    AiReviewContent.cs     # AI quality review step
    AwaitHumanApproval.cs  # Human approval checkpoint
    PublishContent.cs      # Content publishing step
    UnpublishContent.cs    # Compensation step (rollback)
  Services/
    ILlmService.cs         # LLM service interface
    MockLlmService.cs      # Mock LLM implementation
    IApprovalService.cs    # Approval service interface
    MockApprovalService.cs # Mock approval implementation
    IPublishingService.cs  # Publishing service interface
    MockPublishingService.cs # Mock publishing implementation
  ContentWorkflow.cs       # Workflow definition
  Program.cs               # Demo entry point
```

## Running the Sample

```bash
dotnet run --project samples/ContentPipeline
```

Expected output:
```text
===========================================
  Content Pipeline Sample Application
===========================================

Workflow: content-pipeline
Steps: GenerateDraft -> AiReviewContent -> AwaitHumanApproval -> PublishContent

Starting workflow: <guid>
Content title: Introduction to AI-Powered Content Generation

--- Step 1: Generate Draft ---
Draft generated: 1277 characters

--- Step 2: AI Review Content ---
AI Quality Score: 90%
AI Feedback: Excellent content with comprehensive coverage...

--- Step 3: Await Human Approval ---
Reviewer: editor-jane
Decision: APPROVED
Feedback: Great article! Ready for publication.

--- Step 4: Publish Content ---
Published URL: https://example.com/articles/...

===========================================
             AUDIT TRAIL
===========================================
[timestamp] Draft Generated
  Actor: AI
  Details: Generated initial draft...

[timestamp] AI Review Completed
  Actor: AI
  Details: Quality score: 90%...

[timestamp] Human Approval Received
  Actor: editor-jane
  Details: Content approved for publication

[timestamp] Content Published
  Actor: System
  Details: Published to: https://example.com/articles/...
```

## Running Tests

```bash
dotnet test --project samples/ContentPipeline.Tests
```

## Key Concepts Demonstrated

### 1. Workflow State (IWorkflowState)

The `ContentState` record implements `IWorkflowState` and tracks:
- Content title and draft
- AI review feedback and quality score
- Human approval decision with reviewer identity
- Published URL and timestamp
- Complete audit trail

### 2. Workflow Steps (IWorkflowStep<TState>)

Each step implements the `IWorkflowStep<ContentState>` interface:

```csharp
public sealed class GenerateDraft : IWorkflowStep<ContentState>
{
    public async Task<StepResult<ContentState>> ExecuteAsync(
        ContentState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        // Generate draft using LLM service
        // Return updated state with audit entry
    }
}
```

### 3. Workflow Definition (Fluent DSL)

The workflow is defined using the fluent builder pattern:

```csharp
public static WorkflowDefinition<ContentState> Create() =>
    Workflow<ContentState>
        .Create("content-pipeline")
        .StartWith<GenerateDraft>()
        .Then<AiReviewContent>()
        .Then<AwaitHumanApproval>()
        .Finally<PublishContent>();
```

### 4. Human-in-the-Loop Approval

The `AwaitHumanApproval` step demonstrates the approval checkpoint:
- Pauses workflow until human decision is received
- Records reviewer identity and timestamp
- Supports both approval and rejection flows

### 5. Compensation (Rollback)

The `UnpublishContent` step demonstrates compensation:
- Rolls back published content if issues arise
- Clears published state and URL
- Records compensation action in audit trail

### 6. Audit Trail

Every step adds an audit entry tracking:
- Timestamp of the action
- Action name (e.g., "Draft Generated", "Human Approval Received")
- Actor (AI, System, or human reviewer ID)
- Details about the action

## Extending the Sample

To add new steps:

1. Create a new step class implementing `IWorkflowStep<ContentState>`
2. Add tests in `ContentPipeline.Tests`
3. Update `ContentWorkflow.Create()` to include the new step
4. Update `Program.cs` to demonstrate the new step

Example: Adding a spell-check step before publishing:

```csharp
public sealed class SpellCheckContent : IWorkflowStep<ContentState>
{
    public async Task<StepResult<ContentState>> ExecuteAsync(
        ContentState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        // Perform spell check
        // Return updated state
    }
}
```
