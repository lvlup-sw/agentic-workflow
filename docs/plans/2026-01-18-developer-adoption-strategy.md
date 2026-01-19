# Implementation Plan: Developer Adoption Strategy

## Source Design
Link: `docs/designs/2026-01-18-developer-adoption-strategy.md`

## Summary
- Total tasks: 18
- Parallel groups: 3 (one per sample application)
- Estimated test count: ~27 tests

## Overview

This plan creates three runnable sample applications demonstrating Agentic.Workflow capabilities, plus updates to VitePress documentation and README messaging. Each sample is a standalone `dotnet run` project with tests.

**Key Decision:** Samples use mock services (no real LLM calls) for zero-friction running.

---

## Phase 1: Sample Applications

### Group A: Content Publishing Pipeline (Parallel)

#### Task A1: ContentPipeline project structure
**Phase:** RED → GREEN

**TDD Steps:**
1. [RED] Write test: `ContentState_Implements_IWorkflowState`
   - File: `samples/ContentPipeline.Tests/ContentStateTests.cs`
   - Expected failure: Type ContentState not found
   - Run: `dotnet test` - MUST FAIL

2. [GREEN] Create project and state record
   - Files:
     - `samples/ContentPipeline/ContentPipeline.csproj`
     - `samples/ContentPipeline/State/ContentState.cs`
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Witnessed test fail for the right reason
- [ ] Test passes after implementation
- [ ] No extra code beyond test requirements

**Dependencies:** None
**Parallelizable:** Yes (Group A)

---

#### Task A2: ContentPipeline step implementations
**Phase:** RED → GREEN → REFACTOR

**TDD Steps:**
1. [RED] Write test: `GenerateDraft_WithRequest_ProducesDraft`
   - File: `samples/ContentPipeline.Tests/Steps/GenerateDraftTests.cs`
   - Expected failure: Type GenerateDraft not found
   - Run: `dotnet test` - MUST FAIL

2. [GREEN] Implement GenerateDraft step
   - File: `samples/ContentPipeline/Steps/GenerateDraft.cs`
   - Run: `dotnet test` - MUST PASS

3. [RED] Write test: `AiReviewContent_WithDraft_ReturnsQualityScore`
   - File: `samples/ContentPipeline.Tests/Steps/AiReviewContentTests.cs`
   - Expected failure: Type AiReviewContent not found

4. [GREEN] Implement AiReviewContent step
   - File: `samples/ContentPipeline/Steps/AiReviewContent.cs`
   - Run: `dotnet test` - MUST PASS

5. [RED] Write test: `AwaitHumanApproval_WithApprovalDecision_UpdatesState`
   - File: `samples/ContentPipeline.Tests/Steps/AwaitHumanApprovalTests.cs`
   - Expected failure: Type AwaitHumanApproval not found

6. [GREEN] Implement AwaitHumanApproval step
   - File: `samples/ContentPipeline/Steps/AwaitHumanApproval.cs`
   - Run: `dotnet test` - MUST PASS

7. [RED] Write test: `PublishContent_WhenApproved_SetsPublishedState`
   - File: `samples/ContentPipeline.Tests/Steps/PublishContentTests.cs`
   - Expected failure: Type PublishContent not found

8. [GREEN] Implement PublishContent step
   - File: `samples/ContentPipeline/Steps/PublishContent.cs`
   - Run: `dotnet test` - MUST PASS

9. [RED] Write test: `UnpublishContent_WhenCalled_ClearsPublishedState`
   - File: `samples/ContentPipeline.Tests/Steps/UnpublishContentTests.cs`
   - Expected failure: Type UnpublishContent not found

10. [GREEN] Implement UnpublishContent compensation step
    - File: `samples/ContentPipeline/Steps/UnpublishContent.cs`
    - Run: `dotnet test` - MUST PASS

11. [REFACTOR] Extract shared mock LLM service
    - File: `samples/ContentPipeline/Services/MockLlmService.cs`
    - Run: `dotnet test` - MUST STAY GREEN

