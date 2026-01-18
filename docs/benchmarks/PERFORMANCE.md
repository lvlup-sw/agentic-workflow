# Performance Targets and Baselines

This document defines performance targets for Agentic.Workflow subsystems, documents current baseline measurements, and specifies optimization trigger conditions.

## Performance Targets

| Benchmark | Baseline Target | Optimized Target | Measurement | Status |
|-----------|-----------------|------------------|-------------|--------|
| Agent selection (25 candidates) | < 5ms p95 | < 2ms p95 | Latency | Pending |
| Loop detection (window=20) | < 1ms p95 | < 0.5ms p95 | Latency | Pending |
| Ledger append (1000 entries) | < 100us | < 50us | Latency | Pending |
| Cache hit | < 1us | < 0.5us | Latency | Pending |
| Vector search (1000 docs, k=5) | < 10ms | < 5ms | Latency | Pending |
| Concurrent workflows (100) | > 500/sec | > 1000/sec | Throughput | Pending |
| Cache hit allocation | 0 B | 0 B | Memory | Pending |
| Ledger append allocation | < 1KB | < 200B | Memory | Pending |

## Current Baselines

> **Note:** Baseline measurements will be populated after Phase 1 benchmark runs.

### Agent Selection (ThompsonSampling)

| Candidates | Mean | P95 | Gen0 | Allocated | Date |
|------------|------|-----|------|-----------|------|
| 5 | TBD | TBD | TBD | TBD | - |
| 25 | TBD | TBD | TBD | TBD | - |
| 100 | TBD | TBD | TBD | TBD | - |

### Loop Detection

| Window Size | Mean | P95 | Gen0 | Allocated | Date |
|-------------|------|-----|------|-----------|------|
| 10 | TBD | TBD | TBD | TBD | - |
| 20 | TBD | TBD | TBD | TBD | - |
| 50 | TBD | TBD | TBD | TBD | - |

### Ledger Operations

| Operation | Entries | Mean | P95 | Gen0 | Allocated | Date |
|-----------|---------|------|-----|------|-----------|------|
| Append | 100 | TBD | TBD | TBD | TBD | - |
| Append | 1000 | TBD | TBD | TBD | TBD | - |
| Lookup | 1000 | TBD | TBD | TBD | TBD | - |

### Cache Operations

| Operation | Size | Mean | P95 | Gen0 | Allocated | Date |
|-----------|------|------|-----|------|-----------|------|
| Hit | 100 | TBD | TBD | TBD | TBD | - |
| Hit | 1000 | TBD | TBD | TBD | TBD | - |
| Miss | 1000 | TBD | TBD | TBD | TBD | - |

### Vector Search

| Corpus Size | k | Mean | P95 | Gen0 | Allocated | Date |
|-------------|---|------|-----|------|-----------|------|
| 100 | 5 | TBD | TBD | TBD | TBD | - |
| 1000 | 5 | TBD | TBD | TBD | TBD | - |
| 1000 | 10 | TBD | TBD | TBD | TBD | - |

### Integration Benchmarks

| Scenario | Concurrency | Throughput | p95 Latency | Date |
|----------|-------------|------------|-------------|------|
| Simple workflow | 1 | TBD | TBD | - |
| Simple workflow | 10 | TBD | TBD | - |
| Simple workflow | 100 | TBD | TBD | - |

## Optimization Triggers

An optimization is warranted when:

### Latency Triggers

| Condition | Action |
|-----------|--------|
| p95 > 2x baseline target | Investigate hot path |
| Mean > 5ms for cache operations | Add caching layer |
| Mean increases > 20% after change | Regression - revert or fix |

### Memory Triggers

| Condition | Action |
|-----------|--------|
| Gen0 > 0 in hot path | Investigate allocation source |
| Allocated > 1KB per operation | Pool or reuse objects |
| Gen1/Gen2 > 0 | Critical - investigate lifetime |

### Throughput Triggers

| Condition | Action |
|-----------|--------|
| < 500 workflows/sec at 100 concurrency | Profile contention |
| Linear scaling fails before 50 workers | Identify bottleneck |

## Package Integration Decision Criteria

High-performance packages should be adopted when measurements justify:

### BitFaster.Caching

| Integration Point | Trigger Condition | Current | Replace With |
|-------------------|-------------------|---------|--------------|
| StepExecutionLedger | Cache miss > 10% AND memory pressure | ConcurrentDictionary | ConcurrentLru |
| BeliefStore | Belief count > 1000 | ConcurrentDictionary | ConcurrentLfu |
| VectorSearch | Repeated query rate > 30% | None | ConcurrentLru |

### MemoryPack

| Integration Point | Trigger Condition | Current | Replace With |
|-------------------|-------------------|---------|--------------|
| Cache entries | Serialization > 5% of op time | JsonSerializer | MemoryPackSerializer |
| Artifact store | Artifact size > 10KB avg | JsonSerializer | MemoryPackSerializer |
| Ledger hashing | Hash > 1% of append time | SHA256 of JSON | MemoryPack bytes |

### CommunityToolkit.HighPerformance

| Integration Point | Trigger Condition | Current | Replace With |
|-------------------|-------------------|---------|--------------|
| LoopDetector arrays | Allocation > 1KB per detection | new string[n] | SpanOwner<string> |
| Key creation | > 100 key creations/sec | String concat | StringPool |
| Parallel loops | > 10 independent items | Manual loops | ParallelHelper |

## Measurement Methodology

### Environment

- **CI Runner:** ubuntu-latest (GitHub Actions)
- **Development:** Variable (document hardware for local runs)
- **.NET Version:** 10.0 (primary), 8.0 (LTS validation)

### BenchmarkDotNet Configuration

```csharp
AddJob(Job.Default.WithRuntime(CoreRuntime.Core10_0));
AddDiagnoser(MemoryDiagnoser.Default);
AddColumn(StatisticColumn.P95);
```

### Stability Criteria

- Run each benchmark 3 times
- Verify < 5% variance between runs
- Warmup until steady-state detected
- Results should scale predictably with parameters

## Historical Measurements

> **Note:** This section will be populated as benchmarks are run over time.

| Date | Commit | Summary |
|------|--------|---------|
| - | - | Initial baseline pending |

## References

- [Benchmark README](../../src/Agentic.Workflow.Benchmarks/README.md)
- [Design Document](../designs/2026-01-17-performance-benchmarks-and-optimizations.md)
- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
