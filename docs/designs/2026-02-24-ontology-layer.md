# Agentic Ontology: Semantic Type System for Agentic Operations

> **Scope:** Library design for `Agentic.Ontology` NuGet packages (lives in agentic-workflow repo).
> Basileus is the first consumer; this repo receives ADR updates only.

---

## 1. Problem Statement

Three deficiencies in the current platform:

1. **Domain silos.** Trading, StyleEngine, and Knowledge are architecturally independent assemblies with no shared type vocabulary. An agent that ingests a document (Knowledge) and wants to inform a trading strategy has no typed mechanism to express that relationship.

2. **Flat tool discovery.** MCP tools are flat lists with string descriptions. Progressive disclosure tells agents *what tools exist* but not *what types flow between them*. An agent cannot ask "what actions can I take on a Position?" — it must grep tool descriptions.

3. **Blind planning.** Agents plan using natural language reasoning over unstructured context. There is no compile-time world model that constrains which operations are valid, which types they accept/produce, or how workflows chain together.

Palantir's Foundry Ontology solves all three with a semantic layer: Object Types + Link Types + Action Types + Interfaces, backed by a code-generated SDK (OSDK). We adapt their key architectural insights for the Agentic.Workflow ecosystem.

---

## 2. Design Principles

| Principle | Implication |
|-----------|------------|
| Compile-time over reflection | Roslyn source generators, not runtime type scanning |
| Extend progressive disclosure | Ontology metadata enhances tool stubs, doesn't replace filesystem discovery |
| Deterministic structure for stochastic agents | Typed action graph constrains agent planning (CMDP action space reduction) |
| Domain independence preserved | Cross-domain links resolved at composition, not declaration |
| NuGet package | Reusable across any Agentic.Workflow consumer, not basileus-specific |
| EF Core familiarity | Fluent builder API follows established .NET conventions |

---

## 3. Package Architecture

```text
agentic-workflow/
├── src/
│   ├── Agentic.Ontology/              # Contracts, DSL, builder interfaces
│   │   ├── DomainOntology.cs           # Base class for domain modules
│   │   ├── IOntologyBuilder.cs         # Fluent API entry point
│   │   ├── IObjectTypeBuilder.cs       # Object type configuration
│   │   ├── IActionBuilder.cs           # Action definition
│   │   ├── IInterfaceBuilder.cs        # Polymorphic interfaces
│   │   ├── ICrossDomainLinkBuilder.cs  # Cross-domain relationships
│   │   ├── Descriptors/               # Compile-time metadata types
│   │   │   ├── ObjectTypeDescriptor.cs
│   │   │   ├── PropertyDescriptor.cs
│   │   │   ├── LinkDescriptor.cs
│   │   │   ├── ActionDescriptor.cs
│   │   │   └── DomainDescriptor.cs
│   │   └── Query/                     # Agent-facing query contracts
│   │       ├── IOntologyQuery.cs
│   │       └── OntologyQueryResult.cs
│   │
│   ├── Agentic.Ontology.Generators/   # Roslyn incremental source generator
│   │   ├── OntologySourceGenerator.cs  # Entry point (IIncrementalGenerator)
│   │   ├── Analyzers/
│   │   │   ├── DomainOntologyAnalyzer.cs   # Parses DomainOntology.Define()
│   │   │   └── CompositionAnalyzer.cs      # Merges cross-assembly domains
│   │   └── Emitters/
│   │       ├── RegistryEmitter.cs          # OntologyRegistry
│   │       ├── DescriptorEmitter.cs        # Per-object descriptors
│   │       ├── AccessorEmitter.cs          # Typed accessors
│   │       └── ValidationEmitter.cs        # Cross-domain link validation
│   │
│   └── Agentic.Ontology.MCP/         # Progressive disclosure integration
│       ├── OntologyStubGenerator.cs    # Enhanced .pyi stub generation
│       ├── OntologyToolDiscovery.cs    # Semantic tool discovery
│       └── OntologyMcpTools.cs         # MCP tools for ontology queries
```

