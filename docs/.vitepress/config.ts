import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'Agentic.Workflow',
  description: 'Deterministic, auditable AI agent workflows for .NET',

  srcExclude: [
    '**/archive/**',
    '**/theory/**',
    'design.md',
    'deferred-features.md',
    'diagnostics.md',
    'integrations.md',
    'packages.md',
  ],

  // Ignore dead links during initial setup - will be resolved as content is added
  ignoreDeadLinks: true,

  head: [
    ['link', { rel: 'icon', type: 'image/svg+xml', href: '/logo.svg' }],
  ],

  themeConfig: {
    logo: '/logo.svg',

    nav: [
      { text: 'Learn', link: '/learn/' },
      { text: 'Guide', link: '/guide/' },
      { text: 'Reference', link: '/reference/' },
      { text: 'Examples', link: '/examples/' },
    ],

    sidebar: {
      '/learn/': [
        {
          text: 'Learn',
          items: [
            { text: 'Why Agentic.Workflow', link: '/learn/' },
            { text: 'Core Concepts', link: '/learn/core-concepts' },
            { text: 'Comparison', link: '/learn/comparison' },
          ],
        },
      ],
      '/guide/': [
        {
          text: 'Getting Started',
          items: [
            { text: 'Overview', link: '/guide/' },
            { text: 'Installation', link: '/guide/installation' },
            { text: 'First Workflow', link: '/guide/first-workflow' },
          ],
        },
        {
          text: 'Workflow Patterns',
          items: [
            { text: 'Branching', link: '/guide/branching' },
            { text: 'Parallel Execution', link: '/guide/parallel' },
            { text: 'Loops', link: '/guide/loops' },
            { text: 'Approvals', link: '/guide/approvals' },
          ],
        },
        {
          text: 'Agents',
          items: [
            { text: 'Agent Selection', link: '/guide/agents' },
          ],
        },
      ],
      '/reference/': [
        {
          text: 'Reference',
          items: [
            { text: 'Overview', link: '/reference/' },
            { text: 'Packages', link: '/reference/packages' },
            { text: 'Diagnostics', link: '/reference/diagnostics' },
            { text: 'Configuration', link: '/reference/configuration' },
          ],
        },
        {
          text: 'API Documentation',
          items: [
            { text: 'Workflow API', link: '/reference/api/workflow' },
            { text: 'Generators', link: '/reference/api/generators' },
            { text: 'Infrastructure', link: '/reference/api/infrastructure' },
            { text: 'Agents', link: '/reference/api/agents' },
            { text: 'RAG', link: '/reference/api/rag' },
          ],
        },
      ],
      '/examples/': [
        {
          text: 'Examples',
          items: [
            { text: 'Overview', link: '/examples/' },
            { text: 'Order Processing', link: '/examples/order-processing' },
            { text: 'Content Pipeline', link: '/examples/content-pipeline' },
            { text: 'Code Review', link: '/examples/code-review' },
          ],
        },
      ],
    },

    socialLinks: [
      { icon: 'github', link: 'https://github.com/lvlup-sw/agentic-workflow' },
    ],

    editLink: {
      pattern: 'https://github.com/lvlup-sw/agentic-workflow/edit/main/docs/:path',
      text: 'Edit this page on GitHub',
    },

    search: {
      provider: 'local',
    },

    footer: {
      message: 'Released under the MIT License.',
      copyright: 'Copyright (c) lvlup-sw',
    },
  },
})
