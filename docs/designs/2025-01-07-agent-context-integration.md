# Design: Agent Context Integration

**Feature ID:** agent-context-integration
**Date:** 2025-01-07
**Status:** Draft
**Packages Affected:** `Agentic.Workflow`, `Agentic.Workflow.Generators`, `Agentic.Workflow.Agents`, `Agentic.Workflow.Rag`

---

## Problem Statement

The Agentic.Workflow library has fluent DSL infrastructure for context assembly and RAG integration, but the generator doesn't carry this metadata through to code generation, and there's no runtime bridge connecting assembled context to agent step execution.

**Current State:**
- `.WithContext()` and `.FromRetrieval()` builders exist but produce definitions that go unused
- `IAgentStep<TState>` exists but has no context awareness
- `IVectorSearchAdapter` exists but isn't integrated with the workflow DSL
- OnFailure emitter exists but may need verification and step-level integration

**Target State:**
- Generator emits typed `IContextAssembler<TState>` per step with context configuration
- `AgentStepBase<TState>` automatically assembles context before LLM calls
- RAG retrieval flows seamlessly from DSL configuration to runtime execution
- OnFailure handlers properly capture exception context and integrate with compensation

---

## Chosen Approach

**Hybrid: Compile-Time Validation, Runtime Execution**

The generator validates context configuration at compile time and emits strongly-typed context assembler classes. Runtime executes these assemblers via DI, providing flexibility for swapping vector adapters without recompilation.

**Rationale:**
- Matches existing generator patterns (sagas, commands, events)
- Provides compile-time safety for expression validation
- Generated code is readable and debuggable
- Allows vector adapter swapping at runtime via DI

---

## Technical Design

### 1. Generator Model Extensions

Extend `StepModel` to carry context metadata:

```csharp
// In Agentic.Workflow.Generators/Models/StepModel.cs
public record StepModel
{
    // Existing properties...
    public string StepName { get; init; }
    public string StepTypeName { get; init; }
    public string? InstanceName { get; init; }

    // NEW: Context assembly metadata
    public ContextModel? Context { get; init; }
}

public record ContextModel
{
    public IReadOnlyList<ContextSourceModel> Sources { get; init; } = [];
}

public abstract record ContextSourceModel;

public record StateContextSourceModel(
    string PropertyPath,
    string PropertyType,
    string AccessExpression) : ContextSourceModel;

public record RetrievalContextSourceModel(
    string CollectionTypeName,
    string? QueryExpression,      // null if literal query
    string? LiteralQuery,         // null if expression query
    int TopK,
    decimal MinRelevance,
    IReadOnlyList<RetrievalFilterModel> Filters) : ContextSourceModel;

public record LiteralContextSourceModel(string Value) : ContextSourceModel;

public record RetrievalFilterModel(
    string Key,
    string? StaticValue,
    string? ValueExpression);
```

### 2. Context Assembler Generation

New emitter generates typed assembler per step:

```csharp
// Generated output example for step with context
namespace MyApp.Workflows.Generated;

internal sealed class AnalyzeDocumentContextAssembler
    : IContextAssembler<DocumentState>
{
    private readonly IVectorSearchAdapter<ResearchLibrary> _researchLibrary;

    public AnalyzeDocumentContextAssembler(
        IVectorSearchAdapter<ResearchLibrary> researchLibrary)
    {
        _researchLibrary = researchLibrary;
    }

    public async Task<AssembledContext> AssembleAsync(
        DocumentState state,
        StepContext stepContext,
        CancellationToken ct)
    {
        var builder = new AssembledContextBuilder();

        // From state
        builder.AddStateContext("DocumentSummary", state.DocumentSummary);

        // From retrieval
        var retrievalResults = await _researchLibrary.SearchAsync(
            query: state.AnalysisQuery,
            topK: 5,
            minRelevance: 0.7,
            cancellationToken: ct);
        builder.AddRetrievalContext("ResearchLibrary", retrievalResults);

        // From literal
        builder.AddLiteralContext("Always cite sources.");

        return builder.Build();
    }
}
```

