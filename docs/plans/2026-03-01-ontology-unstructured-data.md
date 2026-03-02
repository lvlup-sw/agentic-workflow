# Implementation Plan: First-Class Unstructured Data Support

**Design:** `docs/designs/2026-03-01-ontology-unstructured-data.md`
**Branch:** `feat/ontology-unstructured-data`
**Iron Law:** No production code without a failing test first.

---

## Phase 1: Core Abstractions (Strategos.Ontology)

### Task 001: IEmbeddingProvider interface
**DR:** DR-1 | **Dependencies:** None | **Parallelizable:** Yes

1. **[RED]** Write tests:
   - File: `src/Strategos.Ontology.Tests/Embeddings/IEmbeddingProviderTests.cs`
   - `EmbedAsync_ReturnsFloatArray_ImplementationCanBeCalled` — verify stub implementing interface compiles and returns `float[]`
   - `EmbedBatchAsync_ReturnsReadOnlyList_ImplementationCanBeCalled` — verify batch returns `IReadOnlyList<float[]>`
   - `Dimensions_ReturnsConfiguredValue` — verify `Dimensions` property returns expected int
   - Expected failure: `IEmbeddingProvider` does not exist

2. **[GREEN]** Create interface:
   - File: `src/Strategos.Ontology/Embeddings/IEmbeddingProvider.cs`
   - `IEmbeddingProvider` with `Dimensions`, `EmbedAsync`, `EmbedBatchAsync`

---

### Task 002: ITextChunker, TextChunk, ChunkOptions
**DR:** DR-3 | **Dependencies:** None | **Parallelizable:** Yes

1. **[RED]** Write tests:
   - File: `src/Strategos.Ontology.Tests/Chunking/ChunkingTypesTests.cs`
   - `TextChunk_Constructor_SetsAllProperties` — verify record struct fields
   - `ChunkOptions_Defaults_MaxTokens512OverlapTokens64` — verify default values
   - `ChunkOptions_CustomValues_OverridesDefaults` — verify `init` setters
   - `ITextChunker_Chunk_ReturnsReadOnlyList` — verify stub compiles
   - Expected failure: types/interface do not exist

2. **[GREEN]** Create types:
   - File: `src/Strategos.Ontology/Chunking/TextChunk.cs`
   - File: `src/Strategos.Ontology/Chunking/ChunkOptions.cs`
   - File: `src/Strategos.Ontology/Chunking/ITextChunker.cs`

---

### Task 003: IObjectSetWriter interface
**DR:** DR-4 | **Dependencies:** None | **Parallelizable:** Yes

1. **[RED]** Write tests:
   - File: `src/Strategos.Ontology.Tests/ObjectSets/IObjectSetWriterTests.cs`
   - `StoreAsync_ImplementationCanBeCalled` — verify stub compiles and can be invoked
   - `StoreBatchAsync_ImplementationCanBeCalled` — verify batch method with `IReadOnlyList<T>`
   - Expected failure: `IObjectSetWriter` does not exist

2. **[GREEN]** Create interface:
   - File: `src/Strategos.Ontology/ObjectSets/IObjectSetWriter.cs`

---

### Task 004: OntologyOptions registration extensions
**DR:** DR-7 | **Dependencies:** 001, 003 | **Parallelizable:** No

1. **[RED]** Write tests:
   - File: `src/Strategos.Ontology.Tests/Configuration/EmbeddingProviderRegistrationTests.cs`
   - `UseEmbeddingProvider_RegistersSingleton` — resolve `IEmbeddingProvider` from DI after registration
   - `UseObjectSetWriter_RegistersSingleton` — resolve `IObjectSetWriter` from DI after registration
   - `UseObjectSetProvider_WhenAlsoWriter_RegistersBothInterfaces` — auto-detect dual-interface
   - Expected failure: methods do not exist on `OntologyOptions`

2. **[GREEN]** Edit:
   - File: `src/Strategos.Ontology/Configuration/OntologyOptions.cs` — add `UseEmbeddingProvider<T>()`, `UseObjectSetWriter<T>()`
   - File: `src/Strategos.Ontology/Configuration/OntologyServiceCollectionExtensions.cs` — add auto-detection logic

