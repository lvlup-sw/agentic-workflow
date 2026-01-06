// -----------------------------------------------------------------------
// <copyright file="StepExtractorContextTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Generators.Tests.Helpers;

using Agentic.Workflow.Generators.Helpers;
using Agentic.Workflow.Generators.Tests.Fixtures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// TDD Cycle 1: Tests for StepContext tracking in StepExtractor.
/// These tests verify that steps are properly marked with their context
/// (Linear, ForkPath, BranchPath) to enable context-aware duplicate detection.
/// </summary>
[Property("Category", "Unit")]
public class StepExtractorContextTests
{
    // =============================================================================
    // A. Linear Flow Context Tests
    // =============================================================================

    /// <summary>
    /// Verifies that ExtractRawStepInfos returns all steps with Linear context
    /// for a simple linear workflow.
    /// </summary>
    [Test]
    public async Task ExtractRawStepInfos_LinearFlow_AllStepsHaveLinearContext()
    {
        // Arrange
        var context = CreateContext(SourceTexts.LinearWorkflow);

        // Act
        var rawSteps = StepExtractor.ExtractRawStepInfos(context);

        // Assert
        await Assert.That(rawSteps.Count).IsEqualTo(3);
        await Assert.That(rawSteps.All(s => s.Context == StepContext.Linear)).IsTrue();
    }

    // =============================================================================
    // B. Fork Path Context Tests
    // =============================================================================

    /// <summary>
    /// Verifies that ExtractRawStepInfos marks fork path steps with ForkPath context.
    /// </summary>
    [Test]
    public async Task ExtractRawStepInfos_ForkPaths_StepsHaveForkPathContext()
    {
        // Arrange - WorkflowWithFork has fork paths with ProcessPayment and ReserveInventory
        var context = CreateContext(SourceTexts.WorkflowWithFork);

        // Act
        var rawSteps = StepExtractor.ExtractRawStepInfos(context);

        // Assert - fork path steps should have ForkPath context
        var forkSteps = rawSteps.Where(s => s.StepName is "ProcessPayment" or "ReserveInventory").ToList();
        await Assert.That(forkSteps.Count).IsEqualTo(2);
        await Assert.That(forkSteps.All(s => s.Context == StepContext.ForkPath)).IsTrue();
    }

    /// <summary>
    /// Verifies that non-fork steps in a fork workflow still have Linear context.
    /// </summary>
    [Test]
    public async Task ExtractRawStepInfos_ForkWorkflow_NonForkStepsHaveLinearContext()
    {
        // Arrange
        var context = CreateContext(SourceTexts.WorkflowWithFork);

        // Act
        var rawSteps = StepExtractor.ExtractRawStepInfos(context);

        // Assert - ValidateOrder and SendConfirmation should be Linear
        var linearSteps = rawSteps.Where(s => s.StepName is "ValidateOrder" or "SendConfirmation").ToList();
        await Assert.That(linearSteps.Count).IsEqualTo(2);
        await Assert.That(linearSteps.All(s => s.Context == StepContext.Linear)).IsTrue();
    }

    // =============================================================================
    // C. Branch Path Context Tests
    // =============================================================================

