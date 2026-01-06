# Agentic.Workflow.Rag

This package provides vector store adapters for Agentic.Workflow's RAG (Retrieval-Augmented Generation) integration.

## Implemented Adapters

- **InMemoryAdapter**: A simple in-memory vector store for testing and development.
- **PgVectorAdapter**: (Planned) PostgreSQL pgvector integration.
- **AzureAISearchAdapter**: (Planned) Azure AI Search integration.

## Usage

Register the adapter in your dependency injection container:

```csharp
services.AddSingleton<IVectorSearchAdapter, InMemoryVectorSearchAdapter>();
```