**Verification:**
- [ ] Each step has dedicated test file
- [ ] Mock services used (no real LLM calls)
- [ ] All tests pass

**Dependencies:** Task A1
**Parallelizable:** Yes (Group A)

---

#### Task A3: ContentPipeline workflow definition
**Phase:** RED → GREEN

**TDD Steps:**
1. [RED] Write test: `ContentWorkflow_Create_ReturnsValidDefinition`
   - File: `samples/ContentPipeline.Tests/ContentWorkflowTests.cs`
   - Expected failure: Type ContentWorkflow not found
   - Run: `dotnet test` - MUST FAIL

2. [GREEN] Create workflow definition
   - File: `samples/ContentPipeline/ContentWorkflow.cs`
   - Pattern: Fluent DSL with `AwaitApproval<T>()` and `Compensate<T>()`
   - Run: `dotnet test` - MUST PASS

3. [RED] Write test: `ContentWorkflow_HappyPath_CompletesSuccessfully`
   - File: `samples/ContentPipeline.Tests/ContentWorkflowTests.cs`
   - Expected failure: Test not found
   - Run: `dotnet test` - MUST FAIL

4. [GREEN] Create in-memory test runner for workflow
   - File: `samples/ContentPipeline.Tests/Fixtures/InMemoryWorkflowRunner.cs`
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Workflow compiles and validates
- [ ] Happy path test runs in-memory
- [ ] No PostgreSQL dependency for tests

**Dependencies:** Tasks A1, A2
**Parallelizable:** Yes (Group A)

---

#### Task A4: ContentPipeline runnable demo
**Phase:** GREEN

**TDD Steps:**
1. [GREEN] Create Program.cs with demo runner
   - File: `samples/ContentPipeline/Program.cs`
   - Pattern: Console demo that runs workflow with mock services
   - Verify: `dotnet run --project samples/ContentPipeline` works

2. [GREEN] Create README.md
   - File: `samples/ContentPipeline/README.md`
   - Include: What it shows, how to run, expected output

**Verification:**
- [ ] `dotnet run` produces console output showing workflow execution
- [ ] README explains the sample clearly

**Dependencies:** Tasks A1, A2, A3
**Parallelizable:** Yes (Group A)

---

### Group B: Multi-Model Router (Parallel)

#### Task B1: MultiModelRouter project structure
**Phase:** RED → GREEN

**TDD Steps:**
1. [RED] Write test: `RouterState_Implements_IWorkflowState`
   - File: `samples/MultiModelRouter.Tests/RouterStateTests.cs`
   - Expected failure: Type RouterState not found
   - Run: `dotnet test` - MUST FAIL

2. [GREEN] Create project and state record
   - Files:
     - `samples/MultiModelRouter/MultiModelRouter.csproj`
     - `samples/MultiModelRouter/State/RouterState.cs`
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Witnessed test fail for the right reason
- [ ] Test passes after implementation
- [ ] State includes Thompson Sampling tracking fields

**Dependencies:** None
**Parallelizable:** Yes (Group B)

---

#### Task B2: MultiModelRouter step implementations
**Phase:** RED → GREEN → REFACTOR

**TDD Steps:**
1. [RED] Write test: `ClassifyQuery_WithQuery_ReturnsCategory`
   - File: `samples/MultiModelRouter.Tests/Steps/ClassifyQueryTests.cs`
   - Expected failure: Type ClassifyQuery not found
   - Run: `dotnet test` - MUST FAIL

2. [GREEN] Implement ClassifyQuery step
   - File: `samples/MultiModelRouter/Steps/ClassifyQuery.cs`
   - Run: `dotnet test` - MUST PASS

3. [RED] Write test: `SelectModel_WithCategory_ReturnsModelViaThompsonSampling`
   - File: `samples/MultiModelRouter.Tests/Steps/SelectModelTests.cs`
   - Expected failure: Type SelectModel not found

4. [GREEN] Implement SelectModel step
   - File: `samples/MultiModelRouter/Steps/SelectModel.cs`
   - Uses `IAgentSelector` from Agentic.Workflow.Agents
   - Run: `dotnet test` - MUST PASS

