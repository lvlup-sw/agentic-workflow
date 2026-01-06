# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-05

### Initial Public Release

First stable release of the Agentic.Workflow library for building production-grade agentic workflows.

### Packages

- **Agentic.Workflow** - Core DSL, abstractions, and Thompson Sampling types
- **Agentic.Workflow.Generators** - Roslyn source generators for saga/event generation
- **Agentic.Workflow.Infrastructure** - Infrastructure implementations (belief stores, selectors)
- **Agentic.Workflow.Agents** - Agent-specific integrations (MAF, Semantic Kernel)
- **Agentic.Workflow.Rag** - RAG integration with vector search adapters

### Features

#### Fluent DSL
- `Workflow<TState>.Create()` entry point
- `StartWith<T>()`, `Then<T>()`, `Finally<T>()` for linear flow
- `Branch()` for conditional routing with pattern matching
- `Fork()` / `Join<T>()` for parallel execution
- `RepeatUntil()` for iterative loops with exit conditions
- `AwaitApproval<T>()` for human-in-the-loop workflows
- `Compensate<T>()` for rollback handlers
- `OnFailure()` for error handling

#### Source Generators
- Phase enumeration generation
- Wolverine saga class generation with handlers
- Command and event type generation
- State reducer generation (`[Append]`, `[Merge]` attributes)
- Transition table generation for validation
- DI extension method generation
- Mermaid diagram generation for visualization

#### Thompson Sampling Agent Selection
- `IAgentSelector` interface for agent selection
- Contextual multi-armed bandit with Beta priors
- 7 task categories: Analysis, Coding, Research, Writing, Data, Integration, General
- `ITaskFeatureExtractor` for category classification
- `IBeliefStore` for persistence of agent beliefs

#### Loop Detection
- Exact repetition detection in sliding window
- Semantic repetition via cosine similarity
- Oscillation pattern detection (A-B-A-B)
- No-progress detection

#### Budget Guard
- Step count limits
- Token usage tracking
- Wall time enforcement
- Scarcity-based action scoring

#### Compiler Diagnostics
- AGWF001: Empty workflow name
- AGWF002: No steps found
- AGWF003: Duplicate step name
- AGWF004: Invalid namespace
- AGWF009: Missing StartWith
- AGWF010: Missing Finally
- AGWF012: Fork without Join
- AGWF014: Loop without body
- AGSR001: Invalid reducer attribute usage
- AGSR002: No reducers found

### Infrastructure

- Wolverine saga integration for durable state
- Marten event sourcing for audit trails
- PostgreSQL persistence
- Transactional outbox pattern
- Time-travel debugging via event replay

[1.0.0]: https://github.com/lvlup-sw/agentic-workflow/releases/tag/v1.0.0