### 3. Runtime Abstractions

#### IContextAssembler Interface

```csharp
// In Agentic.Workflow.Agents/Abstractions/IContextAssembler.cs
namespace Agentic.Workflow.Agents;

public interface IContextAssembler<TState>
    where TState : class, IWorkflowState
{
    Task<AssembledContext> AssembleAsync(
        TState state,
        StepContext stepContext,
        CancellationToken cancellationToken);
}
```

#### AssembledContext Model

```csharp
// In Agentic.Workflow.Agents/Models/AssembledContext.cs
namespace Agentic.Workflow.Agents;

public sealed class AssembledContext
{
    public IReadOnlyList<ContextSegment> Segments { get; }
    public IReadOnlyList<RetrievalResult> RetrievalResults { get; }

    public string ToPromptString() =>
        string.Join("\n\n", Segments.Select(s => s.ToPromptString()));

    public string ToPromptString(ContextFormatOptions options) =>
        // Formatted output with headers, separators, etc.
}

public abstract record ContextSegment
{
    public abstract string ToPromptString();
}

public record StateContextSegment(string Name, object? Value) : ContextSegment
{
    public override string ToPromptString() =>
        Value?.ToString() ?? string.Empty;
}

public record RetrievalContextSegment(
    string CollectionName,
    IReadOnlyList<RetrievalResult> Results) : ContextSegment
{
    public override string ToPromptString() =>
        string.Join("\n---\n", Results.Select(r => r.Content));
}

public record LiteralContextSegment(string Value) : ContextSegment
{
    public override string ToPromptString() => Value;
}

public record RetrievalResult(
    string Content,
    double Score,
    string? SourceId,
    IReadOnlyDictionary<string, object?>? Metadata);
```

### 4. AgentStepBase Implementation

```csharp
// In Agentic.Workflow.Agents/AgentStepBase.cs
namespace Agentic.Workflow.Agents;

public abstract class AgentStepBase<TState> : IAgentStep<TState>
    where TState : class, IWorkflowState
{
    private readonly IChatClient _chatClient;
    private readonly IContextAssembler<TState>? _contextAssembler;
    private readonly IConversationThreadManager? _threadManager;

    protected AgentStepBase(
        IChatClient chatClient,
        IContextAssembler<TState>? contextAssembler = null,
        IConversationThreadManager? threadManager = null)
    {
        _chatClient = chatClient;
        _contextAssembler = contextAssembler;
        _threadManager = threadManager;
    }

    public abstract string GetSystemPrompt();

    public virtual Type? GetOutputSchemaType() => null;

    public async Task<StepResult<TState>> ExecuteAsync(
        TState state,
        AgentStepContext context,
        CancellationToken ct)
    {
        // Assemble context if configured
        var assembledContext = _contextAssembler is not null
            ? await _contextAssembler.AssembleAsync(state, context.ToStepContext(), ct)
            : AssembledContext.Empty;

        // Build messages
        var messages = BuildMessages(state, assembledContext, context);

        // Execute LLM call
        var response = await _chatClient.GetResponseAsync(messages, ct);

        // Apply result to state
        return await ApplyResultAsync(state, response, assembledContext, context, ct);
    }

    protected virtual IEnumerable<ChatMessage> BuildMessages(
        TState state,
        AssembledContext context,
        AgentStepContext stepContext)
    {
        yield return new ChatMessage(ChatRole.System, GetSystemPrompt());

        if (!context.IsEmpty)
        {
            yield return new ChatMessage(ChatRole.User,
                $"Context:\n{context.ToPromptString()}");
        }

        yield return new ChatMessage(ChatRole.User, GetUserPrompt(state));
    }

    protected abstract string GetUserPrompt(TState state);

    protected abstract Task<StepResult<TState>> ApplyResultAsync(
        TState state,
        ChatResponse response,
        AssembledContext context,
        AgentStepContext stepContext,
        CancellationToken ct);
}
```

