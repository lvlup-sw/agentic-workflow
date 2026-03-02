# Refactor Plan: Ontology Spec Alignment

> **Spec:** `docs/reference/platform-architecture.md` §4.14 (Feb 27, 2026)
> **Branch:** `feat/ontology-layer-v2`
> **Workflow:** `refactor-ontology-spec-alignment`

## Summary

Close 5 implementation gaps between the ontology layer and the platform architecture spec. The source generator architecture gap (DiagnosticAnalyzer vs IIncrementalGenerator) is **deferred** — this plan covers the remaining gaps.

## Parallelization Groups

```text
Group A ─── Tasks 001-003  (Core missing diagnostics: AONT002, AONT005, AONT008)
Group B ─── Task 004       (Precondition overlap diagnostic: AONT013)
Group C ─── Tasks 005-006  (Lifecycle diagnostics: AONT019-021)
Group D ─── Tasks 007-008  (Derivation diagnostics: AONT024-025)
Group E ─── Tasks 009-010  (Interface action diagnostics: AONT029-030)
Group F ─── Tasks 011-013  (Extension point diagnostics: AONT031-035)
Group G ─── Task 014       (Backfill tests for wired diagnostics: AONT003,011,012,014,017,018,022,027,028)
Group H ─── Task 015       (Precondition evaluation: OntologyQueryService)
Group I ─── Task 016       (BoundToTool expression overload)
Group J ─── Task 017       (Action tool filter fix)
Group K ─── Task 018       (Design doc update)

A-G parallel (all analyzer work)  ──┐
H parallel (Ontology.Query)        ├── all independent
I parallel (Ontology.Builder)      │
J parallel (Ontology.MCP)         ─┘
K sequential after all ────────────── last
```

---

## Tasks

### Task 001: Wire AONT002 — InvalidPropertyExpression
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group A)

1. [RED] Write tests in `Analyzers/CoreDiagnosticTests.cs`:
   - `AONT002_PropertyExpressionNotSimpleMember_ReportsError` — `obj.Property(p => p.ToString())` triggers
   - `AONT002_SimpleMemberAccess_NoDiagnostic` — `obj.Property(p => p.Name)` passes
   - Expected failure: no diagnostic reported (AONT002 not in SupportedDiagnostics)

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add `OntologyDiagnostics.InvalidPropertyExpression` to `SupportedDiagnostics`
   - In `CollectObjectTypeInfo`, for `case "Property"`: validate the expression argument is a simple member access lambda (`p => p.Member`). If it's a method call, conversion, or complex expression, record it in a new `InvalidPropertyExpressions` collection on `ObjectTypeInfo`
   - In `ReportDiagnostics`: report AONT002 for each invalid expression

3. [REFACTOR] Extract expression validation helper

**Files:** `OntologyDefinitionAnalyzer.cs`, `CoreDiagnosticTests.cs`
**Dependencies:** None

---

### Task 002: Wire AONT005 — InterfaceMappingBadProperty
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group A)

1. [RED] Write tests in `Analyzers/CoreDiagnosticTests.cs`:
   - `AONT005_InterfaceMappingNonexistentProperty_ReportsError` — `map.Via(p => p.Missing, s => s.Title)` triggers
   - `AONT005_ValidMapping_NoDiagnostic` — valid property mapping passes
   - Expected failure: AONT005 not wired

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add `OntologyDiagnostics.InterfaceMappingBadProperty` to `SupportedDiagnostics`
   - In `CollectImplementsMappingInfo`: record property names referenced in `Via()` calls
   - In `ReportDiagnostics`: for each `Implements<T>` mapping, verify referenced properties exist on the object type

3. [REFACTOR] Clean up

**Files:** `OntologyDefinitionAnalyzer.cs`, `CoreDiagnosticTests.cs`
**Dependencies:** None

---

### Task 003: Wire AONT008 — EdgeTypeMissingProperty
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group A)

