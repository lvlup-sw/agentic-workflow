# Strategos.Rag

This package provides vector store adapters for Strategos's RAG (Retrieval-Augmented Generation) integration.

## Implemented Adapters

- **InMemoryAdapter**: A simple in-memory vector store for testing and development.
- **PgVectorAdapter**: (Planned) PostgreSQL pgvector integration.
- **AzureAISearchAdapter**: (Planned) Azure AI Search integration.

## Usage

Register the adapter in your dependency injection container:

```csharp
services.AddSingleton<IVectorSearchAdapter, InMemoryVectorSearchAdapter>();
```

## Documentation

- **[RAG API Reference](https://lvlup-sw.github.io/strategos/reference/api/rag)** - Complete API documentation
