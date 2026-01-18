// =============================================================================
// <copyright file="Program.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using BenchmarkDotNet.Running;

namespace Agentic.Workflow.Benchmarks;

/// <summary>
/// Entry point for the benchmark runner.
/// </summary>
public static class Program
{
    /// <summary>
    /// Main entry point that launches the BenchmarkSwitcher.
    /// </summary>
    /// <param name="args">Command-line arguments for benchmark configuration.</param>
    public static void Main(string[] args)
    {
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args, new BenchmarkConfig());
    }
}