1. [RED] Write tests in `Analyzers/CoreDiagnosticTests.cs`:
   - `AONT008_ManyToManyEdgeNoProperties_ReportsError` — `obj.ManyToMany<T>("Link", edge => { })` triggers
   - `AONT008_EdgeWithProperties_NoDiagnostic` — `edge.Property<string>("Type")` passes
   - Expected failure: AONT008 not wired

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add `OntologyDiagnostics.EdgeTypeMissingProperty` to `SupportedDiagnostics`
   - In `CollectObjectTypeInfo`, for `case "ManyToMany"`: track whether edge config lambda has any `Property<>` calls. Record edges with empty property sets
   - In `ReportDiagnostics`: report AONT008 for edges with no properties

3. [REFACTOR] Clean up

**Files:** `OntologyDefinitionAnalyzer.cs`, `CoreDiagnosticTests.cs`
**Dependencies:** None

---

### Task 004: Wire AONT013 — PostconditionOverlapsEvent
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group B)

1. [RED] Write tests in `Analyzers/PreconditionDiagnosticTests.cs`:
   - `AONT013_ModifiesPropertyAlsoUpdatedByEvent_ReportsWarning` — action `.Modifies(p => p.PnL)` + event `UpdatesProperty(p => p.PnL, ...)` triggers
   - `AONT013_NoOverlap_NoDiagnostic` — different properties, no overlap
   - Expected failure: AONT013 not wired

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add `OntologyDiagnostics.PostconditionOverlapsEvent` to `SupportedDiagnostics`
   - In `CollectObjectTypeInfo`: collect event `UpdatesProperty` targets (new `EventUpdatedProperties` collection)
   - In `ReportDiagnostics`: cross-reference `ActionModifiesProperties` with `EventUpdatedProperties` — warn on overlap

3. [REFACTOR] Clean up

**Files:** `OntologyDefinitionAnalyzer.cs`, `PreconditionDiagnosticTests.cs`
**Dependencies:** None

---

### Task 005: Wire AONT019 — LifecycleTransitionBadEvent
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group C)

1. [RED] Write tests in `Analyzers/LifecycleDiagnosticTests.cs`:
   - `AONT019_TriggeredByEventUndeclared_ReportsWarning` — `transition.TriggeredByEvent<UndeclaredEvent>()` triggers
   - `AONT019_TriggeredByDeclaredEvent_NoDiagnostic` — event is declared on same object type
   - Expected failure: AONT019 not wired

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add `OntologyDiagnostics.LifecycleTransitionBadEvent` to `SupportedDiagnostics`
   - In `CollectLifecycleInfo`: record `TriggeredByEvent<T>` type names (new `LifecycleTransitionEvents` list on `ObjectTypeInfo`)
   - In `ReportDiagnostics`: verify each event type is in `DeclaredEvents`

3. [REFACTOR] Clean up

**Files:** `OntologyDefinitionAnalyzer.cs`, `LifecycleDiagnosticTests.cs`
**Dependencies:** None

---

### Task 006: Wire AONT020/021 — LifecycleUnreachableState and DeadEndState
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group C)

1. [RED] Write tests in `Analyzers/LifecycleDiagnosticTests.cs`:
   - `AONT020_UnreachableState_ReportsWarning` — state not target of any transition and not Initial
   - `AONT020_AllStatesReachable_NoDiagnostic` — all states reachable
   - `AONT021_NonTerminalDeadEnd_ReportsWarning` — non-Terminal state with no outgoing transitions
   - `AONT021_TerminalDeadEnd_NoDiagnostic` — Terminal states are allowed dead-ends
   - Expected failure: neither diagnostic wired

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add both to `SupportedDiagnostics`
   - In `ReportDiagnostics` after lifecycle validation: compute reachable states (states that are Initial or appear as `ToState` in any transition). Non-reachable → AONT020. Compute states with no outgoing transitions and not Terminal → AONT021

3. [REFACTOR] Extract graph reachability helper

**Files:** `OntologyDefinitionAnalyzer.cs`, `LifecycleDiagnosticTests.cs`
**Dependencies:** None

---

### Task 007: Wire AONT024 — DerivationCycle
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group D)