---

### Task 005: FixedSizeChunker
**DR:** DR-3 | **Dependencies:** 002 | **Parallelizable:** Yes (with 006)

1. **[RED]** Write tests:
   - File: `src/Strategos.Ontology.Tests/Chunking/FixedSizeChunkerTests.cs`
   - `Chunk_EmptyText_ReturnsEmptyList`
   - `Chunk_ShortText_ReturnsSingleChunk` — text below MaxTokens → 1 chunk
   - `Chunk_LongText_ReturnsMultipleChunks` — verify count and content
   - `Chunk_WithOverlap_ChunksOverlapCorrectly` — trailing words of chunk N appear at start of chunk N+1
   - `Chunk_Offsets_TrackCharacterPositions` — `StartOffset`/`EndOffset` map back to source
   - `Chunk_Index_IsSequential` — `Index` increments 0, 1, 2...
   - `Chunk_WordBoundary_DoesNotSplitMidWord` — splits at whitespace
   - Expected failure: `FixedSizeChunker` does not exist

2. **[GREEN]** Implement:
   - File: `src/Strategos.Ontology/Chunking/FixedSizeChunker.cs`
   - Token estimation: 1 token ≈ 0.75 words (word count × 0.75)
   - Split at word boundaries, apply overlap

---

### Task 006: SentenceBoundaryChunker
**DR:** DR-3 | **Dependencies:** 002 | **Parallelizable:** Yes (with 005)

1. **[RED]** Write tests:
   - File: `src/Strategos.Ontology.Tests/Chunking/SentenceBoundaryChunkerTests.cs`
   - `Chunk_SingleSentence_ReturnsSingleChunk`
   - `Chunk_MultipleSentences_SplitsAtBoundaries` — `.` `!` `?` followed by whitespace
   - `Chunk_LongSentence_FallsBackToWordSplit` — single sentence exceeding MaxTokens
   - `Chunk_MixedPunctuation_HandlesAllTerminators`
   - `Chunk_WithOverlap_IncludesTrailingSentences`
   - `Chunk_Offsets_TrackCharacterPositions`
   - Expected failure: `SentenceBoundaryChunker` does not exist

2. **[GREEN]** Implement:
   - File: `src/Strategos.Ontology/Chunking/SentenceBoundaryChunker.cs`
   - Sentence detection regex, accumulate sentences until MaxTokens, fallback to word-split

---

### Task 007: ParagraphChunker
**DR:** DR-3 | **Dependencies:** 006 | **Parallelizable:** No (depends on SentenceBoundaryChunker)

1. **[RED]** Write tests:
   - File: `src/Strategos.Ontology.Tests/Chunking/ParagraphChunkerTests.cs`
   - `Chunk_SingleParagraph_ReturnsSingleChunk`
   - `Chunk_MultipleParagraphs_SplitsAtDoubleNewline`
   - `Chunk_LargeParagraph_FallsBackToSentenceSplitting` — delegates to `SentenceBoundaryChunker`
   - `Chunk_MixedLineEndings_HandlesRNAndN`
   - `Chunk_Offsets_TrackCharacterPositions`
   - Expected failure: `ParagraphChunker` does not exist

2. **[GREEN]** Implement:
   - File: `src/Strategos.Ontology/Chunking/ParagraphChunker.cs`
   - Split on `\n\n` (and `\r\n\r\n`), fallback to `SentenceBoundaryChunker` for large paragraphs

---

### Task 008: IngestionPipeline\<T\> and builder
**DR:** DR-5 | **Dependencies:** 001, 002, 003 | **Parallelizable:** No