5. [RED] Write test: `GenerateResponse_WithModel_ReturnsResponse`
   - File: `samples/MultiModelRouter.Tests/Steps/GenerateResponseTests.cs`
   - Expected failure: Type GenerateResponse not found

6. [GREEN] Implement GenerateResponse step
   - File: `samples/MultiModelRouter/Steps/GenerateResponse.cs`
   - Run: `dotnet test` - MUST PASS

7. [RED] Write test: `RecordFeedback_WithFeedback_UpdatesThompsonSamplingPriors`
   - File: `samples/MultiModelRouter.Tests/Steps/RecordFeedbackTests.cs`
   - Expected failure: Type RecordFeedback not found

8. [GREEN] Implement RecordFeedback step
   - File: `samples/MultiModelRouter/Steps/RecordFeedback.cs`
   - Run: `dotnet test` - MUST PASS

9. [REFACTOR] Extract mock model implementations
    - File: `samples/MultiModelRouter/Services/MockModelProviders.cs`
    - Run: `dotnet test` - MUST STAY GREEN

**Verification:**
- [ ] Each step has dedicated test file
- [ ] Thompson Sampling integration verified
- [ ] All tests pass

**Dependencies:** Task B1
**Parallelizable:** Yes (Group B)

---

#### Task B3: MultiModelRouter workflow definition
**Phase:** RED → GREEN

**TDD Steps:**
1. [RED] Write test: `RouterWorkflow_Create_ReturnsValidDefinition`
   - File: `samples/MultiModelRouter.Tests/RouterWorkflowTests.cs`
   - Expected failure: Type RouterWorkflow not found
   - Run: `dotnet test` - MUST FAIL

2. [GREEN] Create workflow definition
   - File: `samples/MultiModelRouter/RouterWorkflow.cs`
   - Pattern: Linear flow with Thompson Sampling selection
   - Run: `dotnet test` - MUST PASS

3. [RED] Write test: `RouterWorkflow_MultipleRuns_ImprovesModelSelection`
   - File: `samples/MultiModelRouter.Tests/RouterWorkflowTests.cs`
   - Expected failure: Test runs but asserts fail
   - This tests that learning actually happens over iterations

4. [GREEN] Wire up learning feedback loop
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Workflow compiles and validates
- [ ] Learning test demonstrates improvement
- [ ] No PostgreSQL dependency for tests

**Dependencies:** Tasks B1, B2
**Parallelizable:** Yes (Group B)

---

#### Task B4: MultiModelRouter runnable demo
**Phase:** GREEN

**TDD Steps:**
1. [GREEN] Create Program.cs with demo runner
   - File: `samples/MultiModelRouter/Program.cs`
   - Pattern: Console demo showing model selection evolving over queries
   - Verify: `dotnet run --project samples/MultiModelRouter` works

2. [GREEN] Create README.md
   - File: `samples/MultiModelRouter/README.md`
   - Include: What it shows, how to run, expected output

**Verification:**
- [ ] `dotnet run` shows Thompson Sampling in action
- [ ] README explains the sample clearly

**Dependencies:** Tasks B1, B2, B3
**Parallelizable:** Yes (Group B)

---

### Group C: Agentic Coder (Parallel)

#### Task C1: AgenticCoder project structure
**Phase:** RED → GREEN

**TDD Steps:**
1. [RED] Write test: `CoderState_Implements_IWorkflowState`
   - File: `samples/AgenticCoder.Tests/CoderStateTests.cs`
   - Expected failure: Type CoderState not found
   - Run: `dotnet test` - MUST FAIL

2. [GREEN] Create project and state record
   - Files:
     - `samples/AgenticCoder/AgenticCoder.csproj`
     - `samples/AgenticCoder/State/CoderState.cs`
   - Include `[Append]` for `Attempts` list
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Witnessed test fail for the right reason
- [ ] Test passes after implementation
- [ ] State includes iteration tracking and [Append] history

**Dependencies:** None
**Parallelizable:** Yes (Group C)