1. [RED] Write tests in `Analyzers/DerivationDiagnosticTests.cs`:
   - `AONT024_DerivationCycle_ReportsError` — `A.DerivedFrom(B)`, `B.DerivedFrom(A)` triggers
   - `AONT024_NoCycle_NoDiagnostic` — acyclic chain passes
   - Expected failure: AONT024 not wired

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add `OntologyDiagnostics.DerivationCycle` to `SupportedDiagnostics`
   - In `ReportDiagnostics`: build local derivation graph from `DerivedFromReferences`, run DFS cycle detection. Report AONT024 for each cycle participant

3. [REFACTOR] Extract cycle detection into helper

**Files:** `OntologyDefinitionAnalyzer.cs`, `DerivationDiagnosticTests.cs`
**Dependencies:** None

---

### Task 008: Wire AONT025 — DerivedFromExternalUnresolvable
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group D)

1. [RED] Write tests in `Analyzers/DerivationDiagnosticTests.cs`:
   - `AONT025_DerivedFromExternal_ReportsWarning` — `DerivedFromExternal("other", "Type", "Prop")` always warns (cross-assembly unverifiable)
   - `AONT025_LocalDerivedFrom_NoDiagnostic` — local derivation doesn't trigger
   - Expected failure: AONT025 not wired

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add `OntologyDiagnostics.DerivedFromExternalUnresolvable` to `SupportedDiagnostics`
   - In `CollectObjectTypeInfo`, for `case "DerivedFromExternal"` (or similar): record external derivation references
   - In `ReportDiagnostics`: warn for each external derivation (analogous to AONT007 for cross-domain links)

3. [REFACTOR] Clean up

**Files:** `OntologyDefinitionAnalyzer.cs`, `DerivationDiagnosticTests.cs`
**Dependencies:** None

---

### Task 009: Wire AONT029 — InterfaceActionIncompatible
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group E)

1. [RED] Write tests in new `Analyzers/InterfaceActionDiagnosticTests.cs`:
   - `AONT029_IncompatibleAcceptsType_ReportsError` — interface action `Accepts<SearchRequest>`, concrete action `Accepts<TradeRequest>` triggers
   - `AONT029_CompatibleTypes_NoDiagnostic` — matching Accepts types passes
   - Expected failure: AONT029 not wired

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add `OntologyDiagnostics.InterfaceActionIncompatible` to `SupportedDiagnostics`
   - In `CollectInterfaceInfo`: record Accepts/Returns type names on interface actions
   - In `CollectObjectTypeInfo`: record Accepts/Returns type names on concrete actions
   - In `ReportDiagnostics` where AONT027/028 are checked: also compare Accepts/Returns types of mapped interface action vs concrete action

3. [REFACTOR] Clean up

**Files:** `OntologyDefinitionAnalyzer.cs`, new `InterfaceActionDiagnosticTests.cs`
**Dependencies:** None

---

### Task 010: Wire AONT030 — InterfaceActionNoImplementors
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group E)

1. [RED] Write tests in `Analyzers/InterfaceActionDiagnosticTests.cs`:
   - `AONT030_InterfaceWithActionsNoImplementors_ReportsWarning` — interface has `Action("Search")` but no object types `Implements<T>`
   - `AONT030_InterfaceWithImplementors_NoDiagnostic` — at least one implementor exists
   - Expected failure: AONT030 not wired

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add `OntologyDiagnostics.InterfaceActionNoImplementors` to `SupportedDiagnostics`
   - In `ReportDiagnostics`: for each interface with actions, check if any object type implements it. Warn if none

3. [REFACTOR] Clean up

**Files:** `OntologyDefinitionAnalyzer.cs`, `InterfaceActionDiagnosticTests.cs`
**Dependencies:** None

---

### Task 011: Wire AONT031 — CrossDomainLinkNoExtensionPoint
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group F)

