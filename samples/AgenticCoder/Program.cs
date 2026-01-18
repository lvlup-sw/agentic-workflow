// =============================================================================
// <copyright file="Program.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Steps;
using AgenticCoder.Services;
using AgenticCoder.State;
using AgenticCoder.Steps;

Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine(" AgenticCoder Sample - Iterative Code Generation Workflow");
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine();

// Create mock services
var taskAnalyzer = new MockTaskAnalyzer();
var planner = new MockPlanner();
var codeGenerator = new MockCodeGenerator { AttemptsBeforeSuccess = 3 };
var testRunner = new MockTestRunner();

// Create initial state with a FizzBuzz task
var state = new CoderState
{
    WorkflowId = Guid.NewGuid(),
    TaskDescription = "Implement a FizzBuzz function that returns 'Fizz' for multiples of 3, 'Buzz' for multiples of 5, 'FizzBuzz' for multiples of both, and the number as a string otherwise.",
};

Console.WriteLine($"Workflow ID: {state.WorkflowId}");
Console.WriteLine($"Task: {state.TaskDescription}");
Console.WriteLine();

// Create workflow definition
var workflow = CoderWorkflow.Create();
Console.WriteLine($"Workflow: {workflow.Name}");
Console.WriteLine($"Steps: {workflow.Steps.Count}");
Console.WriteLine($"Loops: {workflow.Loops.Count} (max {workflow.Loops[0].MaxIterations} iterations)");
Console.WriteLine();

// Simulate workflow execution manually (since we don't have the full runtime)
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine(" Step 1: Analyze Task");
Console.WriteLine("-".PadRight(70, '-'));
var analyzeStep = new AnalyzeTask(taskAnalyzer);
var context = StepContext.Create(state.WorkflowId, nameof(AnalyzeTask), "Analyzing");
var result = await analyzeStep.ExecuteAsync(state, context, CancellationToken.None);
state = result.UpdatedState;
Console.WriteLine("Task analyzed successfully.");
Console.WriteLine();

Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine(" Step 2: Plan Implementation");
Console.WriteLine("-".PadRight(70, '-'));
var planStep = new PlanImplementation(planner, taskAnalyzer);
context = StepContext.Create(state.WorkflowId, nameof(PlanImplementation), "Planning");
result = await planStep.ExecuteAsync(state, context, CancellationToken.None);
state = result.UpdatedState;
Console.WriteLine("Plan created:");
Console.WriteLine(state.Plan);
Console.WriteLine();

// Refinement loop simulation
var maxIterations = 3;
for (var iteration = 1; iteration <= maxIterations; iteration++)
{
    Console.WriteLine("-".PadRight(70, '-'));
    Console.WriteLine($" Refinement Loop - Iteration {iteration}/{maxIterations}");
    Console.WriteLine("-".PadRight(70, '-'));

    // Generate Code
    Console.WriteLine();
    Console.WriteLine("[GenerateCode]");
    var generateStep = new GenerateCode(codeGenerator);
    context = StepContext.Create(state.WorkflowId, nameof(GenerateCode), $"Refinement_GenerateCode_{iteration}");
    result = await generateStep.ExecuteAsync(state, context, CancellationToken.None);
    state = result.UpdatedState;
    Console.WriteLine($"Attempt #{state.AttemptCount}:");
    Console.WriteLine($"Reasoning: {state.Attempts[^1].Reasoning}");
    Console.WriteLine("Code:");
    Console.WriteLine(state.Attempts[^1].Code);
    Console.WriteLine();

    // Run Tests
    Console.WriteLine("[RunTests]");
    var testStep = new RunTests(testRunner);
    context = StepContext.Create(state.WorkflowId, nameof(RunTests), $"Refinement_RunTests_{iteration}");
    result = await testStep.ExecuteAsync(state, context, CancellationToken.None);
    state = result.UpdatedState;

    if (state.LatestTestResults!.Passed)
    {
        Console.WriteLine("All tests passed!");
        break;
    }
    else
    {
        Console.WriteLine("Tests failed:");
        foreach (var failure in state.LatestTestResults.Failures)
        {
            Console.WriteLine($"  - {failure}");
        }
    }

    Console.WriteLine();

    // Review Results
    Console.WriteLine("[ReviewResults]");
    var reviewStep = new ReviewResults();
    context = StepContext.Create(state.WorkflowId, nameof(ReviewResults), $"Refinement_ReviewResults_{iteration}");
    result = await reviewStep.ExecuteAsync(state, context, CancellationToken.None);
    state = result.UpdatedState;

    if (!state.LatestTestResults.Passed && iteration < maxIterations)
    {
        Console.WriteLine("Loop condition not met. Continuing refinement...");
    }
    else if (!state.LatestTestResults.Passed)
    {
        Console.WriteLine();
        Console.WriteLine("!!! MAX ITERATIONS REACHED !!!");
        Console.WriteLine("Escalating to human developer for review...");
    }

    Console.WriteLine();
}

// Human approval checkpoint
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine(" Human Checkpoint: AwaitApproval<HumanDeveloper>");
Console.WriteLine("-".PadRight(70, '-'));
if (state.LatestTestResults?.Passed == true)
{
    Console.WriteLine("Code ready for human review.");
    Console.WriteLine($"Total attempts: {state.AttemptCount}");
    Console.WriteLine();
    Console.WriteLine("Final Code:");
    Console.WriteLine(state.Attempts[^1].Code);
    Console.WriteLine();
    Console.WriteLine("Simulating approval...");
    state = state with { HumanApproved = true };
    Console.WriteLine("Human developer approved the implementation.");
}
else
{
    Console.WriteLine("Code requires human intervention due to failed tests.");
    Console.WriteLine($"Total attempts: {state.AttemptCount}");
    Console.WriteLine();
    Console.WriteLine("Last attempt:");
    Console.WriteLine(state.Attempts[^1].Code);
    Console.WriteLine();
    Console.WriteLine("Failures that need human attention:");
    foreach (var failure in state.LatestTestResults?.Failures ?? [])
    {
        Console.WriteLine($"  - {failure}");
    }
}

Console.WriteLine();

// Complete
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine(" Step: Complete");
Console.WriteLine("-".PadRight(70, '-'));
var completeStep = new Complete();
context = StepContext.Create(state.WorkflowId, nameof(Complete), "Completing");
result = await completeStep.ExecuteAsync(state, context, CancellationToken.None);
state = result.UpdatedState;
Console.WriteLine("Workflow completed.");
Console.WriteLine();

// Audit trail
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine(" Audit Trail - All Attempts");
Console.WriteLine("=".PadRight(70, '='));
for (var i = 0; i < state.Attempts.Count; i++)
{
    var attempt = state.Attempts[i];
    Console.WriteLine();
    Console.WriteLine($"Attempt {i + 1}: {attempt.Timestamp:O}");
    Console.WriteLine($"Reasoning: {attempt.Reasoning}");
    Console.WriteLine("Code preview: " + attempt.Code.Split('\n')[0]);
}

Console.WriteLine();
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine(" Final Summary");
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine($"Workflow ID: {state.WorkflowId}");
Console.WriteLine($"Total Attempts: {state.AttemptCount}");
Console.WriteLine($"Tests Passed: {state.LatestTestResults?.Passed}");
Console.WriteLine($"Human Approved: {state.HumanApproved}");
Console.WriteLine();
