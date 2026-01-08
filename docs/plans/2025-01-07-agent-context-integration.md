# Implementation Plan: Agent Context Integration

## Source Design
Link: `docs/designs/2025-01-07-agent-context-integration.md`

## Summary
- **Total tasks:** 24
- **Parallel groups:** 4
- **Estimated test count:** ~45
- **Test framework:** TUnit (async/await)
- **Run command:** `dotnet test`

## Parallelization Strategy

```
Group A: Generator Models (Tasks A1-A4)
    │
    └───┬───> Group B: Context Assembler Emitter (Tasks B1-B5)
        │
Group C: Runtime Abstractions (Tasks C1-C6) ──────────────────┐
    │                                                          │
    └───> Merges with Group B output                          │
                                                               │
Group D: RAG Adapter Enhancement (Tasks D1-D3) ───────────────┤
                                                               │
Group E: OnFailure Verification (Tasks E1-E3) ────────────────┤
                                                               │
                                                               ▼
                                              Group F: Integration (Tasks F1-F3)
```

**Parallel execution:**
- Groups A, C, D, E can start simultaneously in separate worktrees
- Group B depends on Group A completion
- Group F depends on all other groups

---

## Task Breakdown

---

## Group A: Generator Model Extensions

### Task A1: ContextSourceModel Base Type

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write test: `ContextSourceModel_IsAbstractRecord_CannotInstantiate`
   - File: `src/Agentic.Workflow.Generators.Tests/Models/ContextSourceModelTests.cs`
   - Expected failure: Type does not exist
   - Run: `dotnet test --filter "FullyQualifiedName~ContextSourceModelTests"` - MUST FAIL

2. [GREEN] Implement abstract record
   - File: `src/Agentic.Workflow.Generators/Models/ContextSourceModel.cs`
   - Changes: Create `abstract record ContextSourceModel`

3. [REFACTOR] Add XML documentation

**Verification:**
- [ ] Test fails because type doesn't exist
- [ ] Test passes after implementation
- [ ] No extra code beyond test requirements

**Dependencies:** None
**Parallelizable:** Yes (Group A start)

---

### Task A2: StateContextSourceModel Record

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `StateContextSourceModel_WithPropertyPath_StoresPath`
   - `StateContextSourceModel_WithAccessExpression_StoresExpression`
   - File: `src/Agentic.Workflow.Generators.Tests/Models/StateContextSourceModelTests.cs`
   - Expected failure: Type does not exist
   - Run: `dotnet test --filter "FullyQualifiedName~StateContextSourceModelTests"` - MUST FAIL

2. [GREEN] Implement record
   - File: `src/Agentic.Workflow.Generators/Models/ContextSourceModel.cs`
   - Changes: Add `record StateContextSourceModel(string PropertyPath, string PropertyType, string AccessExpression) : ContextSourceModel`

3. [REFACTOR] None needed

**Verification:**
- [ ] Test fails because type doesn't exist
- [ ] Test passes after implementation

**Dependencies:** Task A1
**Parallelizable:** No (sequential in Group A)

---

### Task A3: RetrievalContextSourceModel and Related Records

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `RetrievalContextSourceModel_WithCollectionType_StoresType`
   - `RetrievalContextSourceModel_WithTopK_DefaultsFiveIfNotSet`
   - `RetrievalFilterModel_WithStaticValue_IsStaticReturnsTrue`
   - `RetrievalFilterModel_WithValueExpression_IsStaticReturnsFalse`
   - File: `src/Agentic.Workflow.Generators.Tests/Models/RetrievalContextSourceModelTests.cs`
   - Expected failure: Types do not exist
   - Run: `dotnet test --filter "FullyQualifiedName~RetrievalContextSourceModelTests"` - MUST FAIL

2. [GREEN] Implement records
   - File: `src/Agentic.Workflow.Generators/Models/ContextSourceModel.cs`
   - Changes: Add `RetrievalContextSourceModel`, `RetrievalFilterModel`, `LiteralContextSourceModel`

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests fail because types don't exist
- [ ] Tests pass after implementation

**Dependencies:** Task A1
**Parallelizable:** No (sequential in Group A)

---