---

#### Task C2: AgenticCoder step implementations
**Phase:** RED → GREEN → REFACTOR

**TDD Steps:**
1. [RED] Write test: `AnalyzeTask_WithDescription_ExtractsRequirements`
   - File: `samples/AgenticCoder.Tests/Steps/AnalyzeTaskTests.cs`
   - Expected failure: Type AnalyzeTask not found
   - Run: `dotnet test` - MUST FAIL

2. [GREEN] Implement AnalyzeTask step
   - File: `samples/AgenticCoder/Steps/AnalyzeTask.cs`
   - Run: `dotnet test` - MUST PASS

3. [RED] Write test: `PlanImplementation_WithAnalysis_CreatesPlan`
   - File: `samples/AgenticCoder.Tests/Steps/PlanImplementationTests.cs`
   - Expected failure: Type PlanImplementation not found

4. [GREEN] Implement PlanImplementation step
   - File: `samples/AgenticCoder/Steps/PlanImplementation.cs`
   - Run: `dotnet test` - MUST PASS

5. [RED] Write test: `GenerateCode_WithPlan_ProducesCode`
   - File: `samples/AgenticCoder.Tests/Steps/GenerateCodeTests.cs`
   - Expected failure: Type GenerateCode not found

6. [GREEN] Implement GenerateCode step
   - File: `samples/AgenticCoder/Steps/GenerateCode.cs`
   - Run: `dotnet test` - MUST PASS

7. [RED] Write test: `RunTests_WithCode_ReturnsResults`
   - File: `samples/AgenticCoder.Tests/Steps/RunTestsTests.cs`
   - Expected failure: Type RunTests not found

8. [GREEN] Implement RunTests step
   - File: `samples/AgenticCoder/Steps/RunTests.cs`
   - Run: `dotnet test` - MUST PASS

9. [RED] Write test: `ReviewResults_TestsFail_TriggersRevision`
   - File: `samples/AgenticCoder.Tests/Steps/ReviewResultsTests.cs`
   - Expected failure: Type ReviewResults not found

10. [GREEN] Implement ReviewResults step
    - File: `samples/AgenticCoder/Steps/ReviewResults.cs`
    - Run: `dotnet test` - MUST PASS

11. [RED] Write test: `AwaitHumanCheckpoint_WithApproval_ProceedsToComplete`
    - File: `samples/AgenticCoder.Tests/Steps/AwaitHumanCheckpointTests.cs`
    - Expected failure: Type AwaitHumanCheckpoint not found

12. [GREEN] Implement AwaitHumanCheckpoint step
    - File: `samples/AgenticCoder/Steps/AwaitHumanCheckpoint.cs`
    - Run: `dotnet test` - MUST PASS

13. [REFACTOR] Extract mock test runner service
    - File: `samples/AgenticCoder/Services/MockTestRunner.cs`
    - Run: `dotnet test` - MUST STAY GREEN

**Verification:**
- [ ] Each step has dedicated test file
- [ ] Iteration loop logic verified
- [ ] All tests pass

**Dependencies:** Task C1
**Parallelizable:** Yes (Group C)

---

#### Task C3: AgenticCoder workflow definition
**Phase:** RED → GREEN

**TDD Steps:**
1. [RED] Write test: `CoderWorkflow_Create_ReturnsValidDefinition`
   - File: `samples/AgenticCoder.Tests/CoderWorkflowTests.cs`
   - Expected failure: Type CoderWorkflow not found
   - Run: `dotnet test` - MUST FAIL

2. [GREEN] Create workflow definition
   - File: `samples/AgenticCoder/CoderWorkflow.cs`
   - Pattern: Uses `RepeatUntil()` for code/test/review loop
   - Run: `dotnet test` - MUST PASS

3. [RED] Write test: `CoderWorkflow_TestsFail_RetriesUpToMax`
   - File: `samples/AgenticCoder.Tests/CoderWorkflowTests.cs`
   - Expected failure: Test not implemented

4. [GREEN] Verify loop detection works
   - Run: `dotnet test` - MUST PASS