1. [RED] Write tests in new `Analyzers/ExtensionPointDiagnosticTests.cs`:
   - `AONT031_CrossDomainLinkNoExtensionPoint_ReportsWarning` — link targets type that has no `AcceptsExternalLinks`
   - `AONT031_NoLocalCrossDomainLinks_NoDiagnostic` — no links to check
   - Expected failure: AONT031 not wired

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add `OntologyDiagnostics.CrossDomainLinkNoExtensionPoint` to `SupportedDiagnostics`
   - Note: cross-domain link targets are external (can't verify locally). This diagnostic applies when the link's `From<T>` type is local and the target could have extension points declared. Since we can't verify cross-assembly, emit as informational warning (like AONT007)

3. [REFACTOR] Clean up

**Files:** `OntologyDefinitionAnalyzer.cs`, new `ExtensionPointDiagnosticTests.cs`
**Dependencies:** None

---

### Task 012: Wire AONT032/033 — ExtensionPoint constraint validation
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group F)

1. [RED] Write tests in `Analyzers/ExtensionPointDiagnosticTests.cs`:
   - `AONT032_ExtensionPointInterfaceUnsatisfied_ReportsWarning` — extension point requires `FromInterface<ISearchable>` but incoming link source doesn't implement it (local verification only)
   - `AONT033_ExtensionPointEdgeMissing_ReportsError` — extension point requires edge property "Relevance" but link edge doesn't declare it
   - `AONT032_SatisfiedConstraint_NoDiagnostic`
   - `AONT033_EdgePresent_NoDiagnostic`
   - Expected failure: neither diagnostic wired

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add both to `SupportedDiagnostics`
   - In `CollectObjectTypeInfo`: collect `AcceptsExternalLinks` declarations with their constraints (interface, edge properties)
   - Note: since cross-domain links reference external types, full validation happens at runtime. These diagnostics only fire for same-assembly links that can be verified locally

3. [REFACTOR] Clean up

**Files:** `OntologyDefinitionAnalyzer.cs`, `ExtensionPointDiagnosticTests.cs`
**Dependencies:** None

---

### Task 013: Wire AONT034/035 — ExtensionPoint informational diagnostics
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group F)

1. [RED] Write tests in `Analyzers/ExtensionPointDiagnosticTests.cs`:
   - `AONT034_ExtensionPointNoLinksMatch_ReportsInfo` — extension point declared but no cross-domain links target this type
   - `AONT035_MaxLinksExceeded_ReportsWarning` — more cross-domain links than `MaxLinks` constraint (same-assembly only)
   - `AONT034_LinksMatch_NoDiagnostic`
   - `AONT035_WithinMaxLinks_NoDiagnostic`
   - Expected failure: neither wired

2. [GREEN] In `OntologyDefinitionAnalyzer.cs`:
   - Add both to `SupportedDiagnostics`
   - In `ReportDiagnostics`: count cross-domain links per target type and compare against extension point `MaxLinks`

3. [REFACTOR] Clean up

**Files:** `OntologyDefinitionAnalyzer.cs`, `ExtensionPointDiagnosticTests.cs`
**Dependencies:** None

---

### Task 014: Backfill tests for already-wired diagnostics
**Phase:** RED → GREEN (tests against existing implementation)
**Parallelizable:** Yes (Group G)

9 already-wired diagnostics lack test coverage. Write positive and negative test for each:

1. [RED/GREEN] Tests in appropriate test files:
   - `AONT003_LinkTargetNotRegistered_ReportsError` + `_Registered_NoDiagnostic` (CoreDiagnosticTests)
   - `AONT011_CreatesLinkedUndeclaredLink_ReportsError` + `_DeclaredLink_NoDiagnostic` (PreconditionDiagnosticTests)
   - `AONT012_RequiresLinkUndeclared_ReportsWarning` + `_DeclaredLink_NoDiagnostic` (PreconditionDiagnosticTests)
   - `AONT014_LifecyclePropertyUndeclared_ReportsError` + `_Declared_NoDiagnostic` (LifecycleDiagnosticTests)
   - `AONT017_TransitionBadState_ReportsError` + `_ValidState_NoDiagnostic` (LifecycleDiagnosticTests)
   - `AONT018_TransitionBadAction_ReportsWarning` + `_ValidAction_NoDiagnostic` (LifecycleDiagnosticTests)
   - `AONT022_DerivedFromUndeclaredProperty_ReportsError` + `_DeclaredProperty_NoDiagnostic` (DerivationDiagnosticTests)
   - `AONT027_InterfaceActionUnmapped_ReportsError` + `_Mapped_NoDiagnostic` (new InterfaceActionDiagnosticTests)
   - `AONT028_ActionViaBadReference_ReportsError` + `_ValidReference_NoDiagnostic` (InterfaceActionDiagnosticTests)

   Since the analyzer already reports these, tests should pass on GREEN immediately.

