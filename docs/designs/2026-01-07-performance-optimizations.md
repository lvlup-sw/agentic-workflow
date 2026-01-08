# Performance Optimizations - Phased Implementation

## Summary

Systematic performance enhancements for the Agentic.Workflow library targeting latency, throughput, and memory efficiency across all critical subsystems.

**Approach:** Phased implementation (A → B → C) with measurement between phases to validate impact and guide priorities.

## Motivation

Performance profiling identified **47 optimization opportunities** across 6 subsystems:

| Subsystem | Findings | Highest Severity |
|-----------|----------|------------------|
| Thompson Sampling | 10 | Sequential I/O in hot path |
| Loop Detection | 9 | O(n²) pattern detection, unconditional LLM calls |
| Vector Search (RAG) | 10 | Linear scan on every search |
| Budget Guards | 8 | Repeated scarcity calculations |
| Source Generators | 7 | Repeated syntax tree traversals |
| Workflow Execution | 10 | List copy on every ledger append |

Key issues:
- **Latency:** Sequential belief fetching, unconditional semantic similarity calls
- **Memory:** New list allocations on every ledger update, no collection pooling
- **Algorithmic:** O(n²) oscillation detection, O(n) belief lookups

---

## Phase A: Surgical Hot-Path Optimizations

**Goal:** Quick wins targeting verified hot paths with minimal risk.

### A.1 Thompson Sampling - Parallelize Belief Fetching

**File:** `src/Agentic.Workflow.Infrastructure/Selection/ThompsonSamplingAgentSelector.cs`
**Lines:** 82-99

**Current:** Sequential loop fetching beliefs for each candidate agent.
```csharp
foreach (var agentId in candidates)
{
    var beliefResult = await _beliefStore.GetBeliefAsync(agentId, categoryName, cancellationToken);
    // process belief...
}
```

**Change:** Parallel fetch with `Task.WhenAll()`.
```csharp
var beliefTasks = candidates.Select(agentId =>
    _beliefStore.GetBeliefAsync(agentId, categoryName, cancellationToken));
var beliefs = await Task.WhenAll(beliefTasks);
// process all beliefs...
```

**Impact:** Latency reduced from O(n × belief_fetch_time) to O(belief_fetch_time) for n candidates.

---

### A.2 Thompson Sampling - Add Secondary Indices to Belief Store

**File:** `src/Agentic.Workflow.Infrastructure/Selection/InMemoryBeliefStore.cs`
**Lines:** 82-84, 96-98

**Current:** O(n) filter on all beliefs for `GetBeliefsForAgentAsync` and `GetBeliefsForCategoryAsync`.
```csharp
return _beliefs.Values.Where(b => b.AgentId == agentId).ToList();
```

**Change:** Add reverse indices.
```csharp
private readonly ConcurrentDictionary<string, ConcurrentBag<AgentBelief>> _byAgent = new();
private readonly ConcurrentDictionary<string, ConcurrentBag<AgentBelief>> _byCategory = new();
```

**Impact:** O(1) lookup instead of O(n) scan.

---

### A.3 Thompson Sampling - Early Exit When No Exclusions

**File:** `src/Agentic.Workflow.Infrastructure/Selection/ThompsonSamplingAgentSelector.cs`
**Lines:** 65-67

**Current:** Always materializes list even when `ExcludedAgents` is null/empty.
```csharp
var candidates = availableAgents.Except(context.ExcludedAgents ?? []).ToList();
```

**Change:** Conditional materialization.
```csharp
var candidates = context.ExcludedAgents is { Count: > 0 }
    ? availableAgents.Except(context.ExcludedAgents).ToList()
    : availableAgents.ToList();
```

**Impact:** Avoid unnecessary allocation in common case.

---

### A.4 Loop Detection - Skip Semantic Similarity on High-Confidence Signals

**File:** `src/Agentic.Workflow.Infrastructure/LoopDetection/LoopDetector.cs`
**Lines:** 74-96

**Current:** Semantic similarity calculated unconditionally, even when exact repetition detected.