1. **[RED]** Write tests:
   - File: `src/Strategos.Ontology.Tests/Ingestion/IngestionPipelineBuilderTests.cs`
   - `Build_MissingEmbed_ThrowsInvalidOperationException`
   - `Build_MissingMap_ThrowsInvalidOperationException`
   - `Build_MissingWriteTo_ThrowsInvalidOperationException`
   - `Build_AllComponentsProvided_ReturnsInstance`
   - File: `src/Strategos.Ontology.Tests/Ingestion/IngestionPipelineTests.cs`
   - `ExecuteAsync_SingleText_ChunksEmbedsMapAndStores` — verify full pipeline with in-memory stubs
   - `ExecuteAsync_MultipleTexts_ProcessesAll`
   - `ExecuteAsync_EmptyInput_ReturnsZeroCounts`
   - `ExecuteAsync_BatchesEmbeddings_CallsEmbedBatchAsync` — verify batching behavior
   - `ExecuteAsync_ReportsProgress_WhenProgressProvided`
   - `ExecuteAsync_Cancellation_ThrowsOperationCanceled`
   - `ExecuteAsync_ReturnsCorrectResult_ChunksAndItemCounts`
   - `Chunk_DefaultSentenceBoundary_WhenOptionsOverload` — `.Chunk(options)` uses `SentenceBoundaryChunker`
   - Expected failure: `IngestionPipeline<T>` does not exist

2. **[GREEN]** Implement:
   - File: `src/Strategos.Ontology/Ingestion/IngestionPipeline.cs`
   - File: `src/Strategos.Ontology/Ingestion/IngestionPipelineBuilder.cs`
   - File: `src/Strategos.Ontology/Ingestion/IngestionResult.cs`
   - File: `src/Strategos.Ontology/Ingestion/IngestionProgress.cs`

---

### Task 009: InMemoryObjectSetProvider implements IObjectSetWriter
**DR:** DR-9 | **Dependencies:** 001, 003 | **Parallelizable:** Yes (with 008)

1. **[RED]** Write tests:
   - File: `src/Strategos.Ontology.Tests/ObjectSets/InMemoryObjectSetWriterTests.cs`
   - `StoreAsync_Item_CanBeQueriedBack` — store via writer, retrieve via ExecuteAsync
   - `StoreAsync_ISearchableItem_UsesEmbedding` — item with `ISearchable.Embedding` is stored with vector
   - `StoreBatchAsync_MultipleItems_AllQueryable`
   - `ExecuteSimilarityAsync_WithEmbeddingProvider_UsesRealCosine` — optional `IEmbeddingProvider` enables real similarity
   - `ExecuteSimilarityAsync_WithoutEmbeddingProvider_UsesKeywordScoring` — backward compat
   - `Seed_ExistingMethod_StillWorks` — verify `Seed<T>` unchanged
   - Expected failure: `InMemoryObjectSetProvider` does not implement `IObjectSetWriter`

2. **[GREEN]** Edit:
   - File: `src/Strategos.Ontology/ObjectSets/InMemoryObjectSetProvider.cs`
   - Add `: IObjectSetWriter`, implement `StoreAsync`/`StoreBatchAsync`, add optional `IEmbeddingProvider` ctor param

3. **[REFACTOR]** Ensure all existing tests pass unchanged.

---

## Phase 2: Strategos.Ontology.Embeddings (New Package)

### Task 010: Embeddings project scaffold
**DR:** DR-2 | **Dependencies:** 001 | **Parallelizable:** Yes

1. Create project:
   - File: `src/Strategos.Ontology.Embeddings/Strategos.Ontology.Embeddings.csproj`
   - References: `Strategos.Ontology`, `Microsoft.Extensions.Http`, `Microsoft.Extensions.Options`
   - `<IsAotCompatible>true</IsAotCompatible>`
2. Create test project:
   - File: `src/Strategos.Ontology.Embeddings.Tests/Strategos.Ontology.Embeddings.Tests.csproj`
   - References: main project, TUnit, NSubstitute
3. Add both to `src/strategos.sln`
4. Add packages to `src/Directory.Packages.props`

No TDD phase — scaffold only.

---

### Task 011: OpenAiCompatibleEmbeddingProvider
**DR:** DR-2 | **Dependencies:** 010 | **Parallelizable:** No

