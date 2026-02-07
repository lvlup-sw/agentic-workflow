# Security Audit Report

This report details the findings of a security audit conducted on the Agentic.Workflow codebase.

## 1. Hardcoded Secrets, API Keys, or Credentials

### Finding 1: Hardcoded Password in Documentation

- **Severity**: Medium
- **File Path**: `docs/guide/installation.md`
- **Line Number**: 131, 144
- **Description**: The documentation contains a hardcoded password ("secret") in the PostgreSQL connection string and the Docker quick-start command. While this is in a documentation file, it encourages insecure practices and could be inadvertently used in production environments.
- **Recommended Fix**: Replace the hardcoded password with a placeholder like `<YOUR_SECURE_PASSWORD>` and advise users to replace it with a strong, unique password.

### Finding 2: Placeholder API Key in Documentation

- **Severity**: Low
- **File Path**: `docs/reference/api/rag.md`
- **Line Number**: 193
- **Description**: The documentation includes a placeholder API key ("your-api-key"). While this is not a hardcoded secret, it could be overlooked by users.
- **Recommended Fix**: Replace the placeholder with `<YOUR_API_KEY>` and add a warning to not hardcode secrets and to use a secret manager.

## 2. SQL Injection or Command Injection Vulnerabilities

No SQL injection or command injection vulnerabilities were found. The project uses Marten and Wolverine for data access, which provide protection against SQL injection by using parameterized queries. The `InMemoryVectorSearchAdapter` uses in-memory keyword matching and does not construct SQL queries.

## 3. XSS Vulnerabilities in any frontend code

No XSS vulnerabilities were found. This project is a .NET library and does not contain any frontend code.

## 4. Insecure Dependencies or Outdated Packages with CVEs

No insecure dependencies or outdated packages with known CVEs were found. The project's dependencies are managed centrally in `Directory.Packages.props` and are up-to-date. The `StyleCop.Analyzers` package is a pre-release version, but it is the version the project has chosen to use and there is no evidence to suggest it is insecure.

## 5. Authentication/Authorization Weaknesses

No authentication or authorization weaknesses were found. This project is a library and does not implement authentication or authorization directly.

## 6. Insecure Cryptographic Practices

No insecure cryptographic practices were found. The project uses the standard .NET `SHA256.HashData` method for hashing, which is a secure and appropriate choice. No custom or insecure cryptographic implementations were found.

## 7. Path Traversal Vulnerability

### Finding 3: Path Traversal in `FileSystemArtifactStore.cs`

- **Severity**: High
- **File Path**: `src/Agentic.Workflow.Infrastructure/ArtifactStores/FileSystemArtifactStore.cs`
- **Line Number**: 77
- **Description**: The `StoreAsync` method in `FileSystemArtifactStore.cs` is vulnerable to a path traversal attack. The `category` parameter is taken from user input and concatenated with the base path without proper sanitization. An attacker could provide a malicious `category` string like `../../../../etc/passwd` to write files to arbitrary locations on the file system.
- **Recommended Fix**: Sanitize the `category` parameter to remove any directory traversal characters. A simple way to do this is to disallow `..` and `/` characters in the `category` name. Alternatively, you can use `Path.GetFullPath` to resolve the path and then check if it is within the expected base directory.