**Dependency graph:**

```text
Agentic.Ontology.MCP
    ↓
Agentic.Ontology  ←──  Agentic.Ontology.Generators (analyzer ref)
    ↓
Agentic.Workflow (optional — for workflow binding)
```

`Agentic.Ontology` has zero dependency on `Agentic.Workflow`. The workflow binding (`BoundToWorkflow`) is an optional extension method provided when both packages are referenced.

---

## 4. Palantir → Agentic Ontology Mapping

| Palantir Concept | Agentic Ontology | Notes |
|-----------------|------------------|-------|
| Object Type | `builder.Object<T>()` | Maps existing C# type into ontology |
| Object | Instance of `T` at runtime | Ontology doesn't own storage |
| Property | `obj.Property(x => x.Prop)` | Expression tree → compile-time extraction |
| Link Type | `obj.HasMany<T>()`, `ManyToMany<T>()` | Typed, directional, optional edge type |
| Action Type | `obj.Action("name")` | Bound to workflow or MCP tool |
| Interface | `builder.Interface<T>()` | C# interface → ontology polymorphism |
| OSDK | Source-generated `OntologyAccessor` | Typed, per-domain accessors |
| OMS (metadata service) | Source-generated `OntologyRegistry` | Compile-time catalog |
| Object Storage V2 | Domain-owned (Marten, pgvector, etc.) | Ontology doesn't own persistence |
| Object Set Service | `IOntologyQuery` | Agent-facing query interface |
| Security policies | Deferred to ControlPlane policy engine | Future: per-object permissions in ontology |

**Key architectural difference:** Palantir's Ontology owns storage (Object Storage V2). Ours does not — it is a semantic layer over domain-owned persistence. This keeps domains autonomous and avoids a monolithic data layer.

---

## 5. DSL Specification

### 5.1 DomainOntology — Entry Point

Each domain assembly defines exactly one `DomainOntology` subclass. The source generator discovers it and parses the `Define` method body.

```csharp
namespace Basileus.Trading;

public sealed class TradingOntology : DomainOntology
{
    public override string DomainName => "trading";

    protected override void Define(IOntologyBuilder builder)
    {
        // Object types, links, actions, interfaces defined here
    }
}
```

The source generator:
1. Finds all types deriving from `DomainOntology` in the compilation
2. Parses the `Define` method's syntax tree
3. Extracts builder method calls and their arguments
4. Emits descriptors and registry types

### 5.2 Object Types

Object types map existing C# records/classes into the ontology. The `Object<T>()` call declares that `T` participates in the semantic layer. Properties are selectively exposed via expression trees.

```csharp
protected override void Define(IOntologyBuilder builder)
{
    builder.Object<Position>(obj =>
    {
        obj.Key(p => p.Id);
        obj.Property(p => p.Symbol).Required();
        obj.Property(p => p.Quantity);
        obj.Property(p => p.UnrealizedPnL).Computed();
        obj.Property(p => p.OpenedAt);
    });

    builder.Object<TradeOrder>(obj =>
    {
        obj.Key(o => o.OrderId);
        obj.Property(o => o.Side);
        obj.Property(o => o.Price);
        obj.Property(o => o.FilledQuantity);
        obj.Property(o => o.Status);
    });

    builder.Object<Strategy>(obj =>
    {
        obj.Key(s => s.Id);
        obj.Property(s => s.Name).Required();
        obj.Property(s => s.Description);
    });
}
```

**Design decisions:**

- `Object<T>()` takes a generic parameter referencing an existing type. The ontology does not generate domain types — it maps them.
- `Key()` designates the identity property. Required for entity resolution and linking.
- `Property()` uses expression trees for compile-time member resolution. Only explicitly exposed properties are ontology-visible.
- `.Required()` and `.Computed()` are metadata hints for progressive disclosure stubs and validation.

### 5.3 Links

Links define typed, directional relationships between object types. Three cardinalities are supported, mirroring Palantir's link types.