### Task A4: Extend StepModel with ContextModel

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `StepModel_WithContext_StoresContextModel`
   - `StepModel_WithoutContext_ContextIsNull`
   - `ContextModel_WithSources_StoresSourcesList`
   - File: `src/Agentic.Workflow.Generators.Tests/Models/StepModelContextTests.cs`
   - Expected failure: Property does not exist on StepModel
   - Run: `dotnet test --filter "FullyQualifiedName~StepModelContextTests"` - MUST FAIL

2. [GREEN] Extend StepModel
   - File: `src/Agentic.Workflow.Generators/Models/StepModel.cs`
   - Changes: Add `ContextModel? Context` property, create `ContextModel` record

3. [REFACTOR] Ensure immutability patterns consistent

**Verification:**
- [ ] Tests fail because property doesn't exist
- [ ] Tests pass after implementation

**Dependencies:** Tasks A1-A3
**Parallelizable:** No (sequential in Group A, but A finishes here)

---

## Group B: Context Assembler Generation

### Task B1: ContextModelExtractor Basic Extraction

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `Extract_StepWithNoContext_ReturnsNull`
   - `Extract_StepWithLiteralContext_ReturnsLiteralSource`
   - File: `src/Agentic.Workflow.Generators.Tests/Helpers/ContextModelExtractorTests.cs`
   - Expected failure: Type does not exist
   - Run: `dotnet test --filter "FullyQualifiedName~ContextModelExtractorTests"` - MUST FAIL

2. [GREEN] Implement extractor
   - File: `src/Agentic.Workflow.Generators/Helpers/ContextModelExtractor.cs`
   - Changes: Create `ContextModelExtractor` with `Extract(StepDefinition)` method

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests fail because type doesn't exist
- [ ] Tests pass after implementation

**Dependencies:** Group A complete
**Parallelizable:** Yes (Group B start, after Group A)

---

### Task B2: ContextModelExtractor State Context Extraction

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `Extract_StepWithStateContext_ReturnsStateSource`
   - `Extract_StateContextWithNestedProperty_ExtractsFullPath`
   - File: `src/Agentic.Workflow.Generators.Tests/Helpers/ContextModelExtractorTests.cs`
   - Expected failure: State context not handled
   - Run: `dotnet test --filter "FullyQualifiedName~ContextModelExtractorTests"` - MUST FAIL

2. [GREEN] Add state context extraction
   - File: `src/Agentic.Workflow.Generators/Helpers/ContextModelExtractor.cs`
   - Changes: Handle `StateContextSource` definition type

3. [REFACTOR] Extract expression parsing helper

**Verification:**
- [ ] Tests fail with state context
- [ ] Tests pass after implementation

**Dependencies:** Task B1
**Parallelizable:** No (sequential in Group B)

---

### Task B3: ContextModelExtractor Retrieval Context Extraction

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `Extract_StepWithRetrievalContext_ReturnsRetrievalSource`
   - `Extract_RetrievalWithFilters_ExtractsAllFilters`
   - `Extract_RetrievalWithDynamicQuery_ExtractsQueryExpression`
   - File: `src/Agentic.Workflow.Generators.Tests/Helpers/ContextModelExtractorTests.cs`
   - Expected failure: Retrieval context not handled
   - Run: `dotnet test --filter "FullyQualifiedName~ContextModelExtractorTests"` - MUST FAIL

2. [GREEN] Add retrieval context extraction
   - File: `src/Agentic.Workflow.Generators/Helpers/ContextModelExtractor.cs`
   - Changes: Handle `RetrievalContextSource` definition type, extract filters

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests fail with retrieval context
- [ ] Tests pass after implementation

**Dependencies:** Task B2
**Parallelizable:** No (sequential in Group B)

---

### Task B4: ContextAssemblerEmitter Basic Generation

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `Emit_StepWithContext_GeneratesAssemblerClass`
   - `Emit_StepWithContext_ImplementsIContextAssembler`
   - `Emit_StepWithContext_HasCorrectNamespace`
   - File: `src/Agentic.Workflow.Generators.Tests/Emitters/ContextAssemblerEmitterTests.cs`
   - Expected failure: Emitter type does not exist
   - Run: `dotnet test --filter "FullyQualifiedName~ContextAssemblerEmitterTests"` - MUST FAIL

