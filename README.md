# Agentic.Workflow

[![NuGet](https://img.shields.io/nuget/v/Agentic.Workflow.svg)](https://www.nuget.org/packages/Agentic.Workflow)
[![Build Status](https://img.shields.io/github/actions/workflow/status/lvlup-sw/agentic-workflow/ci.yml?branch=main)](https://github.com/lvlup-sw/agentic-workflow/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET library for building production-grade agentic workflows with deterministic orchestration over probabilistic AI agents.

## Features

- **Fluent DSL** - Intuitive workflow definitions that read like natural language
- **Roslyn Source Generators** - Compile-time validation with generated sagas, events, and state machines
- **Thompson Sampling** - Contextual multi-armed bandit agent selection with Beta priors
- **Loop Detection** - Automatic detection of stuck workflows (exact/semantic repetition, oscillation)
- **Budget Guard** - Resource budget enforcement (steps, tokens, wall time)
- **Durable by Default** - Automatic persistence via Wolverine sagas and Marten event sourcing
- **Human-in-the-Loop** - Built-in approval workflows with timeout escalation
- **Complete Audit Trail** - Event-sourced decision history for compliance and debugging

## Quick Start

### Installation

```bash
dotnet add package Agentic.Workflow
dotnet add package Agentic.Workflow.Generators
dotnet add package Agentic.Workflow.Infrastructure
```

### Define a Workflow

```csharp
using Agentic.Workflow;

var workflow = Workflow<OrderState>
    .Create("process-order")
    .StartWith<ValidateOrder>()
    .Then<ProcessPayment>()
    .Then<FulfillOrder>()
    .Finally<SendConfirmation>();
```

### Implement Steps

```csharp
public class ValidateOrder : IWorkflowStep<OrderState>
{
    public async Task<StepResult<OrderState>> ExecuteAsync(
        OrderState state,
        StepContext context,
        CancellationToken ct)
    {
        // Validation logic
        return state.With(s => s.IsValid, true).AsResult();
    }
}
```

### Register Services

```csharp
services.AddAgenticWorkflow()
    .AddWorkflow<ProcessOrderWorkflow>();
```

## Workflow Patterns

### Conditional Branching

```csharp
Workflow<ClaimState>
    .Create("process-claim")
    .StartWith<AssessClaim>()
    .Branch(state => state.ClaimType,
        when: ClaimType.Auto, then: flow => flow
            .Then<AutoClaimProcessor>(),
        when: ClaimType.Property, then: flow => flow
            .Then<PropertyInspection>()
            .Then<PropertyClaimProcessor>(),
        otherwise: flow => flow
            .Then<ManualReview>())
    .Finally<NotifyClaimant>();
```

### Parallel Execution

```csharp
Workflow<AnalysisState>
    .Create("comprehensive-analysis")
    .StartWith<GatherData>()
    .Fork(
        flow => flow.Then<FinancialAnalysis>(),
        flow => flow.Then<TechnicalAnalysis>(),
        flow => flow.Then<MarketAnalysis>())
    .Join<SynthesizeResults>()
    .Finally<GenerateReport>();
```

### Human Approval

```csharp
Workflow<DocumentState>
    .Create("document-approval")
    .StartWith<DraftDocument>()
    .AwaitApproval<LegalTeam>(options => options
        .WithTimeout(TimeSpan.FromDays(2))
        .OnTimeout(flow => flow.Then<EscalateToManager>()))
    .Then<PublishDocument>()
    .Finally<NotifyStakeholders>();
```

### Iterative Refinement

```csharp
Workflow<RefinementState>
    .Create("iterative-refinement")
    .StartWith<GenerateDraft>()
    .RepeatUntil(
        condition: state => state.QualityScore >= 0.9m,
        maxIterations: 5,
        body: flow => flow
            .Then<Critique>()
            .Then<Refine>())
    .Finally<Publish>();
```

## Packages

| Package | Description |
|---------|-------------|
| `Agentic.Workflow` | Core DSL, abstractions, and Thompson Sampling types |
| `Agentic.Workflow.Generators` | Roslyn source generators for saga/event generation |
| `Agentic.Workflow.Infrastructure` | Infrastructure implementations (belief stores, selectors) |
| `Agentic.Workflow.Agents` | Agent-specific integrations (MAF, Semantic Kernel) |
| `Agentic.Workflow.Rag` | RAG integration with vector search adapters |

## Documentation

- [Design Document](docs/design/agentic-workflow-library.md) - Complete architecture and design rationale
- [Basic Workflow](docs/examples/basic-workflow.md) - Linear workflow example
- [Branching](docs/examples/branching.md) - Conditional routing
- [Fork/Join](docs/examples/fork-join.md) - Parallel execution
- [Approval Flow](docs/examples/approval-flow.md) - Human-in-the-loop
- [Iterative Refinement](docs/examples/iterative-refinement.md) - Quality loops
- [Thompson Sampling](docs/examples/thompson-sampling.md) - Agent selection

## Requirements

- .NET 10 or later
- PostgreSQL (for Wolverine/Marten persistence)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on code style, testing, and the PR process.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