```csharp
builder.Object<Position>(obj =>
{
    // One Position has many TradeOrders
    obj.HasMany<TradeOrder>("Orders");

    // One Position belongs to one Strategy
    obj.HasOne<Strategy>("Strategy");
});

builder.Object<Strategy>(obj =>
{
    // Inverse: one Strategy has many Positions
    obj.HasMany<Position>("Positions");
});
```

**Many-to-many with edge types** (for relationships that carry data):

```csharp
builder.Object<AtomicNote>(obj =>
{
    obj.ManyToMany<AtomicNote>("SemanticLinks", edge =>
    {
        edge.Property<LinkType>("Type");
        edge.Property<double>("Confidence");
        edge.Property<string>("ContextDescription");
    });
});
```

The edge type is a lightweight struct with no separate ontology registration — its properties are declared inline. For complex edges backed by existing C# types:

```csharp
obj.ManyToMany<AtomicNote>("SemanticLinks")
    .WithEdge<KnowledgeLink>(edge =>
    {
        edge.MapProperty(l => l.Type);
        edge.MapProperty(l => l.Confidence);
    });
```

### 5.4 Actions

Actions are operations that can be performed on an object type. Each action declares its input/output types and its execution binding (workflow or MCP tool).

```csharp
builder.Object<Position>(obj =>
{
    // Workflow-backed action (durable, multi-step, saga state)
    obj.Action("ExecuteTrade")
        .Description("Open a new position via the trade execution workflow")
        .Accepts<TradeExecutionRequest>()
        .Returns<TradeExecutionResult>()
        .BoundToWorkflow("execute-trade");

    // Tool-backed action (immediate, single-step)
    obj.Action("GetQuote")
        .Description("Fetch current market quote for this position's symbol")
        .Accepts<QuoteRequest>()
        .Returns<Quote>()
        .BoundToTool("MarketDataMcpTools", "GetQuoteAsync");
});
```

**Unbound actions** (declared in ontology, implementation deferred):

```csharp
obj.Action("Hedge")
    .Description("Hedge this position against adverse movement")
    .Accepts<HedgeRequest>()
    .Returns<HedgeResult>();
    // No binding — implementation registered separately
```

This supports the case where the ontology is defined in a library but implementations are provided by the consuming application.

### 5.5 Interfaces — Polymorphic Object Types

Interfaces define shared shapes across object types, enabling cross-domain polymorphic queries. They map directly to C# interfaces.

```csharp
protected override void Define(IOntologyBuilder builder)
{
    // Declare an ontology interface backed by a C# interface
    builder.Interface<ISearchable>("Searchable", iface =>
    {
        iface.Property(s => s.Title);
        iface.Property(s => s.Description);
        iface.Property(s => s.Embedding);
    });
}
```

Object types declare interface implementations with optional property mapping:

```csharp
// In TradingOntology
builder.Object<Position>(obj =>
{
    obj.Implements<ISearchable>(map =>
    {
        map.Via(p => p.Symbol, s => s.Title);
        map.Via(p => p.DisplayDescription, s => s.Description);
        map.Via(p => p.SearchEmbedding, s => s.Embedding);
    });
});

// In KnowledgeOntology
builder.Object<AtomicNote>(obj =>
{
    obj.Implements<ISearchable>(map =>
    {
        map.Via(n => n.Title, s => s.Title);
        map.Via(n => n.Definition, s => s.Description);
        map.Via(n => n.Embedding, s => s.Embedding);
    });
});
```

**Agent query enabled:** "Find all `Searchable` objects matching 'machine learning'" returns results from any domain — `Position`, `AtomicNote`, `StyleCard` — transparently.

### 5.6 Cross-Domain Links

Domains are independently compiled. Cross-domain relationships are declared by the originating domain using string-based external references, then resolved at composition time.

```csharp
// In KnowledgeOntology
protected override void Define(IOntologyBuilder builder)
{
    builder.Object<AtomicNote>(obj => { /* ... */ });

    // Cross-domain: knowledge informs trading strategies
    builder.CrossDomainLink("KnowledgeInformsStrategy")
        .From<AtomicNote>()
        .ToExternal("trading", "Strategy")
        .ManyToMany()
        .WithEdge(edge =>
        {
            edge.Property<double>("Relevance");
            edge.Property<string>("Rationale");
        });
}
```