2. [GREEN] Implement emitter
   - File: `src/Agentic.Workflow.Generators/Emitters/ContextAssemblerEmitter.cs`
   - Changes: Create emitter implementing `ISourceEmitter` pattern

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests fail because emitter doesn't exist
- [ ] Tests pass after implementation

**Dependencies:** Task B3
**Parallelizable:** No (sequential in Group B)

---

### Task B5: ContextAssemblerEmitter Full Generation

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `Emit_WithStateContext_GeneratesStateAccessor`
   - `Emit_WithRetrievalContext_GeneratesSearchCall`
   - `Emit_WithLiteralContext_GeneratesLiteralString`
   - `Emit_WithMultipleSources_GeneratesAllInOrder`
   - File: `src/Agentic.Workflow.Generators.Tests/Emitters/ContextAssemblerEmitterTests.cs`
   - Expected failure: Generation incomplete
   - Run: `dotnet test --filter "FullyQualifiedName~ContextAssemblerEmitterTests"` - MUST FAIL

2. [GREEN] Complete emitter implementation
   - File: `src/Agentic.Workflow.Generators/Emitters/ContextAssemblerEmitter.cs`
   - Changes: Add source-specific code generation for each context type

3. [REFACTOR] Extract string builders per source type

**Verification:**
- [ ] Tests fail with incomplete generation
- [ ] Tests pass after implementation

**Dependencies:** Task B4
**Parallelizable:** No (sequential in Group B, B finishes here)

---

## Group C: Runtime Abstractions

### Task C1: IContextAssembler Interface

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write test: `IContextAssembler_Interface_ExistsWithCorrectSignature`
   - File: `src/Agentic.Workflow.Agents.Tests/Abstractions/IContextAssemblerTests.cs`
   - Expected failure: Interface does not exist
   - Run: `dotnet test --filter "FullyQualifiedName~IContextAssemblerTests"` - MUST FAIL

2. [GREEN] Implement interface
   - File: `src/Agentic.Workflow.Agents/Abstractions/IContextAssembler.cs`
   - Changes: Create `IContextAssembler<TState>` with `AssembleAsync` method

3. [REFACTOR] Add XML documentation

**Verification:**
- [ ] Test fails because interface doesn't exist
- [ ] Test passes after implementation

**Dependencies:** None
**Parallelizable:** Yes (Group C start)

---

### Task C2: ContextSegment Hierarchy

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `ContextSegment_IsAbstract_CannotInstantiate`
   - `StateContextSegment_ToPromptString_ReturnsValueString`
   - `LiteralContextSegment_ToPromptString_ReturnsLiteral`
   - `RetrievalContextSegment_ToPromptString_JoinsResults`
   - File: `src/Agentic.Workflow.Agents.Tests/Models/ContextSegmentTests.cs`
   - Expected failure: Types do not exist
   - Run: `dotnet test --filter "FullyQualifiedName~ContextSegmentTests"` - MUST FAIL

2. [GREEN] Implement segment types
   - File: `src/Agentic.Workflow.Agents/Models/ContextSegment.cs`
   - Changes: Create abstract `ContextSegment` and concrete implementations

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests fail because types don't exist
- [ ] Tests pass after implementation

**Dependencies:** None
**Parallelizable:** Yes (parallel with C1)

---

### Task C3: RetrievalResult Record

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `RetrievalResult_WithContent_StoresContent`
   - `RetrievalResult_WithScore_StoresScore`
   - `RetrievalResult_WithMetadata_StoresMetadata`
   - File: `src/Agentic.Workflow.Agents.Tests/Models/RetrievalResultTests.cs`
   - Expected failure: Type does not exist
   - Run: `dotnet test --filter "FullyQualifiedName~RetrievalResultTests"` - MUST FAIL

2. [GREEN] Implement record
   - File: `src/Agentic.Workflow.Agents/Models/RetrievalResult.cs`
   - Changes: Create `RetrievalResult` record with properties

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests fail because type doesn't exist
- [ ] Tests pass after implementation

**Dependencies:** None
**Parallelizable:** Yes (parallel with C1, C2)

---

