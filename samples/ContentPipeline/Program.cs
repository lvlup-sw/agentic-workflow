// =============================================================================
// <copyright file="Program.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Steps;
using ContentPipeline;
using ContentPipeline.Services;
using ContentPipeline.State;
using ContentPipeline.Steps;

Console.WriteLine("===========================================");
Console.WriteLine("  Content Pipeline Sample Application");
Console.WriteLine("===========================================");
Console.WriteLine();

// Create workflow definition
var workflowDefinition = ContentWorkflow.Create();
Console.WriteLine($"Workflow: {workflowDefinition.Name}");
Console.WriteLine($"Steps: {string.Join(" -> ", workflowDefinition.Steps.Select(s => s.StepType.Name))}");
Console.WriteLine();

// Create initial state
var workflowId = Guid.NewGuid();
var initialState = new ContentState
{
    WorkflowId = workflowId,
    Title = "Introduction to AI-Powered Content Generation",
};

Console.WriteLine($"Starting workflow: {workflowId}");
Console.WriteLine($"Content title: {initialState.Title}");
Console.WriteLine();

// Create services
var timeProvider = TimeProvider.System;
var llmService = new MockLlmService();
var approvalService = new MockApprovalService(
    shouldApprove: true,
    reviewerId: "editor-jane",
    feedback: "Great article! Ready for publication.");
var publishingService = new MockPublishingService();

// Execute workflow steps manually (demonstrating step execution)
var currentState = initialState;

// Step 1: Generate Draft
Console.WriteLine("--- Step 1: Generate Draft ---");
var generateDraft = new GenerateDraft(llmService, timeProvider);
var context1 = StepContext.Create(workflowId, nameof(GenerateDraft), "GenerateDraft");
var result1 = await generateDraft.ExecuteAsync(currentState, context1, CancellationToken.None);
currentState = result1.UpdatedState;
Console.WriteLine($"Draft generated: {currentState.Draft.Length} characters");
Console.WriteLine($"Preview: {currentState.Draft[..Math.Min(200, currentState.Draft.Length)]}...");
Console.WriteLine();

// Step 2: AI Review Content
Console.WriteLine("--- Step 2: AI Review Content ---");
var aiReview = new AiReviewContent(llmService, timeProvider);
var context2 = StepContext.Create(workflowId, nameof(AiReviewContent), "AiReviewContent");
var result2 = await aiReview.ExecuteAsync(currentState, context2, CancellationToken.None);
currentState = result2.UpdatedState;
Console.WriteLine($"AI Quality Score: {currentState.AiQualityScore:P0}");
Console.WriteLine($"AI Feedback: {currentState.AiReviewFeedback}");
Console.WriteLine();

// Step 3: Await Human Approval
Console.WriteLine("--- Step 3: Await Human Approval ---");
var humanApproval = new AwaitHumanApproval(approvalService, timeProvider);
var context3 = StepContext.Create(workflowId, nameof(AwaitHumanApproval), "AwaitHumanApproval");
var result3 = await humanApproval.ExecuteAsync(currentState, context3, CancellationToken.None);
currentState = result3.UpdatedState;
Console.WriteLine($"Reviewer: {currentState.HumanDecision?.ReviewerId}");
Console.WriteLine($"Decision: {(currentState.HumanDecision?.Approved == true ? "APPROVED" : "REJECTED")}");
Console.WriteLine($"Feedback: {currentState.HumanDecision?.Feedback}");
Console.WriteLine();

// Step 4: Publish Content
Console.WriteLine("--- Step 4: Publish Content ---");
var publish = new PublishContent(publishingService, timeProvider);
var context4 = StepContext.Create(workflowId, nameof(PublishContent), "PublishContent");
var result4 = await publish.ExecuteAsync(currentState, context4, CancellationToken.None);
currentState = result4.UpdatedState;
Console.WriteLine($"Published URL: {currentState.PublishedUrl}");
Console.WriteLine($"Published At: {currentState.PublishedAt}");
Console.WriteLine();

// Display Audit Trail
Console.WriteLine("===========================================");
Console.WriteLine("             AUDIT TRAIL");
Console.WriteLine("===========================================");
foreach (var entry in currentState.AuditEntries)
{
    Console.WriteLine($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] {entry.Action}");
    Console.WriteLine($"  Actor: {entry.Actor}");
    if (entry.Details != null)
    {
        Console.WriteLine($"  Details: {entry.Details}");
    }

    Console.WriteLine();
}

// Demonstrate compensation (unpublish)
Console.WriteLine("===========================================");
Console.WriteLine("     DEMONSTRATING COMPENSATION");
Console.WriteLine("===========================================");
Console.WriteLine("Simulating post-publication issue...");
Console.WriteLine();

var unpublish = new UnpublishContent(publishingService, timeProvider);
var context5 = StepContext.Create(workflowId, nameof(UnpublishContent), "UnpublishContent");
var compensationResult = await unpublish.ExecuteAsync(currentState, context5, CancellationToken.None);
var compensatedState = compensationResult.UpdatedState;

Console.WriteLine($"Content unpublished: {compensatedState.PublishedUrl is null}");
Console.WriteLine();

// Final audit entry from compensation
var lastAuditEntry = compensatedState.AuditEntries.Last();
Console.WriteLine($"Compensation audit entry:");
Console.WriteLine($"  Action: {lastAuditEntry.Action}");
Console.WriteLine($"  Actor: {lastAuditEntry.Actor}");
Console.WriteLine($"  Details: {lastAuditEntry.Details}");
Console.WriteLine();

Console.WriteLine("===========================================");
Console.WriteLine("        WORKFLOW COMPLETED");
Console.WriteLine("===========================================");
