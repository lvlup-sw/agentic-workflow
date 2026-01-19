# Examples

Complete, end-to-end workflow implementations demonstrating real-world patterns you can adapt for your own use cases.

## What These Examples Demonstrate

Each example showcases:

- **Complete state definitions** with realistic domain models
- **Full workflow definitions** using the fluent DSL
- **Step implementations** with dependency injection
- **Error handling** and compensation patterns
- **Registration** and API endpoints
- **Generated artifacts** explanation

## End-to-End Workflows

| Example | Patterns | Description |
|---------|----------|-------------|
| [Order Processing](./order-processing.md) | Linear, Error Handling, Compensation | E-commerce order workflow from validation through fulfillment |
| [Content Pipeline](./content-pipeline.md) | Iterative Refinement, Thompson Sampling, Approval | AI-powered content generation with quality loops |
| [Code Review](./code-review.md) | Fork/Join, Branching, Event Sourcing | Automated PR analysis with parallel checks |

## Pattern Examples

| Example | Pattern | Description |
|---------|---------|-------------|
| [Basic Workflow](./basic-workflow.md) | Linear | Simple sequential step execution |
| [Branching](./branching.md) | Conditional | Route execution based on state |
| [Fork/Join](./fork-join.md) | Parallel | Execute steps concurrently |
| [Iterative Refinement](./iterative-refinement.md) | Loops | Repeat until quality threshold |
| [Approval Flow](./approval-flow.md) | Human-in-the-loop | Pause for human decisions |
| [Thompson Sampling](./thompson-sampling.md) | Agent Selection | Intelligent agent routing |

## Prerequisites

Before running these examples, ensure you have:

1. **.NET 9.0 or later** installed
2. **PostgreSQL** running (for Marten event store)
3. **Agentic.Workflow packages** installed:

```bash
dotnet add package Agentic.Workflow
dotnet add package Agentic.Workflow.Generators
```

## Running the Examples

Each example includes complete code that can be copied into a project. The typical setup pattern is:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Wolverine for message handling
builder.Host.UseWolverine(opts =>
{
    opts.Durability.Mode = DurabilityMode.Solo;
});

// Add Marten for persistence
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Marten")!);
})
.IntegrateWithWolverine();

// Add workflow services
builder.Services.AddAgenticWorkflow()
    .AddWorkflow<YourWorkflow>();

// Register step dependencies
builder.Services.AddScoped<IYourService, YourServiceImpl>();

var app = builder.Build();
app.MapControllers();
app.Run();
```

## Code Organization

Each example follows this structure:

```text
Examples/
  YourWorkflow/
    State/
      YourState.cs           # State record
      DomainModels.cs        # Supporting types
    Steps/
      Step1.cs               # Step implementations
      Step2.cs
    Services/
      IYourService.cs        # Service interfaces
      YourServiceImpl.cs     # Implementations
    YourWorkflow.cs          # Workflow definition
    YourController.cs        # API endpoints
```

## Sample Applications

Complete, runnable sample projects demonstrating Agentic.Workflow patterns. Each sample can be executed with `dotnet run`.

| Sample | Run Command | What It Demonstrates |
|--------|-------------|---------------------|
| [ContentPipeline](https://github.com/lvlup-sw/agentic-workflow/tree/main/samples/ContentPipeline) | `dotnet run --project samples/ContentPipeline` | Human approval gates, compensation, audit trails |
| [MultiModelRouter](https://github.com/lvlup-sw/agentic-workflow/tree/main/samples/MultiModelRouter) | `dotnet run --project samples/MultiModelRouter` | Thompson Sampling, intelligent model selection |
| [AgenticCoder](https://github.com/lvlup-sw/agentic-workflow/tree/main/samples/AgenticCoder) | `dotnet run --project samples/AgenticCoder` | Iterative refinement loops, human checkpoints |

See the [samples directory](https://github.com/lvlup-sw/agentic-workflow/tree/main/samples) for full source code.

## Key Points Across All Examples

- **State is immutable** - Use `With()` to create updated copies
- **Steps are resolved via DI** - Inject services through constructors
- **Failures are explicit** - Return `StepResult.Fail()` with error details
- **Workflows survive restarts** - Durability is automatic via Wolverine
- **Everything is audited** - Events capture all state transitions