### Task C4: AssembledContext Class

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `AssembledContext_Empty_HasNoSegments`
   - `AssembledContext_WithSegments_StoresSegments`
   - `AssembledContext_ToPromptString_JoinsSegments`
   - `AssembledContext_IsEmpty_ReturnsTrueWhenNoSegments`
   - File: `src/Agentic.Workflow.Agents.Tests/Models/AssembledContextTests.cs`
   - Expected failure: Type does not exist
   - Run: `dotnet test --filter "FullyQualifiedName~AssembledContextTests"` - MUST FAIL

2. [GREEN] Implement class
   - File: `src/Agentic.Workflow.Agents/Models/AssembledContext.cs`
   - Changes: Create `AssembledContext` with segments, prompt string generation

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests fail because type doesn't exist
- [ ] Tests pass after implementation

**Dependencies:** Tasks C2, C3
**Parallelizable:** No (depends on C2, C3)

---

### Task C5: AssembledContextBuilder

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `AddStateContext_WithValue_AddsStateSegment`
   - `AddRetrievalContext_WithResults_AddsRetrievalSegment`
   - `AddLiteralContext_WithString_AddsLiteralSegment`
   - `Build_WithMultipleSources_ReturnsAssembledContext`
   - File: `src/Agentic.Workflow.Agents.Tests/Models/AssembledContextBuilderTests.cs`
   - Expected failure: Type does not exist
   - Run: `dotnet test --filter "FullyQualifiedName~AssembledContextBuilderTests"` - MUST FAIL

2. [GREEN] Implement builder
   - File: `src/Agentic.Workflow.Agents/Models/AssembledContextBuilder.cs`
   - Changes: Create builder with fluent Add methods and Build

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests fail because type doesn't exist
- [ ] Tests pass after implementation

**Dependencies:** Task C4
**Parallelizable:** No (depends on C4)

---

### Task C6: AgentStepBase Abstract Class

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `AgentStepBase_ExecuteAsync_CallsAssemblerWhenPresent`
   - `AgentStepBase_ExecuteAsync_SkipsAssemblerWhenNull`
   - `AgentStepBase_BuildMessages_IncludesSystemPrompt`
   - `AgentStepBase_BuildMessages_IncludesContextWhenNotEmpty`
   - File: `src/Agentic.Workflow.Agents.Tests/AgentStepBaseTests.cs`
   - Expected failure: Type does not exist
   - Run: `dotnet test --filter "FullyQualifiedName~AgentStepBaseTests"` - MUST FAIL

2. [GREEN] Implement base class
   - File: `src/Agentic.Workflow.Agents/AgentStepBase.cs`
   - Changes: Create `AgentStepBase<TState>` implementing `IAgentStep<TState>`

3. [REFACTOR] Extract message building to protected virtual method

**Verification:**
- [ ] Tests fail because type doesn't exist
- [ ] Tests pass after implementation

**Dependencies:** Tasks C1, C4, C5
**Parallelizable:** No (depends on C1, C4, C5, C finishes here)

---

## Group D: RAG Adapter Enhancement

### Task D1: IRagCollection Marker Interface

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write test: `IRagCollection_IsMarkerInterface_HasNoMembers`
   - File: `src/Agentic.Workflow.Rag.Tests/Abstractions/IRagCollectionTests.cs`
   - Expected failure: Interface does not exist
   - Run: `dotnet test --filter "FullyQualifiedName~IRagCollectionTests"` - MUST FAIL

2. [GREEN] Implement interface
   - File: `src/Agentic.Workflow.Rag/Abstractions/IRagCollection.cs`
   - Changes: Create marker interface `IRagCollection`

3. [REFACTOR] Add XML documentation

**Verification:**
- [ ] Test fails because interface doesn't exist
- [ ] Test passes after implementation

**Dependencies:** None
**Parallelizable:** Yes (Group D start)

---

### Task D2: Typed IVectorSearchAdapter

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `IVectorSearchAdapter_Generic_HasSearchAsyncMethod`
   - `IVectorSearchAdapter_SearchAsync_AcceptsFilters`
   - File: `src/Agentic.Workflow.Rag.Tests/Abstractions/TypedVectorSearchAdapterTests.cs`
   - Expected failure: Generic interface does not exist
   - Run: `dotnet test --filter "FullyQualifiedName~TypedVectorSearchAdapterTests"` - MUST FAIL