### 5. Typed Vector Search Adapter

```csharp
// In Agentic.Workflow.Rag/Abstractions/IVectorSearchAdapter.cs
namespace Agentic.Workflow.Rag;

// Marker interface for collection registration
public interface IRagCollection { }

// Typed adapter for specific collections
public interface IVectorSearchAdapter<TCollection>
    where TCollection : IRagCollection
{
    Task<IReadOnlyList<RetrievalResult>> SearchAsync(
        string query,
        int topK = 5,
        double minRelevance = 0.7,
        IReadOnlyDictionary<string, object>? filters = null,
        CancellationToken cancellationToken = default);
}

// Registration helper
public static class RagServiceExtensions
{
    public static IServiceCollection AddRagCollection<TCollection, TAdapter>(
        this IServiceCollection services)
        where TCollection : IRagCollection
        where TAdapter : class, IVectorSearchAdapter<TCollection>
    {
        services.AddSingleton<IVectorSearchAdapter<TCollection>, TAdapter>();
        return services;
    }
}
```

### 6. OnFailure Handler Verification

The existing `SagaFailureHandlerComponentEmitter` should be verified to:

1. **Capture exception context** in generated events:
```csharp
public record WorkflowFailedEvent(
    Guid WorkflowId,
    string FailedStepName,
    string ExceptionType,
    string ExceptionMessage,
    string? StackTrace,
    DateTimeOffset Timestamp) : IProgressEvent;
```

2. **Support step-level OnFailure** via `StepConfigurationDefinition`:
```csharp
.Then<ProcessPayment>(step => step
    .OnFailure(failure => failure
        .Then<NotifyPaymentFailure>()
        .Then<RefundAttempt>()))
```

3. **Chain to compensation** when both are configured:
```csharp
.Then<ProcessPayment>(step => step
    .Compensate<RefundPayment>()
    .OnFailure(failure => failure.Then<NotifySupport>()))
// On failure: runs NotifySupport, then triggers RefundPayment compensation
```

---

## Integration Points

### Generator Pipeline

```
WorkflowDefinition
    ↓
DefinitionVisitor (extracts StepModel with ContextModel)
    ↓
SagaEmitter
    ├── SagaStepHandlersEmitter (existing)
    ├── SagaFailureHandlerComponentEmitter (existing, verify)
    └── ContextAssemblerEmitter (NEW)
    ↓
Generated Artifacts:
    - {Workflow}Saga.cs (existing)
    - {Workflow}Commands.cs (existing)
    - {Workflow}Events.cs (existing)
    - {Step}ContextAssembler.cs (NEW, per step with context)
```

### DI Registration

Generated extension method registers assemblers:

```csharp
// Generated in {Workflow}ServiceExtensions.cs
public static IServiceCollection AddProcessOrderWorkflow(
    this IServiceCollection services)
{
    // Existing registrations...
    services.AddTransient<ValidateOrderStep>();
    services.AddTransient<ProcessPaymentStep>();

    // NEW: Context assembler registrations
    services.AddTransient<IContextAssembler<OrderState>,
        AnalyzeOrderContextAssembler>();

    return services;
}
```

### Saga Handler Integration

Generated saga handlers inject assemblers:

```csharp
// In generated saga
public async Task Handle(
    AnalyzeOrderStepStart command,
    IDocumentSession session,
    IMessageBus bus,
    AnalyzeOrderStep step,
    IContextAssembler<OrderState>? contextAssembler, // Injected if configured
    CancellationToken ct)
{
    var context = new AgentStepContext(
        ChatClient: _chatClient,
        WorkflowId: WorkflowId,
        StepName: "AnalyzeOrder",
        StepExecutionId: Guid.NewGuid(),
        ContextAssembler: contextAssembler);

    var result = await step.ExecuteAsync(State, context, ct);
    // ... handle result
}
```