**Resolution:** The source generator in the composing assembly (e.g., `Basileus.AppHost`) validates that `"trading"."Strategy"` exists and the link is structurally sound. Unresolvable external references produce compile errors.

### 5.7 Workflow Integration (Optional Extension)

When both `Agentic.Ontology` and `Agentic.Workflow` are referenced, workflows can declare their ontological context:

```csharp
// In the workflow definition (existing Agentic.Workflow DSL)
var workflow = Workflow<TradeExecutionState>
    .Create("execute-trade")
    .Consumes<Position>()               // Ontology input type
    .Produces<TradeOrder>()             // Ontology output type
    .StartWith<ValidateOrder>()
    .Then<RouteToExchange>()
    .Then<ConfirmExecution>()
    .Finally<UpdatePosition>();
```

`Consumes<T>()` and `Produces<T>()` are extension methods from `Agentic.Ontology`. They:

1. Register the workflow as an action on the consumed type (auto-binding)
2. Enable **workflow chaining inference** — the source generator validates that Workflow A's `Produces<T>` is type-compatible with Workflow B's `Consumes<T>`
3. Give agents a typed dependency graph: "To get a `TradeOrder`, I need to run `execute-trade`, which consumes a `Position`"

---

## 6. Source Generator Pipeline

### 6.1 Per-Domain Generation (Domain Assembly)

When a domain assembly references `Agentic.Ontology.Generators`, the incremental source generator:

1. **Discovers** `DomainOntology` subclasses via `INamedTypeSymbol`
2. **Parses** the `Define()` method body as a Roslyn syntax tree
3. **Extracts** builder method calls: `Object<T>()`, `Property()`, `HasMany<T>()`, `Action()`, etc.
4. **Resolves** expression trees to member symbols (compile-time type checking)
5. **Emits** per-domain artifacts:

```csharp
// Generated: TradingOntologyDescriptor.g.cs
[assembly: OntologyDomain("trading", typeof(TradingOntologyDescriptor))]

public static class TradingOntologyDescriptor
{
    public static DomainDescriptor Descriptor { get; } = new DomainDescriptor
    {
        DomainName = "trading",
        ObjectTypes = ImmutableArray.Create(
            PositionDescriptor.Descriptor,
            TradeOrderDescriptor.Descriptor,
            StrategyDescriptor.Descriptor),
        CrossDomainLinks = ImmutableArray.Create(/* ... */),
        Interfaces = ImmutableArray.Create(/* ... */)
    };
}

// Generated: PositionDescriptor.g.cs
public static class PositionDescriptor
{
    public static ObjectTypeDescriptor Descriptor { get; } = new ObjectTypeDescriptor
    {
        Name = "Position",
        DomainName = "trading",
        ClrType = typeof(Position),
        KeyProperty = "Id",
        Properties = ImmutableArray.Create(
            new PropertyDescriptor("Symbol", typeof(string), required: true),
            new PropertyDescriptor("Quantity", typeof(decimal), required: false),
            new PropertyDescriptor("UnrealizedPnL", typeof(decimal), computed: true)),
        Links = ImmutableArray.Create(
            new LinkDescriptor("Orders", typeof(TradeOrder), LinkCardinality.OneToMany),
            new LinkDescriptor("Strategy", typeof(Strategy), LinkCardinality.ManyToOne)),
        Actions = ImmutableArray.Create(
            new ActionDescriptor("ExecuteTrade",
                accepts: typeof(TradeExecutionRequest),
                returns: typeof(TradeExecutionResult),
                binding: ActionBinding.Workflow("execute-trade")),
            new ActionDescriptor("GetQuote",
                accepts: typeof(QuoteRequest),
                returns: typeof(Quote),
                binding: ActionBinding.Tool("MarketDataMcpTools", "GetQuoteAsync"))),
        Interfaces = ImmutableArray.Create("Searchable")
    };
}
```