2. [GREEN] Implement typed interface
   - File: `src/Agentic.Workflow.Rag/Abstractions/IVectorSearchAdapter.cs`
   - Changes: Add `IVectorSearchAdapter<TCollection>` generic interface

3. [REFACTOR] Ensure non-generic interface still works

**Verification:**
- [ ] Tests fail because generic interface doesn't exist
- [ ] Tests pass after implementation

**Dependencies:** Task D1
**Parallelizable:** No (sequential in Group D)

---

### Task D3: RagServiceExtensions Registration

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `AddRagCollection_WithAdapter_RegistersInDI`
   - `AddRagCollection_Generic_ResolvesCorrectAdapter`
   - File: `src/Agentic.Workflow.Rag.Tests/Extensions/RagServiceExtensionsTests.cs`
   - Expected failure: Extension method does not exist
   - Run: `dotnet test --filter "FullyQualifiedName~RagServiceExtensionsTests"` - MUST FAIL

2. [GREEN] Implement extension
   - File: `src/Agentic.Workflow.Rag/Extensions/RagServiceExtensions.cs`
   - Changes: Create `AddRagCollection<TCollection, TAdapter>` extension

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests fail because extension doesn't exist
- [ ] Tests pass after implementation

**Dependencies:** Task D2
**Parallelizable:** No (sequential in Group D, D finishes here)

---

## Group E: OnFailure Verification & Enhancement

### Task E1: Verify Existing OnFailure Emitter

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `Emit_WorkflowWithOnFailure_GeneratesFailureHandler`
   - `Emit_FailureHandler_CapturesExceptionType`
   - `Emit_FailureHandler_CapturesExceptionMessage`
   - File: `src/Agentic.Workflow.Generators.Tests/Emitters/Saga/SagaFailureHandlerComponentEmitterTests.cs`
   - Expected failure: Test identifies missing exception context capture
   - Run: `dotnet test --filter "FullyQualifiedName~SagaFailureHandlerComponentEmitterTests"` - MUST FAIL (or pass if already implemented)

2. [GREEN] Fix any missing functionality
   - File: `src/Agentic.Workflow.Generators/Emitters/Saga/SagaFailureHandlerComponentEmitter.cs`
   - Changes: Ensure exception context captured in events

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests verify existing behavior or identify gaps
- [ ] Tests pass after any needed fixes

**Dependencies:** None
**Parallelizable:** Yes (Group E start)

---

### Task E2: WorkflowFailedEvent Enhancement

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `WorkflowFailedEvent_HasFailedStepName_Property`
   - `WorkflowFailedEvent_HasExceptionType_Property`
   - `WorkflowFailedEvent_HasStackTrace_Property`
   - File: `src/Agentic.Workflow.Tests/Events/WorkflowFailedEventTests.cs`
   - Expected failure: Properties may not exist or be incomplete
   - Run: `dotnet test --filter "FullyQualifiedName~WorkflowFailedEventTests"` - MUST FAIL (or pass if complete)

2. [GREEN] Enhance event if needed
   - File: Check generated events or base event types
   - Changes: Add missing exception context properties

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests verify event has full exception context
- [ ] Tests pass after any needed enhancements

**Dependencies:** None
**Parallelizable:** Yes (parallel with E1)

---

### Task E3: Step-Level OnFailure Support

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `StepConfiguration_OnFailure_AcceptsHandler`
   - `Emit_StepWithOnFailure_GeneratesStepLevelHandler`
   - File: `src/Agentic.Workflow.Generators.Tests/Emitters/Saga/StepLevelFailureEmitterTests.cs`
   - Expected failure: Step-level failure handling not supported
   - Run: `dotnet test --filter "FullyQualifiedName~StepLevelFailureEmitterTests"` - MUST FAIL

2. [GREEN] Implement step-level failure
   - File: `src/Agentic.Workflow.Generators/Emitters/Saga/SagaStepHandlersEmitter.cs`
   - Changes: Generate try-catch with step-specific failure routing

3. [REFACTOR] Extract failure handler generation helper

**Verification:**
- [ ] Tests fail for step-level failure handling
- [ ] Tests pass after implementation

**Dependencies:** Task E1
**Parallelizable:** No (sequential in Group E, E finishes here)

---