1. **[RED]** Write tests:
   - File: `src/Strategos.Ontology.Embeddings.Tests/OpenAiCompatibleEmbeddingProviderTests.cs`
   - `EmbedAsync_SingleText_ReturnsFloatArray` — mock HTTP returns valid embedding response
   - `EmbedAsync_SendsCorrectRequestBody` — verify model, input fields in POST body
   - `EmbedBatchAsync_UnderBatchSize_SingleApiCall` — texts.Count < BatchSize → 1 HTTP call
   - `EmbedBatchAsync_ExceedsBatchSize_SplitsIntoBatches` — texts.Count > BatchSize → multiple calls
   - `EmbedBatchAsync_ReturnsAllEmbeddings_InOrder` — result count matches input count
   - `Dimensions_ReturnsConfiguredValue` — matches `OpenAiEmbeddingOptions.Dimensions`
   - `EmbedAsync_HttpError_ThrowsHttpRequestException` — non-2xx response throws with details
   - `EmbedAsync_CustomEndpoint_UsesConfiguredUrl` — endpoint from options
   - Expected failure: `OpenAiCompatibleEmbeddingProvider` does not exist

2. **[GREEN]** Implement:
   - File: `src/Strategos.Ontology.Embeddings/OpenAiEmbeddingOptions.cs`
   - File: `src/Strategos.Ontology.Embeddings/OpenAiCompatibleEmbeddingProvider.cs`
   - Internal DTOs for request/response, `System.Net.Http.Json` serialization

---

### Task 012: Embeddings DI extensions
**DR:** DR-7 | **Dependencies:** 011 | **Parallelizable:** No

1. **[RED]** Write tests:
   - File: `src/Strategos.Ontology.Embeddings.Tests/EmbeddingServiceCollectionExtensionsTests.cs`
   - `AddOpenAiEmbeddings_RegistersIEmbeddingProvider` — resolve from DI
   - `AddOpenAiEmbeddings_ConfiguresOptions` — `OpenAiEmbeddingOptions` bound correctly
   - `AddOpenAiEmbeddings_RegistersHttpClient` — `HttpClient` available for provider
   - Expected failure: extension method does not exist

2. **[GREEN]** Implement:
   - File: `src/Strategos.Ontology.Embeddings/EmbeddingServiceCollectionExtensions.cs`

---

## Phase 3: Strategos.Ontology.Npgsql (New Package)

### Task 013: Npgsql project scaffold
**DR:** DR-6 | **Dependencies:** 001, 003 | **Parallelizable:** Yes

1. Create project:
   - File: `src/Strategos.Ontology.Npgsql/Strategos.Ontology.Npgsql.csproj`
   - References: `Strategos.Ontology`, `Npgsql`, `Pgvector`, `Microsoft.Extensions.Options`
2. Create test project:
   - File: `src/Strategos.Ontology.Npgsql.Tests/Strategos.Ontology.Npgsql.Tests.csproj`
   - References: main project, TUnit, NSubstitute, `Testcontainers.PostgreSql`
3. Add both to `src/strategos.sln`
4. Add packages to `src/Directory.Packages.props`

No TDD phase — scaffold only.

---

### Task 014: PgVectorObjectSetProvider — similarity queries
**DR:** DR-6 | **Dependencies:** 013 | **Parallelizable:** No

1. **[RED]** Write tests (Testcontainers):
   - File: `src/Strategos.Ontology.Npgsql.Tests/PgVectorSimilarityTests.cs`
   - `ExecuteSimilarityAsync_CosineDistance_ReturnsRankedResults` — seed vectors, query, verify ordering
   - `ExecuteSimilarityAsync_L2Distance_UsesCorrectOperator`
   - `ExecuteSimilarityAsync_InnerProduct_UsesCorrectOperator`
   - `ExecuteSimilarityAsync_NullQueryVector_CallsEmbeddingProvider` — verify `EmbedAsync` invoked
   - `ExecuteSimilarityAsync_WithQueryVector_BypassesEmbedding` — uses provided vector directly
   - `ExecuteSimilarityAsync_MinRelevance_ExcludesLowScores`
   - `ExecuteSimilarityAsync_TopK_LimitsResults`
   - Expected failure: `PgVectorObjectSetProvider` does not exist

2. **[GREEN]** Implement:
   - File: `src/Strategos.Ontology.Npgsql/PgVectorOptions.cs`
   - File: `src/Strategos.Ontology.Npgsql/PgVectorObjectSetProvider.cs`
   - File: `src/Strategos.Ontology.Npgsql/Internal/TypeMapper.cs` — PascalCase → snake_case

