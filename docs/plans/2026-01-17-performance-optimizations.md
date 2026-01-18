# Implementation Plan: Performance Optimizations Phase A

## Source Design

Link: `docs/designs/2026-01-07-performance-optimizations.md`

## Summary

- **Total tasks:** 24 (12 optimizations × 2 TDD cycles each)
- **Parallel groups:** 4
- **Estimated test count:** ~36 new tests
- **Scope:** Phase A surgical hot-path optimizations only

Phase B and C will be planned after Phase A measurements validate impact.

---

## Parallelization Strategy

| Group | Subsystem | Tasks | Worktree |
|-------|-----------|-------|----------|
| A | Thompson Sampling | A.1, A.2, A.3 | `.worktrees/perf-thompson` |
| B | Loop Detection | A.4, A.5, A.6 | `.worktrees/perf-loop` |
| C | Budget & Ledgers | A.7, A.8, A.9 | `.worktrees/perf-ledgers` |
| D | Source Generators | A.10, A.11, A.12 | `.worktrees/perf-generators` |

All groups are independent and can execute in parallel.

---

## Group A: Thompson Sampling Optimizations

### Task A.1: Parallelize Belief Fetching

**File:** `src/Agentic.Workflow.Infrastructure/Selection/ThompsonSamplingAgentSelector.cs`
**Test File:** `src/Agentic.Workflow.Infrastructure.Tests/Selection/ThompsonSamplingSelectorTests.cs`

**TDD Steps:**

1. [RED] Write test: `SelectAgentAsync_MultipleCandidates_FetchesBeliefsConcurrently`
   - File: `ThompsonSamplingSelectorTests.cs`
   - Expected failure: Test uses mock belief store with delays; current sequential fetch takes N×delay
   - Verify parallel fetch completes in ~1×delay
   - Run: `dotnet test --filter "SelectAgentAsync_MultipleCandidates_FetchesBeliefsConcurrently"` - MUST FAIL

2. [GREEN] Implement parallel fetch with `Task.WhenAll()`
   - File: `ThompsonSamplingAgentSelector.cs` lines 82-99
   - Change sequential `foreach` to `Select` + `Task.WhenAll()`
   - Run: `dotnet test` - MUST PASS

3. [REFACTOR] Extract belief fetch logic to private helper
   - Run: `dotnet test` - MUST STAY GREEN

**Verification:**
- [ ] Witnessed test fail (sequential timing exceeded threshold)
- [ ] Test passes after parallel implementation
- [ ] No extra code beyond test requirements

**Dependencies:** None
**Parallelizable:** Yes (Group A)

---

### Task A.2: Add Secondary Indices to Belief Store

**File:** `src/Agentic.Workflow.Infrastructure/Selection/InMemoryBeliefStore.cs`
**Test File:** `src/Agentic.Workflow.Infrastructure.Tests/Selection/InMemoryBeliefStoreTests.cs`

**TDD Steps:**

1. [RED] Write test: `GetBeliefsForAgentAsync_ManyBeliefs_ReturnsInConstantTime`
   - File: `InMemoryBeliefStoreTests.cs`
   - Expected failure: O(n) scan on large dataset exceeds threshold
   - Run: `dotnet test --filter "GetBeliefsForAgentAsync_ManyBeliefs_ReturnsInConstantTime"` - MUST FAIL

2. [GREEN] Add `_byAgent` reverse index
   - File: `InMemoryBeliefStore.cs` lines 82-84
   - Add `ConcurrentDictionary<string, ConcurrentBag<AgentBelief>> _byAgent`
   - Update on `UpdateBeliefAsync`, query on `GetBeliefsForAgentAsync`
   - Run: `dotnet test` - MUST PASS

3. [RED] Write test: `GetBeliefsForCategoryAsync_ManyBeliefs_ReturnsInConstantTime`
   - Expected failure: O(n) scan exceeds threshold
   - Run: `dotnet test --filter "GetBeliefsForCategoryAsync_ManyBeliefs_ReturnsInConstantTime"` - MUST FAIL

4. [GREEN] Add `_byCategory` reverse index
   - File: `InMemoryBeliefStore.cs` lines 96-98
   - Add `ConcurrentDictionary<string, ConcurrentBag<AgentBelief>> _byCategory`
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Both performance tests fail before optimization
- [ ] Both tests pass after adding indices
- [ ] Existing tests still pass

**Dependencies:** None
**Parallelizable:** Yes (Group A)

---

### Task A.3: Early Exit When No Exclusions

