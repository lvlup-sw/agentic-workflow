# Agentic.Workflow

Fluent DSL for building durable agentic workflows with Wolverine sagas and Marten event sourcing.

## Installation

```bash
dotnet add package Agentic.Workflow
dotnet add package Agentic.Workflow.Generators
```

## Quick Start

```csharp
using Agentic.Workflow.Builders;
using Agentic.Workflow.Attributes;

// Define your workflow state
public record OrderState : IWorkflowState
{
    public Guid WorkflowId { get; init; }
    public OrderStatus Status { get; init; }
    public decimal Total { get; init; }
}

// Define workflow steps
public class ValidateOrder : IWorkflowStep<OrderState> { /* ... */ }
public class ProcessPayment : IWorkflowStep<OrderState> { /* ... */ }
public class FulfillOrder : IWorkflowStep<OrderState> { /* ... */ }

// Build the workflow with fluent DSL
[Workflow("process-order")]
public static partial class ProcessOrderWorkflow
{
    public static WorkflowDefinition<OrderState> Definition => Workflow<OrderState>
        .Create("process-order")
        .StartWith<ValidateOrder>()
        .Then<ProcessPayment>()
        .Finally<FulfillOrder>();
}
```

## Features

- **Fluent DSL**: Intuitive builder pattern for workflow definition
- **Source Generation**: Wolverine sagas, phase enums, and commands generated at compile time
- **Branching**: Conditional paths with `.Branch()` and `.Case()`
- **Loops**: Iterative refinement with `.RepeatUntil()` and `.While()`
- **Parallelism**: Fork/join patterns with `.Fork()` and `.Join()`
- **Approvals**: Human-in-the-loop with `.AwaitApproval()` and escalation
- **Failure Handling**: Recovery paths with `.OnFailure()`
- **Validation**: Guard clauses with `.Validate()`

## Documentation

See [Agentic.Workflow Design Document](https://github.com/levelup-software/agentic-workflow/blob/main/docs/adrs/agentic-workflow-library.md) for complete documentation.

## License

MIT