    /// <summary>
    /// Verifies that ExtractRawStepInfos marks branch path steps with BranchPath context.
    /// </summary>
    [Test]
    public async Task ExtractRawStepInfos_BranchPaths_StepsHaveBranchPathContext()
    {
        // Arrange - WorkflowWithEnumBranch has branch paths with ProcessAutoClaim, ProcessHomeClaim, ProcessLifeClaim
        var context = CreateContext(SourceTexts.WorkflowWithEnumBranch);

        // Act
        var rawSteps = StepExtractor.ExtractRawStepInfos(context);

        // Assert - branch path steps should have BranchPath context
        var branchSteps = rawSteps.Where(s =>
            s.StepName is "ProcessAutoClaim" or "ProcessHomeClaim" or "ProcessLifeClaim").ToList();

        await Assert.That(branchSteps.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(branchSteps.All(s => s.Context == StepContext.BranchPath)).IsTrue();
    }

    // =============================================================================
    // D. No Deduplication Tests
    // =============================================================================

    /// <summary>
    /// Verifies that ExtractRawStepInfos does NOT deduplicate steps.
    /// This is the key difference from ExtractStepInfos - duplicates are preserved
    /// so that duplicate detection can work correctly.
    /// </summary>
    [Test]
    public async Task ExtractRawStepInfos_DuplicateStepsInForkPaths_PreservesDuplicates()
    {
        // Arrange - Create a workflow with duplicate steps in fork paths
        const string duplicateForkWorkflow = """
            using Agentic.Workflow.Abstractions;
            using Agentic.Workflow.Attributes;
            using Agentic.Workflow.Builders;
            using Agentic.Workflow.Definitions;
            using Agentic.Workflow.Steps;

            namespace TestNamespace;

            public record AnalysisState : IWorkflowState
            {
                public Guid WorkflowId { get; init; }
            }

            public class PrepareStep : IWorkflowStep<AnalysisState>
            {
                public Task<StepResult<AnalysisState>> ExecuteAsync(
                    AnalysisState state, StepContext context, CancellationToken ct)
                    => Task.FromResult(StepResult<AnalysisState>.FromState(state));
            }

            public class AnalyzeStep : IWorkflowStep<AnalysisState>
            {
                public Task<StepResult<AnalysisState>> ExecuteAsync(
                    AnalysisState state, StepContext context, CancellationToken ct)
                    => Task.FromResult(StepResult<AnalysisState>.FromState(state));
            }

            public class SynthesizeStep : IWorkflowStep<AnalysisState>
            {
                public Task<StepResult<AnalysisState>> ExecuteAsync(
                    AnalysisState state, StepContext context, CancellationToken ct)
                    => Task.FromResult(StepResult<AnalysisState>.FromState(state));
            }

            public class CompleteStep : IWorkflowStep<AnalysisState>
            {
                public Task<StepResult<AnalysisState>> ExecuteAsync(
                    AnalysisState state, StepContext context, CancellationToken ct)
                    => Task.FromResult(StepResult<AnalysisState>.FromState(state));
            }

            [Workflow("duplicate-fork")]
            public static partial class DuplicateForkWorkflow
            {
                public static WorkflowDefinition<AnalysisState> Definition => Workflow<AnalysisState>
                    .Create("duplicate-fork")
                    .StartWith<PrepareStep>()
                    .Fork(
                        path => path.Then<AnalyzeStep>(),
                        path => path.Then<AnalyzeStep>())
                    .Join<SynthesizeStep>()
                    .Finally<CompleteStep>();
            }
            """;

        var context = CreateContext(duplicateForkWorkflow);

        // Act
        var rawSteps = StepExtractor.ExtractRawStepInfos(context);

        // Assert - should have TWO AnalyzeStep entries (no deduplication)
        var analyzeStepCount = rawSteps.Count(s => s.StepName == "AnalyzeStep");
        await Assert.That(analyzeStepCount).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that the existing ExtractStepInfos still deduplicates (unchanged behavior).
    /// </summary>
    [Test]
    public async Task ExtractStepInfos_DuplicateStepsInForkPaths_Deduplicates()
    {
        // Arrange - Same workflow as above
        const string duplicateForkWorkflow = """
            using Agentic.Workflow.Abstractions;
            using Agentic.Workflow.Attributes;
            using Agentic.Workflow.Builders;
            using Agentic.Workflow.Definitions;
            using Agentic.Workflow.Steps;

            namespace TestNamespace;

            public record AnalysisState : IWorkflowState
            {
                public Guid WorkflowId { get; init; }
            }

            public class PrepareStep : IWorkflowStep<AnalysisState>
            {
                public Task<StepResult<AnalysisState>> ExecuteAsync(
                    AnalysisState state, StepContext context, CancellationToken ct)
                    => Task.FromResult(StepResult<AnalysisState>.FromState(state));
            }

            public class AnalyzeStep : IWorkflowStep<AnalysisState>
            {
                public Task<StepResult<AnalysisState>> ExecuteAsync(
                    AnalysisState state, StepContext context, CancellationToken ct)
                    => Task.FromResult(StepResult<AnalysisState>.FromState(state));
            }

            public class SynthesizeStep : IWorkflowStep<AnalysisState>
            {
                public Task<StepResult<AnalysisState>> ExecuteAsync(
                    AnalysisState state, StepContext context, CancellationToken ct)
                    => Task.FromResult(StepResult<AnalysisState>.FromState(state));
            }

            public class CompleteStep : IWorkflowStep<AnalysisState>
            {
                public Task<StepResult<AnalysisState>> ExecuteAsync(
                    AnalysisState state, StepContext context, CancellationToken ct)
                    => Task.FromResult(StepResult<AnalysisState>.FromState(state));
            }

            [Workflow("duplicate-fork")]
            public static partial class DuplicateForkWorkflow
            {
                public static WorkflowDefinition<AnalysisState> Definition => Workflow<AnalysisState>
                    .Create("duplicate-fork")
                    .StartWith<PrepareStep>()
                    .Fork(
                        path => path.Then<AnalyzeStep>(),
                        path => path.Then<AnalyzeStep>())
                    .Join<SynthesizeStep>()
                    .Finally<CompleteStep>();
            }
            """;

        var context = CreateContext(duplicateForkWorkflow);

        // Act
        var dedupedSteps = StepExtractor.ExtractStepInfos(context);

        // Assert - should have only ONE AnalyzeStep (deduplication preserved)
        var analyzeStepCount = dedupedSteps.Count(s => s.StepName == "AnalyzeStep");
        await Assert.That(analyzeStepCount).IsEqualTo(1);
    }

    // =============================================================================
    // Private Helpers
    // =============================================================================

    private static FluentDslParseContext CreateContext(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();

        return FluentDslParseContext.Create(root, semanticModel, null, CancellationToken.None);
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
