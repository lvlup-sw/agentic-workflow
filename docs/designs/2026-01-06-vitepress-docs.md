# Design: VitePress Documentation Site

## Problem Statement

Agentic.Workflow has comprehensive Markdown documentation scattered across `docs/` but lacks a navigable, user-friendly website. Developers evaluating the library struggle to understand its value proposition quickly, while those integrating it have difficulty finding relevant guides and references. A structured documentation site will improve discoverability, onboarding, and adoption.

## Chosen Approach

**Audience-Oriented Structure** — Reorganize existing docs into clear sections by user journey: Learn, Guide, Reference, and Examples. This serves both evaluators (who need the "why") and integrators (who need the "how") without overcomplicating the build pipeline.

### Rationale

- Existing docs are high-quality but organized by topic rather than user need
- VitePress provides excellent DX with minimal configuration
- GitHub Pages offers free, reliable hosting with easy CI/CD
- Structure scales as documentation grows

## Technical Design

### Project Structure

```
docs/
├── .vitepress/
│   ├── config.ts           # Site config, nav, sidebar
│   └── theme/
│       └── index.ts        # Theme customization (optional)
├── public/
│   └── logo.svg            # Site logo/favicon
├── index.md                # Landing page (hero + features)
├── learn/
│   ├── index.md            # Why Agentic.Workflow
│   ├── concepts.md         # Core concepts (event sourcing, sagas)
│   └── comparison.md       # vs LangGraph, Temporal, etc.
├── guide/
│   ├── index.md            # Getting started overview
│   ├── installation.md     # Package installation
│   ├── first-workflow.md   # Hello world tutorial
│   ├── branching.md        # Conditional workflows
│   ├── parallel.md         # Fork-join patterns
│   ├── loops.md            # Iterative refinement
│   ├── approvals.md        # Human-in-the-loop
│   └── agents.md           # Thompson sampling + LLM integration
├── reference/
│   ├── index.md            # Reference overview
│   ├── packages.md         # Package ecosystem
│   ├── api/
│   │   ├── workflow.md     # Agentic.Workflow types
│   │   ├── generators.md   # Source generator outputs
│   │   ├── infrastructure.md
│   │   ├── agents.md
│   │   └── rag.md
│   ├── diagnostics.md      # Compile-time diagnostics
│   └── configuration.md    # Wolverine/Marten setup
├── examples/
│   ├── index.md            # Examples overview
│   ├── order-processing.md # E-commerce workflow
│   ├── content-pipeline.md # AI content generation
│   └── code-review.md      # Automated PR review
└── contributing.md         # Contribution guide
```

### VitePress Configuration

```typescript
// docs/.vitepress/config.ts
import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'Agentic.Workflow',
  description: 'Deterministic, auditable AI agent workflows for .NET',

  head: [
    ['link', { rel: 'icon', href: '/logo.svg' }]
  ],

  themeConfig: {
    logo: '/logo.svg',

    nav: [
      { text: 'Learn', link: '/learn/' },
      { text: 'Guide', link: '/guide/' },
      { text: 'Reference', link: '/reference/' },
      { text: 'Examples', link: '/examples/' }
    ],

    sidebar: {
      '/learn/': [
        {
          text: 'Learn',
          items: [
            { text: 'Why Agentic.Workflow', link: '/learn/' },
            { text: 'Core Concepts', link: '/learn/concepts' },
            { text: 'Comparison', link: '/learn/comparison' }
          ]
        }
      ],
      '/guide/': [
        {
          text: 'Getting Started',
          items: [
            { text: 'Overview', link: '/guide/' },
            { text: 'Installation', link: '/guide/installation' },
            { text: 'First Workflow', link: '/guide/first-workflow' }
          ]
        },
        {
          text: 'Workflow Patterns',
          items: [
            { text: 'Branching', link: '/guide/branching' },
            { text: 'Parallel Execution', link: '/guide/parallel' },
            { text: 'Loops', link: '/guide/loops' },
            { text: 'Approvals', link: '/guide/approvals' }
          ]
        },
        {
          text: 'AI Integration',
          items: [
            { text: 'Agent Selection', link: '/guide/agents' }
          ]
        }
      ],
      '/reference/': [
        {
          text: 'Reference',
          items: [
            { text: 'Overview', link: '/reference/' },
            { text: 'Packages', link: '/reference/packages' },
            { text: 'Diagnostics', link: '/reference/diagnostics' },
            { text: 'Configuration', link: '/reference/configuration' }
          ]
        },
        {
          text: 'API',
          items: [
            { text: 'Agentic.Workflow', link: '/reference/api/workflow' },
            { text: 'Generators', link: '/reference/api/generators' },
            { text: 'Infrastructure', link: '/reference/api/infrastructure' },
            { text: 'Agents', link: '/reference/api/agents' },
            { text: 'RAG', link: '/reference/api/rag' }
          ]
        }
      ],
      '/examples/': [
        {
          text: 'Examples',
          items: [
            { text: 'Overview', link: '/examples/' },
            { text: 'Order Processing', link: '/examples/order-processing' },
            { text: 'Content Pipeline', link: '/examples/content-pipeline' },
            { text: 'Code Review', link: '/examples/code-review' }
          ]
        }
      ]
    },

    socialLinks: [
      { icon: 'github', link: 'https://github.com/lvlup-sw/agentic-workflow' }
    ],

    search: {
      provider: 'local'
    },

    editLink: {
      pattern: 'https://github.com/lvlup-sw/agentic-workflow/edit/main/docs/:path',
      text: 'Edit this page on GitHub'
    },

    footer: {
      message: 'Released under the MIT License.',
      copyright: 'Copyright © 2024-present LvlUp Software'
    }
  }
})
```