### 6.2 Host Composition Generation (AppHost Assembly)

The composing assembly (e.g., `Basileus.AppHost`) references all domain assemblies. Its source generator:

1. **Discovers** all `[assembly: OntologyDomain]` attributes from referenced assemblies
2. **Merges** domain descriptors into a unified graph
3. **Resolves** cross-domain links (validates external references exist)
4. **Validates** interface implementations (all mapped properties exist on source types)
5. **Validates** workflow chaining (`Produces<T>` → `Consumes<T>` compatibility)
6. **Emits** composed artifacts:

```csharp
// Generated: ComposedOntology.g.cs
public static class ComposedOntology
{
    public static OntologyGraph Graph { get; } = OntologyGraph.Compose(
        TradingOntologyDescriptor.Descriptor,
        KnowledgeOntologyDescriptor.Descriptor,
        StyleEngineOntologyDescriptor.Descriptor);

    // Compile-time validated cross-domain links
    public static ImmutableArray<ResolvedCrossDomainLink> CrossDomainLinks { get; } =
        ImmutableArray.Create(
            new ResolvedCrossDomainLink(
                name: "KnowledgeInformsStrategy",
                from: KnowledgeOntologyDescriptor.AtomicNoteDescriptor,
                to: TradingOntologyDescriptor.StrategyDescriptor,
                cardinality: LinkCardinality.ManyToMany));

    // Workflow chain graph
    public static ImmutableArray<WorkflowChain> WorkflowChains { get; } =
        ImmutableArray.Create(
            new WorkflowChain(
                producer: "ingest-knowledge",
                producesType: typeof(AtomicNote),
                consumer: "execute-trade",
                consumesType: typeof(Position),
                bridgedVia: "KnowledgeInformsStrategy"));
}
```

### 6.3 Compile-Time Diagnostics

The source generator emits diagnostics (errors/warnings) for:

| Code | Severity | Condition |
|------|----------|-----------|
| `ONTO001` | Error | Object type has no `Key()` declaration |
| `ONTO002` | Error | Property expression references non-existent member |
| `ONTO003` | Error | Cross-domain link references unknown domain or object |
| `ONTO004` | Warning | Object type has no actions (pure data, not actionable) |
| `ONTO005` | Error | Interface mapping references incompatible property types |
| `ONTO006` | Warning | Workflow `Produces<T>` has no matching `Consumes<T>` consumer |
| `ONTO007` | Error | Circular cross-domain link dependency |
| `ONTO008` | Warning | Action bound to workflow that doesn't exist in composition |

---

## 7. Progressive Disclosure Integration

### 7.1 Schema-Driven Stub Generation

Today, progressive disclosure generates Python wrapper functions from `[McpServerTool]` attributes. With the ontology, stubs are enriched with type context:

**Before (current):**
```python
# servers/trading.py
def open_position(symbol: str, quantity: float) -> dict:
    """Open a new trading position."""
    return call_mcp_tool("TradingMcpTools", "OpenPosition", symbol=symbol, quantity=quantity)
```

**After (ontology-enhanced):**
```python
# servers/trading/position.pyi
class Position:
    """Object Type: Position (domain: trading)

    Properties:
        symbol: str (required)
        quantity: float
        unrealized_pnl: float (computed)

    Links:
        orders -> TradeOrder[] (one-to-many)
        strategy -> Strategy (many-to-one)

    Actions:
        execute_trade(request: TradeExecutionRequest) -> TradeExecutionResult
        get_quote(request: QuoteRequest) -> Quote

    Interfaces: Searchable
    """
    symbol: str
    quantity: float
    unrealized_pnl: float

    def execute_trade(self, request: TradeExecutionRequest) -> TradeExecutionResult: ...
    def get_quote(self, request: QuoteRequest) -> Quote: ...
```

The stub generator reads `ObjectTypeDescriptor` at ControlPlane startup and produces richer stubs with:
- Type annotations matching ontology properties
- Methods for each declared action
- Docstrings describing links and interfaces
- Cross-references to related object types

