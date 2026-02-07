# Security Audit Report

**Date:** 2026-01-31
**Target:** Agentic.Workflow Codebase
**Commit:** d9824ff9f127d61642837bdbfc6517e3272d135b

## Summary

This security audit analyzed the `Agentic.Workflow` codebase for common vulnerabilities including hardcoded secrets, injection attacks, insecure dependencies, and cryptographic weaknesses. The codebase is generally well-structured and follows modern .NET security practices. Two findings were identified, one of Medium severity and one of Low severity.

## Findings

### 1. Weak Random Number Generation
**Severity:** Medium
**File path:**
- `src/Agentic.Workflow.Infrastructure/Selection/ThompsonSamplingAgentSelector.cs` (Line 48)
- `src/Agentic.Workflow.Infrastructure/Selection/ContextualAgentSelector.cs` (Line 65)

**Description:**
The `ThompsonSamplingAgentSelector` and `ContextualAgentSelector` classes use `System.Random` for sampling from Beta and Gamma distributions. `System.Random` is not a cryptographically secure pseudo-random number generator (CSPRNG). If the seed is known (default constructor uses a time-dependent seed) or the state can be inferred from outputs, an attacker could potentially predict the sequence of random numbers.
In the context of agent selection, this predictability could allow an attacker to manipulate the environment or inputs to force the selection of a specific agent (e.g., a less robust or compromised agent) by anticipating the random sampling outcome.

**Recommended Fix:**
Replace `System.Random` with `System.Security.Cryptography.RandomNumberGenerator` for generating random values. Although `RandomNumberGenerator` is computationally more expensive, it provides unpredictable randomness which is crucial if the agent selection process needs to be resistant to adversarial manipulation. Alternatively, if performance is critical and the threat model allows, document this limitation clearly.

### 2. Potential Denial of Service (DoS) in Loop Detection
**Severity:** Low
**File path:** `src/Agentic.Workflow.Infrastructure/LoopDetection/LoopDetector.cs`

**Description:**
The `CalculateOscillationScore` method implements an algorithm with O(N^2) complexity, where N is the window size. Specifically, it iterates through all possible periods from 2 to N/2, and for each period, it iterates through the entries.
While the algorithm itself is computationally intensive for large N, the risk is currently mitigated by the `LoopDetectionOptions` validation which strictly limits `WindowSize` to a maximum of 20. If this validation were to be removed or significantly relaxed in the future without optimizing the algorithm, it could expose the system to CPU exhaustion attacks (DoS) via large ledgers.

**Recommended Fix:**
Ensure that the `WindowSize` limit in `LoopDetectionOptions` remains enforced. Consider adding a code comment in `LoopDetector.cs` warning about the complexity and the reliance on the window size limit. For a more robust fix, the oscillation detection algorithm could be optimized (e.g., using autocorrelation or suffix trees), though this may be unnecessary given the current constraints.

## Other Checks Performed

- **Hardcoded Secrets:** Scanned for API keys, passwords, tokens, and common secret patterns (e.g., `sk-`, `ghp_`). **None found.**
- **SQL Injection:** Checked for raw SQL execution and unsafe parameter handling. The project uses structured data storage (ledgers) and does not appear to construct raw SQL queries from user input. **None found.**
- **Command Injection:** Checked for `Process.Start` and shell execution. **None found.**
- **Insecure Dependencies:** Reviewed `Directory.Packages.props`. Packages appear to be up-to-date and maintained (targeting .NET 10). **No known CVEs found in specified versions.**
- **Authentication/Authorization:** The library itself is agnostic to auth, but sample applications use Mock services for demonstration. This is expected for samples but should be replaced with real auth in production.
- **Cryptography:** Integrity checks use `SHA256` (via `TaskLedger`), which is secure.