**File:** `src/Agentic.Workflow.Infrastructure/Selection/ThompsonSamplingAgentSelector.cs`
**Test File:** `src/Agentic.Workflow.Infrastructure.Tests/Selection/ThompsonSamplingSelectorTests.cs`

**TDD Steps:**

1. [RED] Write test: `SelectAgentAsync_NoExclusions_SkipsExceptAllocation`
   - File: `ThompsonSamplingSelectorTests.cs`
   - Expected failure: Memory profiling shows `.Except()` allocation even when exclusions empty
   - Run: `dotnet test --filter "SelectAgentAsync_NoExclusions_SkipsExceptAllocation"` - MUST FAIL

2. [GREEN] Add conditional check before `.Except()`
   - File: `ThompsonSamplingAgentSelector.cs` line 65-67
   - Change to: `context.ExcludedAgents is { Count: > 0 } ? ... : availableAgents.ToList()`
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Test demonstrates allocation difference
- [ ] Test passes with conditional
- [ ] Existing exclusion tests still pass

**Dependencies:** None
**Parallelizable:** Yes (Group A)

---

## Group B: Loop Detection Optimizations

### Task A.4: Skip Semantic Similarity on High-Confidence Signals

**File:** `src/Agentic.Workflow.Infrastructure/LoopDetection/LoopDetector.cs`
**Test File:** `src/Agentic.Workflow.Infrastructure.Tests/LoopDetection/LoopDetectorTests.cs`

**TDD Steps:**

1. [RED] Write test: `DetectLoopAsync_ExactRepetition_SkipsSemanticSimilarity`
   - File: Create `LoopDetectorTests.cs` if not exists
   - Mock `_similarityCalculator` to throw if called
   - Provide entries with exact repetition (score = 1.0)
   - Expected failure: Semantic similarity called despite high-confidence repetition
   - Run: `dotnet test --filter "DetectLoopAsync_ExactRepetition_SkipsSemanticSimilarity"` - MUST FAIL

2. [GREEN] Add early return before semantic similarity
   - File: `LoopDetector.cs` lines 74-96
   - Check `repetitionScore >= 1.0 - double.Epsilon` and return early
   - Run: `dotnet test` - MUST PASS

3. [RED] Write test: `DetectLoopAsync_PerfectNoProgress_SkipsSemanticSimilarity`
   - Same pattern for no-progress score
   - Run: `dotnet test --filter "DetectLoopAsync_PerfectNoProgress_SkipsSemanticSimilarity"` - MUST FAIL

4. [GREEN] Add early return for no-progress case
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Tests verify similarity calculator not invoked on high-confidence
- [ ] Lower confidence cases still call similarity
- [ ] Existing detection tests pass

**Dependencies:** None
**Parallelizable:** Yes (Group B)

---

### Task A.5: Fix String Comparison in Oscillation Detection

**File:** `src/Agentic.Workflow.Infrastructure/LoopDetection/LoopDetector.cs`
**Test File:** `src/Agentic.Workflow.Infrastructure.Tests/LoopDetection/LoopDetectorTests.cs`

**TDD Steps:**

1. [RED] Write test: `CalculateOscillationScore_NonInternedStrings_DetectsPattern`
   - File: `LoopDetectorTests.cs`
   - Create action strings dynamically (not interned): `new string("action".ToCharArray())`
   - Expected failure: Reference equality fails for equivalent strings
   - Run: `dotnet test --filter "CalculateOscillationScore_NonInternedStrings_DetectsPattern"` - MUST FAIL

2. [GREEN] Change `==` to `string.Equals(..., StringComparison.Ordinal)`
   - File: `LoopDetector.cs` line 271
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Test fails with reference equality
- [ ] Test passes with ordinal comparison
- [ ] Correctness fix, not just optimization

**Dependencies:** None
**Parallelizable:** Yes (Group B)

---

### Task A.6: Avoid Intermediate List in Repetition Scoring

**File:** `src/Agentic.Workflow.Infrastructure/LoopDetection/LoopDetector.cs`
**Test File:** `src/Agentic.Workflow.Infrastructure.Tests/LoopDetection/LoopDetectorTests.cs`

**TDD Steps:**

1. [RED] Write test: `CalculateRepetitionScore_LargeEntrySet_NoIntermediateListAllocation`
   - File: `LoopDetectorTests.cs`
   - Use memory diagnostics or allocation counter
   - Expected failure: `.ToList()` creates intermediate allocation
   - Run: `dotnet test --filter "CalculateRepetitionScore_LargeEntrySet_NoIntermediateListAllocation"` - MUST FAIL