**Change:** Add early return before expensive async call.
```csharp
var repetitionScore = CalculateRepetitionScore(recentEntries);
if (repetitionScore >= 1.0 - double.Epsilon)
{
    return LoopDetectionResult.LoopDetected(
        repetitionScore, LoopRecoveryStrategy.Backtrack);
}

var noProgressScore = CalculateNoProgressScore(recentEntries);
if (noProgressScore >= 1.0 - double.Epsilon)
{
    return LoopDetectionResult.LoopDetected(
        noProgressScore, LoopRecoveryStrategy.Replan);
}

// Only compute semantic similarity if no high-confidence signal
var outputs = recentEntries.Select(e => e.Output).ToList();
var semanticScore = await _similarityCalculator.CalculateMaxSimilarityAsync(...);
```

**Impact:** Avoid LLM embedding calls when not needed.

---

### A.5 Loop Detection - Fix String Comparison

**File:** `src/Agentic.Workflow.Infrastructure/LoopDetection/LoopDetector.cs`
**Line:** 271

**Current:** Reference equality comparison.
```csharp
if (actions[i] == actions[i % period])
```

**Change:** Ordinal comparison.
```csharp
if (string.Equals(actions[i], actions[i % period], StringComparison.Ordinal))
```

**Impact:** Correctness fix - ensures pattern detection works with non-interned strings.

---

### A.6 Loop Detection - Avoid Intermediate List in Repetition Scoring

**File:** `src/Agentic.Workflow.Infrastructure/LoopDetection/LoopDetector.cs`
**Lines:** 196-202

**Current:** Creates intermediate list for max count.
```csharp
var actionGroups = entries.GroupBy(e => e.Action).Select(g => g.Count()).ToList();
var maxCount = actionGroups.Max();
```

**Change:** Direct LINQ.
```csharp
var maxCount = entries.GroupBy(e => e.Action).Max(g => g.Count());
```

**Impact:** Avoid one list allocation per detection call.

---

### A.7 Budget Guards - Cache OverallScarcity

**File:** `src/Agentic.Workflow.Infrastructure/Budget/WorkflowBudget.cs`
**Lines:** 46-60

**Current:** Computed property iterates all resources on every access.

**Change:** Compute once and cache in record.
```csharp
public sealed record WorkflowBudget : IWorkflowBudget
{
    private readonly Lazy<ScarcityLevel> _cachedScarcity;

    public ScarcityLevel OverallScarcity => _cachedScarcity.Value;

    public WorkflowBudget(...)
    {
        _cachedScarcity = new Lazy<ScarcityLevel>(() =>
            Resources.Values.Select(r => r.Scarcity).Max());
    }
}
```

**Impact:** O(1) scarcity checks after first access.

---

### A.8 Workflow Execution - Replace Append().ToList()

**File:** `src/Agentic.Workflow.Infrastructure/Ledgers/ProgressLedger.cs`
**Lines:** 69-77

**Current:** Creates new list on every append.
```csharp
return this with { Entries = Entries.Append(entry).ToList() };
```

**Change:** Use list with pre-allocation.
```csharp
public IProgressLedger WithEntry(ProgressEntry entry)
{
    var newEntries = new List<ProgressEntry>(Entries.Count + 1);
    newEntries.AddRange(Entries);
    newEntries.Add(entry);
    return this with { Entries = newEntries };
}
```

**Impact:** Avoid re-enumeration and right-size allocation.

---

### A.9 Workflow Execution - Use ValueTask for Sync Paths

**File:** `src/Agentic.Workflow.Infrastructure/ExecutionLedgers/InMemoryStepExecutionLedger.cs`
**Lines:** 44-101

**Current:** Returns `Task.FromResult()` for synchronous dictionary lookups.

**Change:** Use `ValueTask<T>` to avoid Task allocation.
```csharp
public ValueTask<TResult?> TryGetCachedResultAsync<TResult>(...)
{
    if (!_cache.TryGetValue(key, out var entry))
        return default; // No allocation

    var result = JsonSerializer.Deserialize<TResult>(entry.Json);
    return new ValueTask<TResult?>(result);
}
```

**Impact:** Zero-allocation cache hits.

---

### A.10 Source Generators - Cache Compilation Metadata Lookups

