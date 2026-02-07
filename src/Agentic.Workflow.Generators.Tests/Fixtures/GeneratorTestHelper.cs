// -----------------------------------------------------------------------
// <copyright file="GeneratorTestHelper.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis.Text;

namespace Agentic.Workflow.Generators.Tests.Fixtures;
/// <summary>
/// Provides test infrastructure for running source generators in tests.
/// </summary>
public static class GeneratorTestHelper
{
    /// <summary>
    /// Runs the workflow generator against the provided source code.
    /// </summary>
    /// <param name="source">The source code to compile and run the generator against.</param>
    /// <returns>The generator driver run result containing generated output and diagnostics.</returns>
    public static GeneratorDriverRunResult RunGenerator(string source)
    {
        return RunGenerator<WorkflowIncrementalGenerator>(source);
    }

    /// <summary>
    /// Runs the state reducer generator against the provided source code.
    /// </summary>
    /// <param name="source">The source code to compile and run the generator against.</param>
    /// <returns>The generator driver run result containing generated output and diagnostics.</returns>
    public static GeneratorDriverRunResult RunStateReducerGenerator(string source)
    {
        return RunGenerator<StateReducerIncrementalGenerator>(source);
    }

    /// <summary>
    /// Runs the specified generator against the provided source code.
    /// </summary>
    /// <typeparam name="TGenerator">The type of incremental generator to run.</typeparam>
    /// <param name="source">The source code to compile and run the generator against.</param>
    /// <returns>The generator driver run result containing generated output and diagnostics.</returns>
    public static GeneratorDriverRunResult RunGenerator<TGenerator>(string source)
        where TGenerator : IIncrementalGenerator, new()
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new TGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        return driver.GetRunResult();
    }

    /// <summary>
    /// Gets the generated source code from the run result by hint name suffix.
    /// </summary>
    /// <param name="result">The generator driver run result.</param>
    /// <param name="hintNameSuffix">The suffix of the hint name to find (e.g., "Phase.g.cs").</param>
    /// <returns>The generated source code, or empty string if not found.</returns>
    public static string GetGeneratedSource(GeneratorDriverRunResult result, string hintNameSuffix)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));
        ArgumentNullException.ThrowIfNull(hintNameSuffix, nameof(hintNameSuffix));

        return result.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.EndsWith(hintNameSuffix, StringComparison.Ordinal))
            ?.GetText()
            .ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the compilation diagnostics (errors/warnings) after running the generator.
    /// </summary>
    /// <param name="source">The source code to compile.</param>
    /// <returns>The collection of compilation diagnostics.</returns>
    public static IEnumerable<Diagnostic> GetCompilationDiagnostics(string source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new WorkflowIncrementalGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out _);

        return outputCompilation.GetDiagnostics();
    }

    private static List<MetadataReference> GetMetadataReferences()
    {
        var references = new List<MetadataReference>();

        // Add core runtime references
        var runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        var coreAssemblies = new[]
        {
            "System.Runtime.dll",
            "System.Private.CoreLib.dll",
            "netstandard.dll",
        };

        foreach (var assembly in coreAssemblies)
        {
            var path = Path.Combine(runtimePath, assembly);
            if (File.Exists(path))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        // Add loaded assemblies (filtering out dynamic ones)
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            {
                try
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
                catch
                {
                    // Ignore assemblies that can't be loaded as references
                }
            }
        }

        // Add the Workflow library reference
        var workflowAssembly = typeof(Agentic.Workflow.Abstractions.IWorkflowState).Assembly;
        if (!string.IsNullOrEmpty(workflowAssembly.Location))
        {
            references.Add(MetadataReference.CreateFromFile(workflowAssembly.Location));
        }

        return references;
    }
}