2. [GREEN] Change to direct LINQ `.Max(g => g.Count())`
   - File: `LoopDetector.cs` lines 196-202
   - Remove `.ToList()` before `.Max()`
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Allocation test shows reduction
- [ ] Result correctness unchanged
- [ ] Existing tests pass

**Dependencies:** None
**Parallelizable:** Yes (Group B)

---

## Group C: Budget & Ledger Optimizations

### Task A.7: Cache OverallScarcity in WorkflowBudget

**File:** `src/Agentic.Workflow.Infrastructure/Budget/WorkflowBudget.cs`
**Test File:** `src/Agentic.Workflow.Infrastructure.Tests/Budget/WorkflowBudgetTests.cs`

**TDD Steps:**

1. [RED] Write test: `OverallScarcity_AccessedMultipleTimes_ComputesOnce`
   - File: Create `WorkflowBudgetTests.cs` if not exists
   - Access `OverallScarcity` property multiple times
   - Use mock/spy to verify computation count
   - Expected failure: Computed on every access
   - Run: `dotnet test --filter "OverallScarcity_AccessedMultipleTimes_ComputesOnce"` - MUST FAIL

2. [GREEN] Add `Lazy<ScarcityLevel>` caching
   - File: `WorkflowBudget.cs` lines 46-60
   - Add private `_cachedScarcity` field initialized in constructor
   - Property returns `_cachedScarcity.Value`
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Test verifies single computation
- [ ] Scarcity value still correct
- [ ] Immutability preserved (record type)

**Dependencies:** None
**Parallelizable:** Yes (Group C)

---

### Task A.8: Replace Append().ToList() in ProgressLedger

**File:** `src/Agentic.Workflow.Infrastructure/Ledgers/ProgressLedger.cs`
**Test File:** `src/Agentic.Workflow.Infrastructure.Tests/Ledgers/ProgressLedgerTests.cs`

**TDD Steps:**

1. [RED] Write test: `WithEntry_LargeEntryCount_PreallocatesCapacity`
   - File: Create `ProgressLedgerTests.cs` if not exists
   - Create ledger with 1000 entries, add one more
   - Profile allocation size
   - Expected failure: Full re-enumeration allocates more than needed
   - Run: `dotnet test --filter "WithEntry_LargeEntryCount_PreallocatesCapacity"` - MUST FAIL

2. [GREEN] Use pre-allocated list
   - File: `ProgressLedger.cs` lines 69-77
   - Change to: `new List<ProgressEntry>(Entries.Count + 1)` + `AddRange` + `Add`
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Allocation test shows improvement
- [ ] Entries correctly appended
- [ ] Immutability preserved

**Dependencies:** None
**Parallelizable:** Yes (Group C)

---

### Task A.9: Use ValueTask for Sync Paths in StepExecutionLedger

**File:** `src/Agentic.Workflow.Infrastructure/ExecutionLedgers/InMemoryStepExecutionLedger.cs`
**Test File:** `src/Agentic.Workflow.Infrastructure.Tests/ExecutionLedgers/InMemoryStepExecutionLedgerTests.cs`

**TDD Steps:**

1. [RED] Write test: `TryGetCachedResultAsync_CacheHit_ReturnsValueTaskWithoutAllocation`
   - File: `InMemoryStepExecutionLedgerTests.cs`
   - Verify `ValueTask` returned for sync path (cache hit)
   - Expected failure: Current `Task.FromResult()` allocates
   - Run: `dotnet test --filter "TryGetCachedResultAsync_CacheHit_ReturnsValueTaskWithoutAllocation"` - MUST FAIL

2. [GREEN] Change return type to `ValueTask<T>`
   - File: `InMemoryStepExecutionLedger.cs` lines 44-101
   - Return `new ValueTask<T?>(result)` for sync paths
   - Return `default` for not-found
   - Run: `dotnet test` - MUST PASS

3. [REFACTOR] Update interface if needed
   - May need to update `IStepExecutionLedger` interface
   - Run: `dotnet test` - MUST STAY GREEN

**Verification:**
- [ ] ValueTask returned without Task allocation
- [ ] Interface consumers updated
- [ ] Async paths still work

**Dependencies:** None
**Parallelizable:** Yes (Group C)

---

## Group D: Source Generator Optimizations

### Task A.10: Cache Compilation Metadata Lookups

**File:** `src/Agentic.Workflow.Generators/StateReducerIncrementalGenerator.cs`
**Test File:** `src/Agentic.Workflow.Generators.Tests/StateReducerGeneratorIntegrationTests.cs`

**TDD Steps:**