**File:** `src/Agentic.Workflow.Generators/StateReducerIncrementalGenerator.cs`
**Lines:** 185, 209-210

**Current:** Repeated `GetTypeByMetadataName()` calls per property.

**Change:** Cache in transform context.
```csharp
private sealed class WellKnownTypes
{
    public INamedTypeSymbol? EnumerableT { get; }
    public INamedTypeSymbol? ReadOnlyDictT { get; }
    public INamedTypeSymbol? DictT { get; }

    public static WellKnownTypes FromCompilation(Compilation compilation) => new()
    {
        EnumerableT = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1"),
        ReadOnlyDictT = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyDictionary`2"),
        DictT = compilation.GetTypeByMetadataName("System.Collections.Generic.IDictionary`2"),
    };
}
```

**Impact:** Single lookup per compilation context instead of per-property.

---

### A.11 Source Generators - Use HashSet for Contains Checks

**File:** `src/Agentic.Workflow.Generators/WorkflowIncrementalGenerator.cs`
**Lines:** 203, 214, 238, 246

**Current:** O(n) `.Contains()` on List.
```csharp
if (!allStepNames.Contains(handlerStep))
```

**Change:** Use HashSet for lookups.
```csharp
var existingStepNames = new HashSet<string>(allStepNames, StringComparer.Ordinal);
if (!existingStepNames.Contains(handlerStep))
```

**Impact:** O(1) vs O(n) for each contains check.

---

### A.12 Source Generators - Pre-allocate List Capacities

**File:** `src/Agentic.Workflow.Generators/WorkflowIncrementalGenerator.cs`
**Lines:** 195-197

**Current:** Default capacity lists.
```csharp
var allStepNames = new List<string>(stepNames);
var allStepModels = new List<StepModel>(stepModels);
```

**Change:** Pre-allocate with expected growth.
```csharp
var estimatedSize = stepNames.Count + failureHandlers?.Count ?? 0 + forks?.Count ?? 0;
var allStepNames = new List<string>(estimatedSize);
allStepNames.AddRange(stepNames);
```

**Impact:** Avoid list resizing during merge operations.

---

## Phase B: Algorithmic & Memory Overhaul

**Goal:** Address fundamental O(n²) complexity and allocation patterns.

### B.1 Vector Search - Partial Sort with PriorityQueue

**File:** `src/Agentic.Workflow.Rag/Adapters/InMemoryVectorSearchAdapterGeneric.cs`
**Lines:** 49-52

**Current:** Full sort then take top-K: O(n log n).

**Change:** Use `PriorityQueue<T>` for O(n + k log k).
```csharp
var heap = new PriorityQueue<VectorSearchResult, double>();
foreach (var doc in documents)
{
    if (doc.Score >= minRelevance)
    {
        heap.Enqueue(doc, -doc.Score); // Min-heap with negated scores
        if (heap.Count > topK)
            heap.Dequeue();
    }
}
var results = new VectorSearchResult[heap.Count];
for (int i = results.Length - 1; i >= 0; i--)
    results[i] = heap.Dequeue();
```

**Impact:** O(n + k log k) vs O(n log n) - significant for large document sets.

---

### B.2 Vector Search - Implement Filter Indexing

**File:** `src/Agentic.Workflow.Rag/Adapters/InMemoryVectorSearchAdapterGeneric.cs`
**Line:** 46

**Current:** `filters` parameter ignored.

**Change:** Build inverted index on common filter fields.
```csharp
private readonly ConcurrentDictionary<string, HashSet<int>> _filterIndex = new();

public void AddDocument(string content, string? id, IReadOnlyDictionary<string, object?>? metadata)
{
    var docIndex = documents.Count;
    // ... add document ...

    if (metadata != null)
    {
        foreach (var (key, value) in metadata)
        {
            var indexKey = $"{key}:{value}";
            _filterIndex.GetOrAdd(indexKey, _ => new HashSet<int>()).Add(docIndex);
        }
    }
}

