# Implementation Plan: VitePress Documentation Site

## Source Design

Link: `docs/designs/2026-01-06-vitepress-docs.md`

## Summary

- **Total tasks:** 6
- **Parallel groups:** 4 (content sections can run in parallel)
- **Verification:** VitePress build succeeds with no broken links

## Task Breakdown

---

### Task 001: Initialize VitePress Foundation

**Objective:** Set up VitePress project structure with config and landing page.

**Steps:**

1. Create `docs/package.json` with VitePress dependency
2. Create `docs/.vitepress/config.ts` with full navigation structure
3. Create `docs/index.md` landing page with hero and features
4. Create placeholder `index.md` files for each section (learn, guide, reference, examples)
5. Run `npm install` and verify `npm run docs:dev` starts successfully

**Files to create:**
- `docs/package.json`
- `docs/.vitepress/config.ts`
- `docs/index.md`
- `docs/learn/index.md` (placeholder)
- `docs/guide/index.md` (placeholder)
- `docs/reference/index.md` (placeholder)
- `docs/examples/index.md` (placeholder)

**Verification:**
- [ ] `npm run docs:dev` starts without errors
- [ ] Landing page renders with hero section
- [ ] Navigation shows all four sections

**Dependencies:** None
**Parallelizable:** No (foundation for all other tasks)
**Branch:** `feature/001-vitepress-init`

---

### Task 002: Learn Section Content

**Objective:** Create the "Learn" section with value proposition, concepts, and comparison.

**Steps:**

1. Create `docs/learn/index.md` — Extract value prop from README.md
2. Create `docs/learn/concepts.md` — Extract core concepts from design.md
3. Create `docs/learn/comparison.md` — Expand comparison table from README.md

**Content migration:**
| Source | Destination | Action |
|--------|-------------|--------|
| `README.md` (The Problem, The Solution, How It Works) | `learn/index.md` | Extract, expand |
| `docs/design.md` (Architecture sections) | `learn/concepts.md` | Extract key concepts |
| `README.md` (comparison table) | `learn/comparison.md` | Expand with details |

**Verification:**
- [ ] All three pages render correctly
- [ ] No broken internal links
- [ ] Sidebar navigation works

**Dependencies:** Task 001
**Parallelizable:** Yes (with Tasks 003, 004, 005)
**Branch:** `feature/002-learn-section`

---

### Task 003: Guide Section Content

**Objective:** Create the "Guide" section with getting started and workflow pattern tutorials.

**Steps:**

1. Create `docs/guide/index.md` — Getting started overview
2. Create `docs/guide/installation.md` — Package installation guide (new content)
3. Migrate `docs/examples/basic-workflow.md` → `docs/guide/first-workflow.md`
4. Migrate `docs/examples/branching.md` → `docs/guide/branching.md`
5. Migrate `docs/examples/fork-join.md` → `docs/guide/parallel.md`
6. Migrate `docs/examples/iterative-refinement.md` → `docs/guide/loops.md`
7. Migrate `docs/examples/approval-flow.md` → `docs/guide/approvals.md`
8. Migrate `docs/examples/thompson-sampling.md` → `docs/guide/agents.md`

**Content migration:**
| Source | Destination | Action |
|--------|-------------|--------|
| New | `guide/index.md` | Write overview |
| New | `guide/installation.md` | Write install guide |
| `examples/basic-workflow.md` | `guide/first-workflow.md` | Adapt as tutorial |
| `examples/branching.md` | `guide/branching.md` | Add context |
| `examples/fork-join.md` | `guide/parallel.md` | Rename, context |
| `examples/iterative-refinement.md` | `guide/loops.md` | Rename, context |
| `examples/approval-flow.md` | `guide/approvals.md` | Add context |
| `examples/thompson-sampling.md` | `guide/agents.md` | Rename, context |

**Verification:**
- [ ] All eight pages render correctly
- [ ] Code examples have proper syntax highlighting
- [ ] Progressive learning path is clear

**Dependencies:** Task 001
**Parallelizable:** Yes (with Tasks 002, 004, 005)
**Branch:** `feature/003-guide-section`

---

### Task 004: Reference Section Content

**Objective:** Create the "Reference" section with API docs, packages, and diagnostics.

**Steps:**