1. [RED] Write test: `GenerateReducer_MultipleProperties_LooksUpMetadataOnce`
   - File: `StateReducerGeneratorIntegrationTests.cs`
   - Create state with 10+ properties using collections
   - Instrument/spy `GetTypeByMetadataName` call count
   - Expected failure: Called once per property
   - Run: `dotnet test --filter "GenerateReducer_MultipleProperties_LooksUpMetadataOnce"` - MUST FAIL

2. [GREEN] Create `WellKnownTypes` cache class
   - File: `StateReducerIncrementalGenerator.cs` lines 185, 209-210
   - Add nested `WellKnownTypes` class with cached symbols
   - Initialize once per compilation context
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Metadata looked up once per compilation
- [ ] Generated code identical
- [ ] Generator performance improved

**Dependencies:** None
**Parallelizable:** Yes (Group D)

---

### Task A.11: Use HashSet for Contains Checks

**File:** `src/Agentic.Workflow.Generators/WorkflowIncrementalGenerator.cs`
**Test File:** `src/Agentic.Workflow.Generators.Tests/WorkflowModelFactoryTests.cs`

**TDD Steps:**

1. [RED] Write test: `BuildModel_ManySteps_UsesConstantTimeContainsCheck`
   - File: `WorkflowModelFactoryTests.cs` or new test file
   - Create workflow with 50+ steps
   - Verify O(1) lookup time pattern
   - Expected failure: O(n) List.Contains calls
   - Run: `dotnet test --filter "BuildModel_ManySteps_UsesConstantTimeContainsCheck"` - MUST FAIL

2. [GREEN] Convert to HashSet before lookups
   - File: `WorkflowIncrementalGenerator.cs` lines 203, 214, 238, 246
   - Add `var existingStepNames = new HashSet<string>(allStepNames, StringComparer.Ordinal)`
   - Use HashSet for `.Contains()` checks
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Contains checks use HashSet
- [ ] Generated output unchanged
- [ ] Large workflow generation faster

**Dependencies:** None
**Parallelizable:** Yes (Group D)

---

### Task A.12: Pre-allocate List Capacities

**File:** `src/Agentic.Workflow.Generators/WorkflowIncrementalGenerator.cs`
**Test File:** `src/Agentic.Workflow.Generators.Tests/WorkflowModelFactoryTests.cs`

**TDD Steps:**

1. [RED] Write test: `BuildModel_WithFailureHandlersAndForks_PreallocatesLists`
   - File: `WorkflowModelFactoryTests.cs`
   - Create workflow with steps, failure handlers, and forks
   - Check allocation patterns
   - Expected failure: Multiple list resizes during merge
   - Run: `dotnet test --filter "BuildModel_WithFailureHandlersAndForks_PreallocatesLists"` - MUST FAIL

2. [GREEN] Pre-allocate with estimated capacity
   - File: `WorkflowIncrementalGenerator.cs` lines 195-197
   - Calculate: `stepNames.Count + failureHandlers?.Count ?? 0 + forks?.Count ?? 0`
   - Use capacity in `new List<T>(estimatedSize)`
   - Run: `dotnet test` - MUST PASS

**Verification:**
- [ ] Lists pre-allocated to avoid resizing
- [ ] Merge operations produce same result
- [ ] Generator memory usage reduced

**Dependencies:** None
**Parallelizable:** Yes (Group D)

---

## Completion Checklist

### Phase A Completion
- [ ] All 12 optimizations implemented with TDD
- [ ] All existing tests pass
- [ ] No regressions in functionality
- [ ] Ready for integration

### Pre-Phase B Requirements
- [ ] Create BenchmarkDotNet project (C.5 moved up)
- [ ] Establish baseline measurements
- [ ] Document measured improvements from Phase A
- [ ] Decide Phase B priority based on data

---

## Test Commands

```bash
# Run all tests
dotnet test

# Run specific group
dotnet test --filter "Category=Unit&FullyQualifiedName~ThompsonSampling"
dotnet test --filter "Category=Unit&FullyQualifiedName~LoopDetector"
dotnet test --filter "Category=Unit&FullyQualifiedName~Budget"
dotnet test --filter "Category=Unit&FullyQualifiedName~Generator"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## Notes

1. **Performance tests:** Some tests verify performance characteristics (timing, allocations). These may be flaky in CI; consider marking as `[Property("Category", "Performance")]` for selective execution.

2. **Interface changes:** Task A.9 (ValueTask) may require interface updates. Coordinate with consumers.

3. **Generator testing:** Generator tests use compilation-based testing. Ensure test compilation references are stable.

4. **Measurement:** After Phase A, run benchmarks before planning Phase B to validate optimization impact.