### 7.2 Ontology-Aware Tool Discovery

Agents currently discover tools via `ls servers/`. With the ontology, a meta-tool enables semantic discovery:

```python
# Agent asks: "What can I do with a Position?"
result = ontology.query(object_type="Position", include=["actions", "links"])

# Returns structured response:
# {
#   "object": "Position",
#   "domain": "trading",
#   "actions": [
#     {"name": "ExecuteTrade", "accepts": "TradeExecutionRequest", "returns": "TradeExecutionResult"},
#     {"name": "GetQuote", "accepts": "QuoteRequest", "returns": "Quote"}
#   ],
#   "links": [
#     {"name": "Orders", "target": "TradeOrder", "cardinality": "one-to-many"},
#     {"name": "Strategy", "target": "Strategy", "cardinality": "many-to-one"}
#   ],
#   "implements": ["Searchable"]
# }
```

This transforms agent planning from "grep tool descriptions" to "query typed action graph" — directly reducing the CMDP action space per §2.3 of the agentic workflow theory.

---

## 8. Agent Query Interface

The ontology exposes an `IOntologyQuery` contract for agent-facing queries:

```csharp
public interface IOntologyQuery
{
    /// <summary>List all domains in the composed ontology.</summary>
    IReadOnlyList<DomainDescriptor> ListDomains();

    /// <summary>Get all object types in a domain.</summary>
    IReadOnlyList<ObjectTypeDescriptor> GetObjectTypes(string domain);

    /// <summary>Get full descriptor for a specific object type.</summary>
    ObjectTypeDescriptor? GetObjectType(string domain, string objectTypeName);

    /// <summary>Find all actions available on an object type.</summary>
    IReadOnlyList<ActionDescriptor> GetActions(string domain, string objectTypeName);

    /// <summary>Find all object types implementing an interface.</summary>
    IReadOnlyList<ObjectTypeDescriptor> GetImplementors(string interfaceName);

    /// <summary>Traverse the link graph from a given object type.</summary>
    IReadOnlyList<LinkTraversalResult> TraverseLinks(
        string domain, string objectTypeName, int maxDepth = 2);

    /// <summary>Find workflow chains: which workflows can produce the input for a target workflow.</summary>
    IReadOnlyList<WorkflowChain> FindWorkflowChains(string targetWorkflow);
}
```

**Implementation:** `OntologyQueryService` backed by the source-generated `ComposedOntology`. All queries resolve against compile-time descriptors — no runtime reflection.

**MCP exposure:** `Agentic.Ontology.MCP` provides `OntologyMcpTools` that wraps `IOntologyQuery` as MCP tools, exposable via the ControlPlane or AgentHost Workflow MCP Server.

---

## 9. Exarchos Integration

Exarchos (the SDLC orchestrator) consumes the ontology to plan multi-step operations:

1. **Workflow MCP Server** (already exists in AgentHost) gains ontology query tools alongside existing workflow event tools
2. Exarchos queries the ontology to understand available operations before dispatching work
3. Workflow chaining metadata enables Exarchos to plan operation sequences: "ingesting knowledge produces `AtomicNote`, which can inform `Strategy` via the `KnowledgeInformsStrategy` link"

The ontology functions as the "map" that Exarchos uses for strategic planning, while individual agents use it tactically during execution.

---

## 10. Basileus Adoption Strategy

### What Changes in This Repo

1. **ADR update:** Add Ontology Layer section to `docs/adrs/platform-architecture.md`
2. **Domain ontology classes:** Each domain assembly adds a `DomainOntology` subclass:
   - `domains/trading/` → `TradingOntology`
   - `domains/knowledge/` → `KnowledgeOntology`
   - `domains/style-engine/` → `StyleEngineOntology`
3. **Package references:** Domain assemblies reference `Agentic.Ontology` + `Agentic.Ontology.Generators`
4. **AppHost composition:** Register all domain ontologies for cross-domain resolution
5. **ControlPlane enhancement:** Progressive disclosure uses ontology descriptors for richer stubs
6. **`ISearchable` interface:** Shared interface in `Basileus.Core` implemented by key domain types