public Task<IReadOnlyList<VectorSearchResult>> SearchAsync(..., IReadOnlyDictionary<string, object>? filters, ...)
{
    IEnumerable<int> candidates = filters != null
        ? GetFilteredIndices(filters)
        : Enumerable.Range(0, documents.Count);

    // Search only filtered candidates
}
```

**Impact:** Pre-filter search space before scoring.

---

### B.3 Vector Search - Add Batch Search API

**File:** `src/Agentic.Workflow.Rag/Abstractions/IVectorSearchAdapter.cs`

**Change:** Add batch interface.
```csharp
Task<IReadOnlyList<IReadOnlyList<VectorSearchResult>>> BatchSearchAsync(
    IReadOnlyList<string> queries,
    int topK = 5,
    double minRelevance = 0.7,
    IReadOnlyDictionary<string, object>? filters = null,
    CancellationToken cancellationToken = default);
```

**Impact:** Enable vectorized processing across multiple queries.

---

### B.4 Loop Detection - ArrayPool for Temporary Collections

**File:** `src/Agentic.Workflow.Infrastructure/LoopDetection/LoopDetector.cs`
**Line:** 237

**Current:** Allocates new array.
```csharp
var actions = entries.Select(e => e.Action).ToArray();
```

**Change:** Rent from pool.
```csharp
var actions = ArrayPool<string>.Shared.Rent(entries.Count);
try
{
    for (int i = 0; i < entries.Count; i++)
        actions[i] = entries[i].Action;

    // ... use actions ...
}
finally
{
    ArrayPool<string>.Shared.Return(actions);
}
```

**Impact:** Zero heap allocations for temporary arrays.

---

### B.5 Loop Detection - Rolling Hash for O(n) Pattern Detection

**File:** `src/Agentic.Workflow.Infrastructure/LoopDetection/LoopDetector.cs`
**Lines:** 230-248

**Current:** O(n²) nested loop for oscillation detection.

**Change:** Use rolling hash with Rabin-Karp style pattern matching.
```csharp
private static double CalculateOscillationScore(IReadOnlyList<ProgressEntry> entries)
{
    if (entries.Count < 4) return 0;

    var actions = entries.Select(e => e.Action.GetHashCode()).ToArray();
    var maxScore = 0.0;

    // Use rolling hash for period detection
    for (var period = 2; period <= entries.Count / 2; period++)
    {
        var matches = 0;
        for (var i = period; i < entries.Count; i++)
        {
            if (actions[i] == actions[i - period])
                matches++;
        }
        var score = (double)matches / (entries.Count - period);
        maxScore = Math.Max(maxScore, score);
    }

    return maxScore;
}
```

**Impact:** Still O(n²) but with cheaper hash comparison instead of string comparison.

---

### B.6 Task Ledger - Incremental Hash Computation

**File:** `src/Agentic.Workflow.Infrastructure/Ledgers/TaskLedger.cs`
**Lines:** 138-152

**Current:** Full JSON serialization and SHA256 on every update.

**Change:** Incremental hash update.
```csharp
public sealed record TaskLedger : ITaskLedger
{
    private readonly IncrementalHash _hashState;

    public ITaskLedger WithTask(TaskEntry task)
    {
        var newHash = _hashState.Clone();
        newHash.AppendData(Encoding.UTF8.GetBytes(task.TaskId));
        newHash.AppendData(Encoding.UTF8.GetBytes(task.Description ?? ""));

        return this with
        {
            Tasks = Tasks.Append(task).ToList(),
            _hashState = newHash,
            ContentHash = Convert.ToHexString(newHash.GetCurrentHash()).ToLowerInvariant()
        };
    }
}
```

**Impact:** O(1) hash update instead of O(n) re-serialization.

---

### B.7 Condition Registry - Remove Boxing

**File:** `src/Agentic.Workflow/Services/WorkflowConditionRegistry.cs`
**Lines:** 65, 96

**Current:** Stores `Func<TState, bool>` as `object`, requiring cast on every evaluation.

**Change:** Use generic wrapper with interface.
```csharp
private interface IConditionWrapper
{
    bool Evaluate(object state);
}

private sealed class ConditionWrapper<TState> : IConditionWrapper
    where TState : class
{
    private readonly Func<TState, bool> _condition;

    public bool Evaluate(object state) => _condition((TState)state);
}