### Landing Page Structure

The `index.md` landing page uses VitePress frontmatter for hero section:

```markdown
---
layout: home

hero:
  name: Agentic.Workflow
  text: Deterministic AI Agent Workflows
  tagline: Build auditable, event-sourced workflows for .NET with a fluent DSL
  actions:
    - theme: brand
      text: Get Started
      link: /guide/
    - theme: alt
      text: Why Agentic?
      link: /learn/

features:
  - icon: 🎯
    title: Deterministic Execution
    details: Agent outputs become immutable events. Replay any workflow with identical results.
  - icon: 🔍
    title: Full Auditability
    details: Every decision captured with context. Time-travel debugging built in.
  - icon: ⚡
    title: Compile-Time Safety
    details: Invalid workflows fail at build time with clear diagnostics.
  - icon: 🤖
    title: Agent-Native
    details: Thompson Sampling for intelligent agent selection. Confidence-based routing.
---
```

### Content Migration Plan

| Source File | Destination | Action |
|-------------|-------------|--------|
| `README.md` | `learn/index.md` | Extract value prop, trim install instructions |
| `docs/design.md` | `learn/concepts.md` | Extract core concepts section |
| `README.md` (comparison table) | `learn/comparison.md` | Expand with more detail |
| `docs/packages.md` | `reference/packages.md` | Move as-is |
| `docs/diagnostics.md` | `reference/diagnostics.md` | Move as-is |
| `docs/integrations.md` | `reference/configuration.md` | Rename, light edits |
| `docs/examples/basic-workflow.md` | `guide/first-workflow.md` | Adapt as tutorial |
| `docs/examples/branching.md` | `guide/branching.md` | Move, add context |
| `docs/examples/fork-join.md` | `guide/parallel.md` | Rename, move |
| `docs/examples/iterative-refinement.md` | `guide/loops.md` | Rename, move |
| `docs/examples/approval-flow.md` | `guide/approvals.md` | Move |
| `docs/examples/thompson-sampling.md` | `guide/agents.md` | Rename, move |
| New content | `guide/installation.md` | Write new |
| New content | `examples/*.md` | Write 2-3 end-to-end examples |

### GitHub Actions Deployment

```yaml
# .github/workflows/docs.yml
name: Deploy Docs

on:
  push:
    branches: [main]
    paths:
      - 'docs/**'
      - '.github/workflows/docs.yml'
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: pages
  cancel-in-progress: false

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - uses: actions/setup-node@v4
        with:
          node-version: 20
          cache: npm
          cache-dependency-path: docs/package-lock.json

      - name: Install dependencies
        run: npm ci
        working-directory: docs

      - name: Build docs
        run: npm run docs:build
        working-directory: docs

      - uses: actions/configure-pages@v4

      - uses: actions/upload-pages-artifact@v3
        with:
          path: docs/.vitepress/dist

  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    needs: build
    runs-on: ubuntu-latest
    steps:
      - id: deployment
        uses: actions/deploy-pages@v4
```

### Package Configuration

```json
// docs/package.json
{
  "name": "agentic-workflow-docs",
  "private": true,
  "type": "module",
  "scripts": {
    "docs:dev": "vitepress dev",
    "docs:build": "vitepress build",
    "docs:preview": "vitepress preview"
  },
  "devDependencies": {
    "vitepress": "^1.5.0"
  }
}
```

## Integration Points

### Existing Codebase

- **No changes to src/** — Documentation is purely additive
- **docs/ restructure** — Existing files move but content preserved
- **README.md** — Simplified to point to docs site, keep quick install

### CI/CD Integration

- New workflow `docs.yml` runs independently of main CI
- Triggers only on `docs/**` changes to avoid unnecessary builds
- Uses GitHub Pages deployment action (no external services)

### Future Extensibility

- **Versioned docs:** VitePress supports version dropdowns via config
- **API generation:** Can add `typedoc` or `docfx` output as separate build step
- **Internationalization:** VitePress has built-in i18n support
- **Algolia search:** Can upgrade from local search later

## Testing Strategy

### Manual Verification

1. Run `npm run docs:dev` locally and verify all pages render
2. Check all internal links resolve (VitePress warns on broken links)
3. Test search functionality finds expected content
4. Verify mobile responsiveness in browser dev tools

### Automated Checks

1. VitePress build fails on broken links (built-in)
2. Add `markdownlint` to CI for consistent formatting
3. Consider `linkcheck` action for external link validation

### Acceptance Criteria

- [ ] All existing doc content accessible via navigation
- [ ] Landing page clearly communicates value proposition
- [ ] Getting started guide takes user from install to running workflow
- [ ] Search returns relevant results for key terms
- [ ] Site deploys automatically on push to main
- [ ] Mobile layout is usable

## Open Questions

1. **Logo/branding:** Does a logo exist, or should we use text-only header?
2. **Code syntax highlighting:** Should we add `.NET`-specific language aliases?
3. **Analytics:** Add Plausible/Fathom for privacy-friendly analytics?
4. **Custom domain:** Configure later or set up initially?

## Implementation Tasks

1. Initialize VitePress in `docs/` with npm
2. Create `.vitepress/config.ts` with navigation structure
3. Create landing page `index.md` with hero and features
4. Restructure existing docs into Learn/Guide/Reference/Examples
5. Write new `installation.md` getting started content
6. Create 2-3 end-to-end example docs
7. Add GitHub Actions workflow for deployment
8. Configure GitHub Pages in repository settings
9. Update root README.md to link to docs site
10. Test locally and deploy