**Files:** `CoreDiagnosticTests.cs`, `PreconditionDiagnosticTests.cs`, `LifecycleDiagnosticTests.cs`, `DerivationDiagnosticTests.cs`, new `InterfaceActionDiagnosticTests.cs`
**Dependencies:** None (but logically paired with Tasks 009-010 for InterfaceAction tests)

---

### Task 015: Implement IsPreconditionSatisfiable evaluation
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group H)

1. [RED] Write tests in `Query/OntologyQueryTests.cs`:
   - `GetValidActions_PropertyPredicateUnsat_FiltersAction` — action with `Requires(p => p.Status == Active)` filtered out when `knownProperties` has `Status = Closed`
   - `GetValidActions_PropertyPredicateSat_IncludesAction` — passes when `Status = Active`
   - `GetValidActions_LinkExistsUnsat_FiltersAction` — action with `RequiresLink("Strategy")` filtered when link info absent
   - `GetValidActions_LinkExistsSat_IncludesAction` — passes when link info present
   - `GetValidActions_NoPreconditions_IncludesAll` — actions without preconditions always included
   - Expected failure: all return all actions (stub returns true)

2. [GREEN] In `OntologyQueryService.IsPreconditionSatisfiable()`:
   - For `PreconditionKind.PropertyPredicate`: parse the `Expression` string to extract property name. Check if `knownProperties` contains the property. If the property is known, attempt simple equality/comparison evaluation against the stored expression description. If the expression can't be evaluated, default to satisfiable (optimistic)
   - For `PreconditionKind.LinkExists`: check if `knownProperties` contains a key matching the `LinkName` (convention: link existence represented as a boolean property)
   - For `PreconditionKind.Custom`: return true (cannot evaluate)

3. [REFACTOR] Extract precondition evaluator into dedicated class

**Files:** `OntologyQueryService.cs`, `OntologyQueryTests.cs`
**Dependencies:** None

---

### Task 016: Add expression-based BoundToTool overload
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group I)

1. [RED] Write tests in `Builder/ActionBuilderOfTTests.cs`:
   - `BoundToTool_Expression_SetsToolNameAndMethod` — `action.BoundToTool<TradingMcpTools>(t => t.GetQuoteAsync)` sets ToolName to type name and MethodName to method name
   - `BoundToTool_Expression_SetsBindingTypeToTool` — binding type is `ActionBindingType.Tool`
   - Expected failure: method doesn't exist

2. [GREEN] In `IActionBuilder.cs` and `IActionBuilderOfT.cs`:
   - Add `IActionBuilder<T> BoundToTool<TTool>(Expression<Func<TTool, Delegate>> methodSelector)` overload
   - In `ActionBuilderOfT.cs`: implement by extracting type name from `typeof(TTool).Name` and method name from the expression body
   - Keep existing `BoundToTool(string, string)` for backward compatibility

3. [REFACTOR] Share extraction logic with `ExpressionHelper`

**Files:** `IActionBuilder.cs`, `IActionBuilderOfT.cs`, `ActionBuilder.cs`, `ActionBuilderOfT.cs`, `ActionBuilderOfTTests.cs`
**Dependencies:** None

---

### Task 017: Fix OntologyActionTool batch dispatch filter
**Phase:** RED → GREEN → REFACTOR
**Parallelizable:** Yes (Group J)

1. [RED] Write test in `OntologyActionToolTests.cs`:
   - `ExecuteAsync_BatchWithFilter_PassesFilterToObjectSet` — verify that when `filter` is provided and `objectId` is null, the filter string is passed to the ObjectSet expression (not just `RootExpression`)
   - Expected failure: batch dispatch uses `new RootExpression(typeof(object))` ignoring filter