private static readonly ConcurrentDictionary<string, IConditionWrapper> Conditions = new();
```

**Impact:** Single virtual call instead of boxing + cast.

---

## Phase C: Full Performance Architecture

**Goal:** Production-grade performance infrastructure with observability.

### C.1 Immutable Collection Types for Ledgers

Replace `List<T>` with `ImmutableList<T>` for structural sharing.

**Files affected:**
- `ProgressLedger.cs`
- `TaskLedger.cs`

**Benefit:** Copy-on-write with O(log n) append instead of O(n) full copy.

---

### C.2 Lock-Free Belief Store

Replace `ConcurrentDictionary` locks with `Interlocked` operations for belief updates.

**File:** `InMemoryBeliefStore.cs`

**Benefit:** Reduced contention in high-parallelism scenarios.

---

### C.3 Batch Embedding Pipeline

Pool embedding requests and batch to LLM provider.

**New component:** `BatchEmbeddingService`

**Benefit:** Amortize network overhead across multiple similarity checks.

---

### C.4 Query Result Caching in Vector Search

Add LRU cache with TTL for repeated queries.

**File:** `InMemoryVectorSearchAdapter.cs`

**Benefit:** Avoid re-scoring for identical queries within cache window.

---

### C.5 BenchmarkDotNet Suite

Create benchmark project for baseline and regression tracking.

**New project:** `Agentic.Workflow.Benchmarks`

**Benchmarks:**
- `ThompsonSamplingBenchmarks` - Selection latency vs candidate count
- `LoopDetectionBenchmarks` - Detection latency vs window size
- `VectorSearchBenchmarks` - Search latency vs document count
- `LedgerBenchmarks` - Append latency vs entry count

---

### C.6 Metrics Instrumentation

Add OpenTelemetry metrics for production observability.

**Metrics:**
- `agentic.workflow.agent_selection.duration_ms` (histogram)
- `agentic.workflow.loop_detection.duration_ms` (histogram)
- `agentic.workflow.step_cache.hit_ratio` (gauge)
- `agentic.workflow.budget.scarcity_level` (gauge)

---

## Success Criteria

| Metric | Baseline | Phase A Target | Phase B Target | Phase C Target |
|--------|----------|----------------|----------------|----------------|
| Agent selection latency (10 candidates) | Sequential | 50% reduction | 60% reduction | 70% reduction |
| Loop detection latency (window=20) | ~200 ops | ~180 ops | ~100 ops | ~80 ops |
| Vector search (1000 docs, k=5) | O(n log n) | Same | O(n + k log k) | + caching |
| Memory per ledger append | Full copy | Pre-alloc | Pool | ImmutableList |

---

## Implementation Order

### Phase A (Immediate)
1. A.4 - Skip semantic similarity (highest latency impact)
2. A.1 - Parallelize belief fetching
3. A.8 - Replace Append().ToList()
4. A.2 - Add secondary indices
5. A.7 - Cache OverallScarcity
6. A.9 - ValueTask for sync paths
7. A.3, A.5, A.6 - Quick fixes
8. A.10, A.11, A.12 - Generator optimizations

### Phase B (After measuring Phase A)
1. B.1 - Partial sort (vector search)
2. B.5 - Rolling hash (loop detection)
3. B.4 - ArrayPool (loop detection)
4. B.6 - Incremental hash (task ledger)
5. B.2, B.3 - Filter indexing + batch API
6. B.7 - Remove boxing

### Phase C (After measuring Phase B)
1. C.5 - BenchmarkDotNet suite (first, to establish baselines)
2. C.6 - Metrics instrumentation
3. C.1, C.2 - Immutable collections + lock-free store
4. C.3, C.4 - Batch embedding + query caching

---

## Test Plan

Each phase includes:
1. Unit tests for modified components
2. Benchmark comparison (before/after)
3. Memory profiling with dotMemory or similar
4. Integration tests to verify correctness

---

## Dependencies

- Phase B depends on Phase A baseline measurements
- Phase C benchmarks depend on BenchmarkDotNet setup
- Metrics instrumentation depends on OpenTelemetry packages