## Group F: Integration

### Task F1: Generator Pipeline Integration

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `Generator_WorkflowWithContext_EmitsContextAssembler`
   - `Generator_ContextAssembler_RegisteredInDIExtensions`
   - File: `src/Agentic.Workflow.Generators.Tests/GeneratorContextIntegrationTests.cs`
   - Expected failure: Generator doesn't emit context assemblers
   - Run: `dotnet test --filter "FullyQualifiedName~GeneratorContextIntegrationTests"` - MUST FAIL

2. [GREEN] Wire emitter into generator
   - File: `src/Agentic.Workflow.Generators/WorkflowSourceGenerator.cs`
   - Changes: Add `ContextAssemblerEmitter` to emission pipeline

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests fail because assemblers not generated
- [ ] Tests pass after wiring

**Dependencies:** Groups A, B, C complete
**Parallelizable:** Yes (Group F start, after A, B, C)

---

### Task F2: Saga Handler Context Injection

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `Emit_SagaHandler_InjectsContextAssemblerWhenConfigured`
   - `Emit_SagaHandler_PassesAssemblerToStep`
   - File: `src/Agentic.Workflow.Generators.Tests/Emitters/Saga/SagaStepHandlersContextTests.cs`
   - Expected failure: Saga handlers don't inject assemblers
   - Run: `dotnet test --filter "FullyQualifiedName~SagaStepHandlersContextTests"` - MUST FAIL

2. [GREEN] Modify saga emitter
   - File: `src/Agentic.Workflow.Generators/Emitters/Saga/SagaStepHandlersEmitter.cs`
   - Changes: Inject `IContextAssembler<TState>?` for steps with context

3. [REFACTOR] None needed

**Verification:**
- [ ] Tests fail because injection not happening
- [ ] Tests pass after modification

**Dependencies:** Task F1
**Parallelizable:** No (sequential in Group F)

---

### Task F3: End-to-End Integration Test

**Phase:** RED → GREEN → REFACTOR

1. [RED] Write tests:
   - `EndToEnd_WorkflowWithRAGContext_AssemblesContextAtRuntime`
   - `EndToEnd_AgentStepBase_ReceivesAssembledContext`
   - File: `src/Agentic.Workflow.Tests/Integration/ContextIntegrationTests.cs`
   - Expected failure: Full pipeline not working
   - Run: `dotnet test --filter "FullyQualifiedName~ContextIntegrationTests"` - MUST FAIL

2. [GREEN] Fix any integration issues
   - Files: Various as needed
   - Changes: Wire all components together

3. [REFACTOR] Clean up test fixtures

**Verification:**
- [ ] Tests fail for end-to-end flow
- [ ] Tests pass after full integration

**Dependencies:** Tasks F1, F2, Group D
**Parallelizable:** No (final task, F finishes here)

---

## Completion Checklist

- [ ] All tests written before implementation
- [ ] All tests pass
- [ ] Code coverage meets 80% threshold
- [ ] Generator emits valid C# for all context configurations
- [ ] Runtime assemblers integrate with AgentStepBase
- [ ] RAG adapters registered via typed DI
- [ ] OnFailure handlers capture full exception context
- [ ] Ready for review

---

## Task Summary by Group

| Group | Tasks | Parallelizable With | Worktree Branch |
|-------|-------|---------------------|-----------------|
| A: Generator Models | A1-A4 | C, D, E | `feature/gen-models` |
| B: Context Emitter | B1-B5 | None (after A) | `feature/context-emitter` |
| C: Runtime Abstractions | C1-C6 | A, D, E | `feature/runtime-abstractions` |
| D: RAG Adapter | D1-D3 | A, C, E | `feature/rag-adapter` |
| E: OnFailure | E1-E3 | A, C, D | `feature/onfailure` |
| F: Integration | F1-F3 | None (after all) | `feature/integration` |

---

## Delegation Recommendation

For optimal parallelization, dispatch to 4 subagents:

1. **Subagent 1:** Groups A → B (sequential, generator focus)
2. **Subagent 2:** Group C (runtime abstractions)
3. **Subagent 3:** Group D (RAG adapters)
4. **Subagent 4:** Group E (OnFailure verification)

After all complete, orchestrator runs Group F for integration.