2. [GREEN] In `OntologyActionTool.DispatchBatchAsync()`:
   - Accept `filter` parameter
   - Parse filter string into a `FilterExpression` wrapping the `RootExpression`
   - Pass the composed expression to `_objectSetProvider.ExecuteAsync`

3. [REFACTOR] Extract filter parsing

**Files:** `OntologyActionTool.cs`, `OntologyActionToolTests.cs`
**Dependencies:** None

---

### Task 018: Update design doc with spec authority note
**Phase:** Direct edit (no TDD needed for docs)
**Parallelizable:** Yes (Group K, but run last)

1. Add a note at top of `docs/designs/2026-02-24-ontology-layer.md`:
   - `> **Note:** The authoritative specification for the ontology layer is `docs/reference/platform-architecture.md` §4.14. This design document captures the initial runtime-first proposal (Revision 2). The platform architecture doc incorporates additional schema refinements (preconditions, lifecycles, derivation chains, interface actions, extension points) and specifies a compile-time source generation architecture as the target. The current implementation follows a hybrid approach: runtime-first architecture with the full schema refinement vocabulary. Migration to compile-time source generation is planned as a separate effort.`

**Files:** `docs/designs/2026-02-24-ontology-layer.md`
**Dependencies:** None (but run after implementation tasks for clean commit history)

---

## Dependency Graph

```text
[001] AONT002 ─────────────────────────┐
[002] AONT005 ─────────────────────────┤
[003] AONT008 ─────────────────────────┤
[004] AONT013 ─────────────────────────┤
[005] AONT019 ─────────────────────────┤
[006] AONT020/021 ─────────────────────┤
[007] AONT024 ─────────────────────────┤  All parallel
[008] AONT025 ─────────────────────────┤
[009] AONT029 ─────────────────────────┤
[010] AONT030 ─────────────────────────┤
[011] AONT031 ─────────────────────────┤
[012] AONT032/033 ─────────────────────┤
[013] AONT034/035 ─────────────────────┤
[014] Backfill tests ─────────────────┤
[015] Precondition eval ──────────────┤
[016] BoundToTool expr ───────────────┤
[017] Action tool filter ─────────────┘
                                       ↓
[018] Design doc update ──────────── sequential (last)
```

## Task Summary

| ID | Title | Package | Group | Diagnostics |
|----|-------|---------|-------|-------------|
| 001 | Wire AONT002 InvalidPropertyExpression | Generators | A | AONT002 |
| 002 | Wire AONT005 InterfaceMappingBadProperty | Generators | A | AONT005 |
| 003 | Wire AONT008 EdgeTypeMissingProperty | Generators | A | AONT008 |
| 004 | Wire AONT013 PostconditionOverlapsEvent | Generators | B | AONT013 |
| 005 | Wire AONT019 LifecycleTransitionBadEvent | Generators | C | AONT019 |
| 006 | Wire AONT020/021 UnreachableState + DeadEndState | Generators | C | AONT020, AONT021 |
| 007 | Wire AONT024 DerivationCycle | Generators | D | AONT024 |
| 008 | Wire AONT025 DerivedFromExternalUnresolvable | Generators | D | AONT025 |
| 009 | Wire AONT029 InterfaceActionIncompatible | Generators | E | AONT029 |
| 010 | Wire AONT030 InterfaceActionNoImplementors | Generators | E | AONT030 |
| 011 | Wire AONT031 CrossDomainLinkNoExtensionPoint | Generators | F | AONT031 |
| 012 | Wire AONT032/033 ExtensionPoint constraints | Generators | F | AONT032, AONT033 |
| 013 | Wire AONT034/035 ExtensionPoint informational | Generators | F | AONT034, AONT035 |
| 014 | Backfill tests for 9 wired diagnostics | Generators.Tests | G | AONT003,011,012,014,017,018,022,027,028 |
| 015 | Implement precondition evaluation | Ontology | H | — |
| 016 | Add BoundToTool expression overload | Ontology | I | — |
| 017 | Fix action tool batch filter | Ontology.MCP | J | — |
| 018 | Update design doc authority note | docs | K | — |
