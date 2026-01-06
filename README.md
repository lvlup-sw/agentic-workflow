# Agentic.Workflow

[![NuGet](https://img.shields.io/nuget/v/Agentic.Workflow.svg)](https://www.nuget.org/packages/Agentic.Workflow)
[![Build Status](https://img.shields.io/github/actions/workflow/status/lvlup-sw/agentic-workflow/ci.yml?branch=main)](https://github.com/lvlup-sw/agentic-workflow/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

> Deterministic orchestration for probabilistic AI agents

## The Problem

AI agents are inherently probabilistic—given the same input, an LLM may produce different outputs. Current solutions force an unsatisfying choice:

- **Agent frameworks** (LangGraph, CrewAI, AutoGen) offer great developer experience but lack production reliability: no durability, limited error recovery, poor auditability.
- **Workflow engines** (Temporal, Durable Task) provide reliability but have no awareness of agent-specific patterns: confidence handling, context management, AI-aware compensation.

## The Solution

Agentic.Workflow bridges these domains with a key insight: while agent *outputs* are probabilistic, the *workflow itself* can be deterministic if we treat each agent decision as an immutable event in an event-sourced system.

You get agent framework ergonomics with enterprise-grade reliability:

```csharp
var workflow = Workflow<OrderState>
    .Create("process-order")
    .StartWith<ValidateOrder>()
    .Then<ProcessPayment>()
    .Then<FulfillOrder>()
    .Finally<SendConfirmation>();
```

## How It Compares

| Capability | Agentic.Workflow | LangGraph | Temporal |
|------------|:----------------:|:---------:|:--------:|
| Durable by default | ✓ | | ✓ |
| Agent-native patterns | ✓ | ✓ | |
| Event-sourced audit trail | ✓ | | |
| Compile-time validation | ✓ | | |
| Confidence-based routing | ✓ | | |
| Thompson Sampling agent selection | ✓ | | |

## Key Features

- **Fluent DSL** — Intuitive workflow definitions that read like natural language
- **Roslyn Source Generators** — Compile-time validation; invalid workflows fail at build time
- **Thompson Sampling** — Contextual multi-armed bandit for intelligent agent selection
- **Confidence Routing** — Automatic escalation to human review for low-confidence decisions
- **Event-Sourced Audit Trail** — Complete decision history: what the agent saw, what it decided, which model version produced the output
- **Durable by Default** — Automatic persistence via Wolverine sagas and Marten event sourcing
- **Human-in-the-Loop** — Built-in approval workflows with timeout escalation
- **Compensation Handlers** — Explicit rollback for AI decisions when workflows fail

## Quick Start

```bash
dotnet add package Agentic.Workflow
dotnet add package Agentic.Workflow.Generators
```

```csharp
services.AddAgenticWorkflow()
    .AddWorkflow<ProcessOrderWorkflow>();
```

## Documentation

- [Design Document](docs/design/agentic-workflow-library.md) — Architecture and design rationale
- [Examples](docs/examples/) — Branching, fork/join, approvals, Thompson Sampling

## Requirements

- .NET 10 or later
- PostgreSQL (for Wolverine/Marten persistence)

## License

MIT — see [LICENSE](LICENSE) for details.