1. Create `docs/reference/index.md` — Reference overview
2. Migrate `docs/packages.md` → `docs/reference/packages.md`
3. Migrate `docs/diagnostics.md` → `docs/reference/diagnostics.md`
4. Migrate `docs/integrations.md` → `docs/reference/configuration.md`
5. Create `docs/reference/api/workflow.md` — Core types reference
6. Create `docs/reference/api/generators.md` — Generator outputs reference
7. Create `docs/reference/api/infrastructure.md` — Infrastructure types
8. Create `docs/reference/api/agents.md` — Agent integration types
9. Create `docs/reference/api/rag.md` — RAG types reference

**Content migration:**
| Source | Destination | Action |
|--------|-------------|--------|
| New | `reference/index.md` | Write overview |
| `packages.md` | `reference/packages.md` | Move as-is |
| `diagnostics.md` | `reference/diagnostics.md` | Move as-is |
| `integrations.md` | `reference/configuration.md` | Rename, adjust |
| New | `reference/api/*.md` | Write from code inspection |

**Verification:**
- [ ] All nine pages render correctly
- [ ] API type links work
- [ ] Diagnostics table renders properly

**Dependencies:** Task 001
**Parallelizable:** Yes (with Tasks 002, 003, 005)
**Branch:** `feature/004-reference-section`

---

### Task 005: Examples Section Content

**Objective:** Create the "Examples" section with end-to-end workflow examples.

**Steps:**

1. Create `docs/examples/index.md` — Examples overview
2. Create `docs/examples/order-processing.md` — E-commerce workflow example
3. Create `docs/examples/content-pipeline.md` — AI content generation example
4. Create `docs/examples/code-review.md` — Automated PR review example

**Content:**
| File | Description |
|------|-------------|
| `examples/index.md` | Overview linking to all examples |
| `examples/order-processing.md` | Complete e-commerce order workflow |
| `examples/content-pipeline.md` | AI-powered content generation |
| `examples/code-review.md` | Automated code review workflow |

**Verification:**
- [ ] All four pages render correctly
- [ ] Code examples are complete and runnable
- [ ] Each example demonstrates different patterns

**Dependencies:** Task 001
**Parallelizable:** Yes (with Tasks 002, 003, 004)
**Branch:** `feature/005-examples-section`

---

### Task 006: CI/CD and Finalization

**Objective:** Add GitHub Actions deployment and update root README.

**Steps:**

1. Create `.github/workflows/docs.yml` — GitHub Pages deployment
2. Create `docs/public/` directory for static assets
3. Update root `README.md` to link to documentation site
4. Create `docs/contributing.md` — Contribution guide for docs
5. Run full build verification: `npm run docs:build`
6. Test local preview: `npm run docs:preview`

**Files to create/modify:**
- `.github/workflows/docs.yml`
- `docs/public/.gitkeep`
- `README.md` (update links)
- `docs/contributing.md`

**Verification:**
- [ ] `npm run docs:build` succeeds with no warnings
- [ ] All internal links resolve
- [ ] GitHub Actions workflow is valid YAML
- [ ] README points to docs site

**Dependencies:** Tasks 002, 003, 004, 005
**Parallelizable:** No (requires all content complete)
**Branch:** `feature/006-cicd-finalization`

---

## Parallelization Strategy

```
Task 001 (Foundation)
    │
    ├──────┬──────┬──────┐
    ▼      ▼      ▼      ▼
Task 002  003   004    005
(Learn) (Guide) (Ref) (Examples)
    │      │      │      │
    └──────┴──────┴──────┘
           │
           ▼
       Task 006
    (CI/CD + Final)
```

### Parallel Groups

| Group | Tasks | Can Run Together |
|-------|-------|------------------|
| Foundation | 001 | Sequential (first) |
| Content | 002, 003, 004, 005 | All 4 in parallel |
| Finalization | 006 | Sequential (last) |

### Worktree Strategy

After Task 001 completes on `main`:
- Worktree A: `feature/002-learn-section`
- Worktree B: `feature/003-guide-section`
- Worktree C: `feature/004-reference-section`
- Worktree D: `feature/005-examples-section`

All four can execute simultaneously. Task 006 runs after integration.

## Completion Checklist

- [ ] VitePress builds successfully
- [ ] All pages accessible via navigation
- [ ] No broken internal links
- [ ] Search indexes all content
- [ ] Mobile layout is functional
- [ ] GitHub Actions workflow ready
- [ ] README updated with docs link