---

### Task 015: PgVectorObjectSetProvider — filter and stream queries
**DR:** DR-6 | **Dependencies:** 014 | **Parallelizable:** No

1. **[RED]** Write tests (Testcontainers):
   - File: `src/Strategos.Ontology.Npgsql.Tests/PgVectorQueryTests.cs`
   - `ExecuteAsync_RootExpression_ReturnsAllItems`
   - `ExecuteAsync_FilterExpression_AppliesWhere` — filter by property value
   - `StreamAsync_RootExpression_YieldsAllItems`
   - `StreamAsync_FilterExpression_YieldsFilteredItems`
   - `ExecuteAsync_EmptyTable_ReturnsEmptyResult`
   - Expected failure: `ExecuteAsync`/`StreamAsync` not implemented

2. **[GREEN]** Implement:
   - File: `src/Strategos.Ontology.Npgsql/PgVectorObjectSetProvider.cs` (edit)
   - File: `src/Strategos.Ontology.Npgsql/Internal/ExpressionTranslator.cs` — `FilterExpression` → SQL WHERE

---

### Task 016: PgVectorObjectSetProvider — write path
**DR:** DR-6 | **Dependencies:** 014 | **Parallelizable:** Yes (with 015)

1. **[RED]** Write tests (Testcontainers):
   - File: `src/Strategos.Ontology.Npgsql.Tests/PgVectorWriteTests.cs`
   - `StoreAsync_SingleItem_CanBeRetrieved` — store + ExecuteAsync roundtrip
   - `StoreAsync_ISearchableItem_UsesProvidedEmbedding` — `ISearchable.Embedding` passthrough
   - `StoreAsync_NonSearchableItem_CallsEmbeddingProvider` — auto-embed via `EmbedAsync`
   - `StoreBatchAsync_MultipleItems_AllRetrievable` — bulk store + verify count
   - `StoreBatchAsync_UsesCopy_ForPerformance` — verify COPY path (or batch INSERT)
   - Expected failure: `IObjectSetWriter` methods not implemented

2. **[GREEN]** Implement:
   - File: `src/Strategos.Ontology.Npgsql/PgVectorObjectSetProvider.cs` (edit)
   - `StoreAsync` with parameterized INSERT, `StoreBatchAsync` with Npgsql binary COPY

---

### Task 017: Schema management (EnsureSchemaAsync)
**DR:** DR-6 | **Dependencies:** 014 | **Parallelizable:** Yes (with 015, 016)

1. **[RED]** Write tests (Testcontainers):
   - File: `src/Strategos.Ontology.Npgsql.Tests/PgVectorSchemaTests.cs`
   - `EnsureSchemaAsync_CreatesTable_WithExpectedColumns` — verify id, data, embedding, created_at
   - `EnsureSchemaAsync_CreatesVectorExtension`
   - `EnsureSchemaAsync_CreatesIvfFlatIndex_ByDefault`
   - `EnsureSchemaAsync_Idempotent_NoErrorOnRerun`
   - `EnsureSchemaAsync_HnswIndex_WhenConfigured` — `PgVectorOptions.IndexType = Hnsw`
   - `EnsureSchemaAsync_VectorDimension_MatchesEmbeddingProvider`
   - Expected failure: `EnsureSchemaAsync` does not exist

2. **[GREEN]** Implement:
   - File: `src/Strategos.Ontology.Npgsql/PgVectorObjectSetProvider.cs` (edit)
   - CREATE EXTENSION, CREATE TABLE, CREATE INDEX with correct vector dimensions

---

### Task 018: Npgsql DI extensions
**DR:** DR-7 | **Dependencies:** 014 | **Parallelizable:** Yes (with 015-017)

1. **[RED]** Write tests:
   - File: `src/Strategos.Ontology.Npgsql.Tests/PgVectorServiceCollectionExtensionsTests.cs`
   - `AddPgVectorObjectSets_RegistersIObjectSetProvider`
   - `AddPgVectorObjectSets_RegistersIObjectSetWriter`
   - `UsePgVector_OnOntologyOptions_RegistersBothInterfaces`
   - Expected failure: extension methods do not exist

