// =============================================================================
// <copyright file="BenchmarkConfig.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Validators;

namespace Agentic.Workflow.Benchmarks;

/// <summary>
/// Configuration for BenchmarkDotNet benchmark runs.
/// </summary>
/// <remarks>
/// <para>
/// Configures benchmarks to run on .NET 10 runtime
/// with memory diagnostics and P95 statistics.
/// </para>
/// <para>
/// Output formats include GitHub Markdown and full JSON exports
/// for CI/CD integration and historical tracking.
/// </para>
/// </remarks>
public sealed class BenchmarkConfig : ManualConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BenchmarkConfig"/> class.
    /// </summary>
    public BenchmarkConfig()
    {
        // Job for .NET 10 runtime
        _ = this.AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core10_0)
            .WithId("net10"));

        // Diagnosers
        _ = this.AddDiagnoser(MemoryDiagnoser.Default);

        // Column configuration
        _ = this.AddColumn(StatisticColumn.P95);
        _ = this.AddColumn(RankColumn.Arabic);

        // Export formats
        _ = this.AddExporter(MarkdownExporter.GitHub);
        _ = this.AddExporter(JsonExporter.Full);

        // Validation
        _ = this.AddValidator(BaselineValidator.FailOnError);
        _ = this.WithOptions(ConfigOptions.DisableOptimizationsValidator);
    }
}