5. [RED] Write test: `CoderWorkflow_MaxAttemptsReached_EscalatesToHuman`
   - File: `samples/AgenticCoder.Tests/CoderWorkflowTests.cs`
   - Expected failure: Test not implemented

6. [GREEN] Verify escalation path
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Workflow compiles with `RepeatUntil()` pattern
- [ ] Loop detection verified
- [ ] Escalation path tested

**Dependencies:** Tasks C1, C2
**Parallelizable:** Yes (Group C)

---

#### Task C4: AgenticCoder runnable demo
**Phase:** GREEN

**TDD Steps:**
1. [GREEN] Create Program.cs with demo runner
   - File: `samples/AgenticCoder/Program.cs`
   - Pattern: Console demo showing code iteration
   - Verify: `dotnet run --project samples/AgenticCoder` works

2. [GREEN] Create README.md
   - File: `samples/AgenticCoder/README.md`
   - Include: What it shows, how to run, expected output

**Verification:**
- [ ] `dotnet run` shows iteration loop in action
- [ ] README explains the sample clearly

**Dependencies:** Tasks C1, C2, C3
**Parallelizable:** Yes (Group C)

---

## Phase 2: Documentation Updates (Sequential after Phase 1)

#### Task D1: Update examples/index.md
**Phase:** GREEN

**TDD Steps:**
1. [GREEN] Add "Sample Applications" section
   - File: `docs/examples/index.md`
   - Link to sample directories with run instructions

**Dependencies:** Tasks A4, B4, C4
**Parallelizable:** No

---

#### Task D2: Update homepage with "See It In Action"
**Phase:** GREEN

**TDD Steps:**
1. [GREEN] Add samples showcase section
   - File: `docs/index.md`
   - Pattern: Feature cards linking to samples

**Dependencies:** Task D1
**Parallelizable:** No

---

#### Task D3: Update learn/index.md
**Phase:** GREEN

**TDD Steps:**
1. [GREEN] Add "Try the Samples" section
   - File: `docs/learn/index.md`
   - Quick-start links to each sample

**Dependencies:** Task D2
**Parallelizable:** No

---

## Phase 3: Messaging Updates (Sequential)

#### Task E1: Update README.md messaging
**Phase:** GREEN

**TDD Steps:**
1. [GREEN] Refresh problem statement with use-case focus
   - File: `README.md`
   - Lead with real-world scenarios
   - Link to sample applications

**Dependencies:** Task D3
**Parallelizable:** No

---

## Parallelization Strategy

```
Phase 1 (Parallel):
┌─────────────────────────────────────────────────────────────────────────┐
│                                                                         │
│  Group A (ContentPipeline)    Group B (MultiModelRouter)    Group C (AgenticCoder)     │
│  ├─ Task A1 ──────────────    ├─ Task B1 ──────────────    ├─ Task C1 ──────────────   │
│  ├─ Task A2 (depends A1)      ├─ Task B2 (depends B1)      ├─ Task C2 (depends C1)     │
│  ├─ Task A3 (depends A1,A2)   ├─ Task B3 (depends B1,B2)   ├─ Task C3 (depends C1,C2)  │
│  └─ Task A4 (depends all)     └─ Task B4 (depends all)     └─ Task C4 (depends all)    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
                                    ↓
Phase 2 (Sequential):
  Task D1 → Task D2 → Task D3
                                    ↓
Phase 3 (Sequential):
  Task E1
```

**Worktree Assignment:**
- Worktree 1: Group A (ContentPipeline)
- Worktree 2: Group B (MultiModelRouter)
- Worktree 3: Group C (AgenticCoder)
- Main: Phase 2 & 3 (after integration)

---

## Completion Checklist

- [ ] All 3 sample applications compile and run with `dotnet run`
- [ ] Each sample has README with clear instructions
- [ ] All tests pass: `dotnet test samples/`
- [ ] VitePress docs updated with sample links
- [ ] README messaging refreshed with use-case focus
- [ ] No PostgreSQL required for samples (pure mocks)
