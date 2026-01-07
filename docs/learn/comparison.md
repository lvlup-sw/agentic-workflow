---
outline: deep
---

# Framework Comparison

How does Agentic.Workflow compare to other solutions? This page helps you understand when to choose Agentic.Workflow versus alternatives.

## Quick Comparison

| Capability | Agentic.Workflow | LangGraph | MS Agent Framework | Temporal |
|------------|:----------------:|:---------:|:------------------:|:--------:|
| .NET native | :white_check_mark: | | :white_check_mark: | :white_check_mark: |
| Durable execution | event-sourced | checkpoints | checkpoints | event history |
| Full audit trail | :white_check_mark: | | | |
| Confidence-based routing | :white_check_mark: | | | |
| Thompson Sampling | :white_check_mark: | | | |
| Compile-time validation | :white_check_mark: | | | |
| Human-in-the-loop | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Compensation handlers | :white_check_mark: | | | :white_check_mark: |
| Production status | 1.0 | stable | preview | stable |

## Detailed Comparison

### LangGraph

[LangGraph](https://www.langchain.com/langgraph) is part of the LangChain ecosystem and provides a graph-based approach to building agent workflows in Python.

**Strengths:**
- Rich ecosystem of integrations
- Active community and documentation
- Flexible graph-based composition
- Built-in support for common agent patterns

**Limitations:**
- Python-only (no native .NET support)
- Checkpoint-based persistence doesn't capture full decision context
- No compile-time validation of workflow structure
- Limited confidence handling primitives

**Choose LangGraph when:**
- Your team is Python-native
- You need deep LangChain ecosystem integration
- Rapid prototyping is more important than audit compliance

**Choose Agentic.Workflow when:**
- You're building in .NET
- You need complete audit trails for compliance
- Compile-time safety is a priority
- You want intelligent agent selection (Thompson Sampling)

### Microsoft Agent Framework

[Microsoft Agent Framework](https://learn.microsoft.com/en-us/agent-framework/) (currently in preview) is Microsoft's approach to building AI agent applications in .NET.

**Strengths:**
- Native .NET integration
- Backed by Microsoft
- Integrates with Azure AI services
- Good developer experience

**Limitations:**
- Still in preview, API may change
- Checkpoint-based persistence
- Limited workflow patterns (no explicit phases or compensation)
- No built-in confidence routing or multi-armed bandit selection

**Choose MS Agent Framework when:**
- You want tight Azure ecosystem integration
- You're building simpler agent applications
- You're comfortable with preview software

**Choose Agentic.Workflow when:**
- You need production-ready stability
- Event-sourced audit trails are required
- You want advanced patterns like Thompson Sampling
- Explicit compensation handlers are important

### Temporal

[Temporal](https://temporal.io/) is a battle-tested workflow orchestration platform used by major companies for mission-critical workflows.

**Strengths:**
- Extremely mature and battle-tested
- Excellent durability guarantees
- Strong support for long-running workflows
- Good .NET SDK

**Limitations:**
- No awareness of AI-specific patterns
- Requires custom work for confidence handling
- No built-in agent selection strategies
- Event history is lower-level than AI-focused abstractions

**Choose Temporal when:**
- You have existing Temporal infrastructure
- Your workflows aren't primarily AI-driven
- You need Temporal's specific scale characteristics

**Choose Agentic.Workflow when:**
- Your workflows center around AI agent decisions
- You want confidence routing out of the box
- Thompson Sampling for agent selection is valuable
- You prefer AI-specific abstractions

## Unique Features Explained

### Event-Sourced Audit Trail

Unlike checkpoint-based systems, Agentic.Workflow captures every decision as an immutable event. This means you can answer questions like:

- What prompt was sent to the model?
- What was the model's confidence level?
- What version of the model produced this output?
- What was the full context when this decision was made?

This is essential for debugging production issues, compliance requirements, and understanding agent behavior over time.

### Thompson Sampling

[Thompson Sampling](https://en.wikipedia.org/wiki/Thompson_sampling) is a contextual multi-armed bandit algorithm used for intelligent agent selection. When multiple agents can handle a task, Thompson Sampling balances:

- **Exploitation** - Using agents that have performed well
- **Exploration** - Trying potentially better agents

This is more sophisticated than simple round-robin or static selection, especially as you accumulate performance data.

```csharp
.Branch<ClassifyIntent>()
    .When(intent => intent == Intent.Sales, b => b
        .Using<AgentSelector>(selector => selector
            .WithThompsonSampling()
            .Choose<SalesAgentA, SalesAgentB, SalesAgentC>()))
```

### Compile-Time Validation

Roslyn source generators analyze your workflow definitions at compile time and generate type-safe artifacts. This catches errors early:

- Missing step implementations
- Invalid phase transitions
- Type mismatches in state
- Unreachable workflow paths

If your workflow definition has structural problems, you'll see them as build errors with clear diagnostics, not runtime exceptions.

### Confidence-Based Routing

AI agents don't always produce high-confidence outputs. Agentic.Workflow lets you define explicit routing based on confidence levels:

```csharp
.Then<ClassifyDocument>()
    .OnConfidence(c => c
        .When(conf => conf >= 0.9, b => b.Then<AutoProcess>())
        .When(conf => conf >= 0.5, b => b.Then<HumanReview>())
        .Otherwise(b => b.Then<ManualClassification>()))
```

Low-confidence decisions automatically route to human review or alternative processing, without custom conditional logic.

## Decision Guide

**You need Agentic.Workflow if:**
- :white_check_mark: Building AI agent workflows in .NET
- :white_check_mark: Audit compliance requires full decision history
- :white_check_mark: You want compile-time safety for workflow definitions
- :white_check_mark: Intelligent agent selection would improve outcomes
- :white_check_mark: Confidence-based routing is important

**Consider alternatives if:**
- Your team is Python-native (LangGraph)
- You're not building AI-driven workflows (Temporal)
- You need tight Azure integration above all else (MS Agent Framework)
- You're prototyping and don't need durability yet

## What's Next

Ready to get started? Head to the [installation guide](/guide/installation) to add Agentic.Workflow to your project, or see [complete examples](/examples/) of workflows in action.