2. **[GREEN]** Implement:
   - File: `src/Strategos.Ontology.Npgsql/PgVectorServiceCollectionExtensions.cs`

---

## Phase 4: Integration & Documentation

### Task 019: End-to-end integration test
**DR:** DR-5, DR-6 | **Dependencies:** 008, 012, 018 | **Parallelizable:** No

1. **[RED]** Write test (Testcontainers):
   - File: `src/Strategos.Ontology.Npgsql.Tests/Integration/IngestionPipelineIntegrationTests.cs`
   - `FullPipeline_IngestAndQuery_ReturnsRelevantResults` — raw text → SentenceBoundaryChunker → mock HTTP embedder → PgVector store → ObjectSet.SimilarTo() → verify ranked results
   - `FullPipeline_WithDI_ResolvesAllComponents` — wire up via `AddOntology` + `AddOpenAiEmbeddings` + `AddPgVectorObjectSets`

2. **[GREEN]** Wire up full pipeline; tests should pass with existing implementations.

---

### Task 020: Update platform-architecture.md
**DR:** DR-8 | **Dependencies:** 019 | **Parallelizable:** No

1. Edit `docs/reference/platform-architecture.md`:
   - §12.2: Replace `IVectorSearchAdapter` references with `IObjectSetProvider` + `IngestionPipeline<T>`
   - §12.3: Replace manual adapter registration with package-based approach
   - §4.14: Add subsection covering embedding, chunking, pgvector packages
   - Remove "deferred" status for context assembly and RAG
   - Ensure all code examples compile against new API

No TDD — documentation only.

---

### Task 021: Update package descriptions
**DR:** DR-8 | **Dependencies:** 020 | **Parallelizable:** No

1. Edit `src/Strategos.Ontology/Strategos.Ontology.csproj` — update description to mention embedding/chunking/ingestion
2. Verify README references if any exist

No TDD — metadata only.

---

## Dependency Graph

```text
Tasks 001, 002, 003 ──→ all independent, run in parallel

Task 004 ──→ blocked by 001 + 003

Task 005 ──→ blocked by 002 ─┐
Task 006 ──→ blocked by 002 ─┼─ chunkers can parallelize
Task 007 ──→ blocked by 006 ─┘

Task 008 ──→ blocked by 001 + 002 + 003
Task 009 ──→ blocked by 001 + 003

Task 010 ──→ blocked by 001
Task 011 ──→ blocked by 010
Task 012 ──→ blocked by 011

Task 013 ──→ blocked by 001 + 003
Task 014 ──→ blocked by 013
Task 015 ──→ blocked by 014
Task 016 ──→ blocked by 014 ─┐
Task 017 ──→ blocked by 014 ─┼─ can parallelize
Task 018 ──→ blocked by 014 ─┘

Task 019 ──→ blocked by 008 + 012 + 018
Task 020 ──→ blocked by 019
Task 021 ──→ blocked by 020
```

## Delegation Groups

| Group | Tasks | Description | Blocked By |
|-------|-------|-------------|------------|
| **A** | 001, 002, 003 | Core interfaces (parallel) | — |
| **B** | 004 | OntologyOptions extensions | A |
| **C** | 005, 006 | Fixed + Sentence chunkers (parallel) | 002 |
| **D** | 007 | Paragraph chunker | 006 |
| **E** | 008, 009 | Pipeline + InMemory writer (parallel) | A |
| **F** | 010, 011, 012 | Embeddings package (sequential) | 001 |
| **G** | 013, 014 | Npgsql scaffold + similarity | 001, 003 |
| **H** | 015, 016, 017, 018 | Npgsql query/write/schema/DI (parallel) | 014 |
| **I** | 019 | E2E integration test | E, F, H |
| **J** | 020, 021 | Documentation | I |

**Critical path:** 001 → 010 → 011 → 012 → 019 → 020 → 021
(or: 001+003 → 013 → 014 → 015/016/017/018 → 019)

**Estimated new tests:** ~80-100 across all phases.