---

## Testing Strategy

### Unit Tests

1. **Generator Tests**
   - `ContextModelExtractor` correctly extracts context from definitions
   - `ContextAssemblerEmitter` generates valid C# for all context source types
   - Generated assemblers compile without errors
   - Expression-based queries emit correct accessor code

2. **Runtime Tests**
   - `AssembledContext` correctly formats segments
   - `AgentStepBase` calls assembler when present, skips when null
   - Vector search adapter receives correct parameters from definition

3. **Integration Tests**
   - End-to-end: DSL → Generator → Runtime execution
   - Context flows correctly through saga handlers
   - RAG results appear in assembled context

### Test Fixtures

```csharp
// Test collection marker
public class TestKnowledgeBase : IRagCollection { }

// In-memory adapter for testing
public class InMemoryVectorAdapter<T> : IVectorSearchAdapter<T>
    where T : IRagCollection
{
    private readonly List<(string Content, double Score)> _documents = [];

    public void Seed(params (string Content, double Score)[] documents) =>
        _documents.AddRange(documents);

    public Task<IReadOnlyList<RetrievalResult>> SearchAsync(...) =>
        Task.FromResult<IReadOnlyList<RetrievalResult>>(
            _documents
                .Where(d => d.Score >= minRelevance)
                .OrderByDescending(d => d.Score)
                .Take(topK)
                .Select(d => new RetrievalResult(d.Content, d.Score, null, null))
                .ToList());
}
```

---

## Implementation Phases

### Phase 1: Generator Model Extensions
- Extend `StepModel` with `ContextModel`
- Implement `ContextModelExtractor` to parse definitions
- Add unit tests for model extraction

### Phase 2: Context Assembler Generation
- Create `ContextAssemblerEmitter`
- Generate typed assemblers per step
- Register in DI extensions
- Add generator tests

### Phase 3: Runtime Abstractions
- Define `IContextAssembler<TState>`
- Implement `AssembledContext` and segments
- Add `AssembledContextBuilder`
- Add runtime unit tests

### Phase 4: AgentStepBase Implementation
- Create `AgentStepBase<TState>`
- Integrate context assembler injection
- Wire into saga handlers
- Add integration tests

### Phase 5: RAG Adapter Enhancement
- Add typed `IVectorSearchAdapter<TCollection>`
- Create registration extensions
- Add `InMemoryVectorAdapter` for testing
- Verify existing adapter compatibility

### Phase 6: OnFailure Verification & Enhancement
- Audit existing emitter for exception context capture
- Add step-level OnFailure support if missing
- Verify compensation integration
- Add failure scenario tests

---

## Open Questions

1. **Context Format Customization** — Should `AssembledContext.ToPromptString()` support templates/formatting options, or is a simple concatenation sufficient for MVP?

2. **Streaming Support** — Should `AgentStepBase` support streaming responses, and if so, how does context assembly interact with streaming?

3. **Context Caching** — For expensive RAG queries, should assemblers support caching within a workflow execution?

4. **Conversation History** — The existing `IConversationalState` interface exists. Should context assembly integrate with conversation history, or remain separate?

---

## Success Criteria

- [ ] Steps with `.WithContext()` compile and generate context assemblers
- [ ] `.FromRetrieval<T>()` flows to `IVectorSearchAdapter<T>` at runtime
- [ ] `AgentStepBase<TState>` automatically assembles context
- [ ] OnFailure handlers capture exception details
- [ ] All features work with existing Thompson Sampling, loop detection, and budget guard
- [ ] 90%+ test coverage on new code
- [ ] Documentation updated with usage examples

---

## References

- [Deferred Features Analysis](../deferred-features.md)
- [Package Documentation](../packages.md)
- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/en-us/dotnet/ai/)
