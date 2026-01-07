# Agentic.Workflow

[![NuGet](https://img.shields.io/nuget/v/Agentic.Workflow.svg)](https://www.nuget.org/packages/Agentic.Workflow)
[![Build Status](https://img.shields.io/github/actions/workflow/status/lvlup-sw/agentic-workflow/ci.yml?branch=main)](https://github.com/lvlup-sw/agentic-workflow/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

> Deterministic orchestration for probabilistic AI agents

## Documentation

**[View the full documentation](https://lvlup-sw.github.io/agentic-workflow/)**

- [Learn](https://lvlup-sw.github.io/agentic-workflow/learn/) - Core concepts and value proposition
- [Guide](https://lvlup-sw.github.io/agentic-workflow/guide/) - Step-by-step tutorials
- [Reference](https://lvlup-sw.github.io/agentic-workflow/reference/) - API documentation
- [Examples](https://lvlup-sw.github.io/agentic-workflow/examples/) - Real-world workflows

## The Problem

AI agents are inherently probabilistic—given the same input, an LLM may produce different outputs. Current solutions force an unsatisfying choice:

- **Agent frameworks** ([LangGraph](https://www.langchain.com/langgraph), [MS Agent Framework](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview)) offer great developer experience but rely on checkpoint-based persistence—they can resume workflows, but can't answer "what did the agent see when it made that decision?"

- **Workflow engines** ([Temporal](https://temporal.io/)) provide battle-tested durability but have no awareness of agent-specific patterns: confidence handling, context assembly, AI-aware compensation.

## The Solution

Agentic.Workflow bridges these domains with a key insight: while agent *outputs* are probabilistic, the *workflow itself* can be deterministic if we treat each agent decision as an immutable event in an event-sourced system.

```csharp
var workflow = Workflow<OrderState>
    .Create("process-order")
    .StartWith<ValidateOrder>()
    .Then<ProcessPayment>()
    .Then<FulfillOrder>()
    .Finally<SendConfirmation>();
```

## How It Works

The library builds on proven .NET infrastructure rather than reinventing durability:

**[Wolverine](https://wolverine.netlify.app/)** provides saga orchestration—each workflow becomes a saga with automatic message routing, transactional outbox (state + messages commit atomically), and retry policies.

**[Marten](https://martendb.io/)** provides event sourcing—every step completion, branch decision, and approval is captured as an immutable event in PostgreSQL. This enables time-travel debugging ("what was the state when this decision was made?") and complete audit trails.

**Roslyn Source Generators** transform fluent DSL definitions into type-safe artifacts at compile time: phase enums, commands, events, saga handlers, and state reducers. Invalid workflows fail at build time with clear diagnostics, not at runtime with cryptic exceptions.

## Packages

| Package | Purpose |
|---------|---------|
| `Agentic.Workflow` | Core fluent DSL and abstractions |
| `Agentic.Workflow.Generators` | Compile-time source generation (sagas, events, phase enums) |
| `Agentic.Workflow.Infrastructure` | Production implementations (Thompson Sampling, loop detection, budgets) |
| `Agentic.Workflow.Agents` | Microsoft Agent Framework integration for LLM-powered steps |
| `Agentic.Workflow.Rag` | Vector store adapters for RAG patterns |

**Minimal setup** (workflows without LLM agents):
```bash
dotnet add package Agentic.Workflow
dotnet add package Agentic.Workflow.Generators
```

**With LLM integration** (most common):
```bash
dotnet add package Agentic.Workflow
dotnet add package Agentic.Workflow.Generators
dotnet add package Agentic.Workflow.Agents
dotnet add package Agentic.Workflow.Infrastructure
```

See [Package Documentation](docs/packages.md) for detailed guidance.

## How It Compares

| Capability | Agentic.Workflow | [LangGraph](https://www.langchain.com/langgraph) | [MS Agent Framework](https://learn.microsoft.com/en-us/agent-framework/) | [Temporal](https://temporal.io/) |
|------------|:----------------:|:---------:|:------------------:|:--------:|
| .NET native | ✓ | | ✓ | ✓ |
| Durable execution | event-sourced | checkpoints | checkpoints | event history |
| Full audit trail | ✓ | | | |
| Confidence-based routing | ✓ | | | |
| Thompson Sampling | ✓ | | | |
| Compile-time validation | ✓ | | | |
| Human-in-the-loop | ✓ | ✓ | ✓ | ✓ |
| Compensation handlers | ✓ | | | ✓ |
| Production status | 1.0 | stable | preview | stable |

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

```csharp
// Register workflows at startup
services.AddAgenticWorkflow()
    .AddWorkflow<ProcessOrderWorkflow>();

// Define a workflow
public class ProcessOrderWorkflow : IWorkflowDefinition<OrderState>
{
    public IWorkflow<OrderState> Define() =>
        Workflow<OrderState>
            .Create("process-order")
            .StartWith<ValidateOrder>()
            .Then<ProcessPayment>()
            .Finally<SendConfirmation>();
}
```

## Requirements

- .NET 10 or later
- PostgreSQL (for Wolverine/Marten persistence)

## License

MIT — see [LICENSE](LICENSE) for details.