### What Does NOT Change

- Domain types (Position, AtomicNote, StyleCard) — unchanged, ontology maps them
- Existing MCP tools — unchanged, ontology binds to them
- Existing workflows — unchanged, optional `Consumes`/`Produces` added incrementally
- Domain independence — preserved, cross-domain links resolved at composition

---

## 11. Illustrative Example: Full Domain Ontology

Complete `KnowledgeOntology` showing all DSL features together:

```csharp
namespace Basileus.Knowledge;

public sealed class KnowledgeOntology : DomainOntology
{
    public override string DomainName => "knowledge";

    protected override void Define(IOntologyBuilder builder)
    {
        // -- Interfaces --

        builder.Interface<ISearchable>("Searchable", iface =>
        {
            iface.Property(s => s.Title);
            iface.Property(s => s.Description);
            iface.Property(s => s.Embedding);
        });

        // -- Object Types --

        builder.Object<AtomicNote>(obj =>
        {
            obj.Key(n => n.Id);
            obj.Property(n => n.CanonicalName).Required();
            obj.Property(n => n.Title).Required();
            obj.Property(n => n.Definition).Required();
            obj.Property(n => n.Category);
            obj.Property(n => n.Context);
            obj.Property(n => n.CreatedAt);
            obj.Property(n => n.ModifiedAt);

            obj.ManyToMany<AtomicNote>("SemanticLinks")
                .WithEdge<KnowledgeLink>(edge =>
                {
                    edge.MapProperty(l => l.Type);
                    edge.MapProperty(l => l.Confidence);
                    edge.MapProperty(l => l.ContextDescription);
                });

            obj.HasMany<SourceReference>("Sources");

            obj.Action("Ingest")
                .Description("Ingest a source document into the knowledge graph")
                .Accepts<IngestRequest>()
                .Returns<IngestionResult>()
                .BoundToWorkflow("ingest-knowledge");

            obj.Action("Query")
                .Description("Query the knowledge graph for relevant concepts")
                .Accepts<KnowledgeQueryRequest>()
                .Returns<KnowledgeQueryResult>()
                .BoundToWorkflow("query-knowledge");

            obj.Implements<ISearchable>(map =>
            {
                map.Via(n => n.Title, s => s.Title);
                map.Via(n => n.Definition, s => s.Description);
                map.Via(n => n.Embedding, s => s.Embedding);
            });
        });

        builder.Object<SourceReference>(obj =>
        {
            obj.Key(s => s.Title);  // Natural key
            obj.Property(s => s.Author);
            obj.Property(s => s.Uri);
            obj.Property(s => s.RetrievedAt);
        });

        // -- Cross-Domain Links --

        builder.CrossDomainLink("KnowledgeInformsStrategy")
            .From<AtomicNote>()
            .ToExternal("trading", "Strategy")
            .ManyToMany()
            .WithEdge(edge =>
            {
                edge.Property<double>("Relevance");
                edge.Property<string>("Rationale");
            });

        builder.CrossDomainLink("KnowledgeInformsStyle")
            .From<AtomicNote>()
            .ToExternal("style-engine", "StyleCard")
            .ManyToMany()
            .WithEdge(edge =>
            {
                edge.Property<double>("Relevance");
            });
    }
}
```

---

## 12. Future Considerations

- **Object-level security policies:** Per-object type permission declarations in the ontology, enforced by ControlPlane policy engine
- **Ontology versioning:** Schema evolution with backward compatibility guarantees (additive changes safe, breaking changes produce compile errors)
- **Runtime object resolution:** Given an ontology object descriptor, resolve actual instances from domain persistence (Marten, pgvector, etc.)
- **Ontology visualization:** Generate Mermaid/D3 diagrams from the compiled ontology graph
- **Cost profiles on actions:** Budget metadata per action for scarcity-aware agent planning (§4.3 of workflow theory)
- **Event typing:** Marten events annotated with ontology types for richer audit trails
