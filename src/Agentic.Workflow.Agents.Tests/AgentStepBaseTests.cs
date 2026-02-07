// =============================================================================
// <copyright file="AgentStepBaseTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Steps;

namespace Agentic.Workflow.Agents.Tests;

/// <summary>
/// Unit tests for the <see cref="AgentStepBase{TState}"/> abstract class.
/// </summary>
[Property("Category", "Unit")]
public class AgentStepBaseTests
{
    // =============================================================================
    // Test State
    // =============================================================================

    /// <summary>
    /// Test workflow state implementation.
    /// </summary>
    private sealed record TestWorkflowState : IWorkflowState
    {
        /// <inheritdoc/>
        public Guid WorkflowId { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the user query.
        /// </summary>
        public string UserQuery { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        public string Response { get; init; } = string.Empty;
    }

    /// <summary>
    /// Concrete test implementation of AgentStepBase.
    /// </summary>
    private sealed class TestAgentStep : AgentStepBase<TestWorkflowState>
    {
        /// <summary>
        /// Gets or sets the system prompt to return.
        /// </summary>
        public string TestSystemPrompt { get; set; } = "You are a test assistant.";

        /// <summary>
        /// Gets or sets the user prompt to return.
        /// </summary>
        public string TestUserPrompt { get; set; } = "Test query";

        /// <summary>
        /// Gets or sets the response content for simulation.
        /// </summary>
        public string SimulatedResponse { get; set; } = "Test response";

        /// <summary>
        /// Gets a value indicating whether ApplyResultAsync was called.
        /// </summary>
        public bool ApplyResultCalled { get; private set; }

        /// <summary>
        /// Gets the last response passed to ApplyResultAsync.
        /// </summary>
        public string? LastAppliedResponse { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAgentStep"/> class.
        /// </summary>
        /// <param name="chatClient">The chat client.</param>
        /// <param name="contextAssembler">Optional context assembler.</param>
        public TestAgentStep(
            IChatClient chatClient,
            IContextAssembler<TestWorkflowState>? contextAssembler = null)
            : base(chatClient, contextAssembler)
        {
        }

        /// <inheritdoc/>
        public override string GetSystemPrompt() => TestSystemPrompt;

        /// <inheritdoc/>
        public override Type? GetOutputSchemaType() => null;

        /// <inheritdoc/>
        protected override string GetUserPrompt(TestWorkflowState state) => TestUserPrompt;

        /// <inheritdoc/>
        protected override Task<StepResult<TestWorkflowState>> ApplyResultAsync(
            TestWorkflowState state,
            string response,
            CancellationToken cancellationToken)
        {
            ApplyResultCalled = true;
            LastAppliedResponse = response;
            var newState = state with { Response = response };
            return Task.FromResult(StepResult<TestWorkflowState>.FromState(newState));
        }
    }

    /// <summary>
    /// Test implementation of context assembler.
    /// </summary>
    private sealed class TestContextAssembler : IContextAssembler<TestWorkflowState>
    {
        /// <summary>
        /// Gets a value indicating whether AssembleAsync was called.
        /// </summary>
        public bool AssembleCalled { get; private set; }

        /// <summary>
        /// Gets or sets the context to return.
        /// </summary>
        public AssembledContext ContextToReturn { get; set; } = AssembledContext.Empty;

        /// <inheritdoc/>
        public Task<AssembledContext> AssembleAsync(
            TestWorkflowState state,
            StepContext stepContext,
            CancellationToken cancellationToken)
        {
            AssembleCalled = true;
            return Task.FromResult(ContextToReturn);
        }
    }

    // =============================================================================
    // A. Assembler Invocation Tests
    // =============================================================================

    /// <summary>
    /// Verifies that ExecuteAsync calls the assembler when one is provided.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_WithAssembler_CallsAssembler()
    {
        // Arrange
        var chatClient = Substitute.For<IChatClient>();
        var assembler = new TestContextAssembler();
        var step = new TestAgentStep(chatClient, assembler);
        var state = new TestWorkflowState { UserQuery = "What is 2+2?" };
        var context = CreateStepContext();

        SetupChatClientResponse(chatClient, "4");

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(assembler.AssembleCalled).IsTrue();
    }

    /// <summary>
    /// Verifies that ExecuteAsync works when no assembler is provided.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_WithNullAssembler_SkipsAssembly()
    {
        // Arrange
        var chatClient = Substitute.For<IChatClient>();
        var step = new TestAgentStep(chatClient, null);
        var state = new TestWorkflowState { UserQuery = "What is 2+2?" };
        var context = CreateStepContext();

        SetupChatClientResponse(chatClient, "4");

        // Act
        var result = await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert - Should complete without throwing
        await Assert.That(result).IsNotNull();
        await Assert.That(result.UpdatedState.Response).IsEqualTo("4");
    }

    // =============================================================================
    // B. Message Building Tests
    // =============================================================================

    /// <summary>
    /// Verifies that BuildMessages includes the system prompt.
    /// </summary>
    [Test]
    public async Task BuildMessages_IncludesSystemPrompt()
    {
        // Arrange
        var chatClient = Substitute.For<IChatClient>();
        var step = new TestAgentStep(chatClient)
        {
            TestSystemPrompt = "You are a math expert.",
        };
        var state = new TestWorkflowState();
        var emptyContext = AssembledContext.Empty;

        // Act
        var messages = step.BuildMessagesForTest(state, emptyContext);

        // Assert
        await Assert.That(messages).IsNotEmpty();
        var systemMessage = messages.FirstOrDefault(m => m.Role == ChatRole.System);
        await Assert.That(systemMessage).IsNotNull();
        await Assert.That(systemMessage!.Text).Contains("math expert");
    }

    /// <summary>
    /// Verifies that BuildMessages includes context message when context is non-empty.
    /// </summary>
    [Test]
    public async Task BuildMessages_WithNonEmptyContext_IncludesContextMessage()
    {
        // Arrange
        var chatClient = Substitute.For<IChatClient>();
        var step = new TestAgentStep(chatClient);
        var state = new TestWorkflowState();
        var contextBuilder = new AssembledContextBuilder();
        contextBuilder.AddLiteralContext("Important context information");
        var context = contextBuilder.Build();

        // Act
        var messages = step.BuildMessagesForTest(state, context);

        // Assert
        var messagesText = string.Join(" ", messages.Select(m => m.Text ?? string.Empty));
        await Assert.That(messagesText).Contains("Important context information");
    }

    /// <summary>
    /// Verifies that BuildMessages omits context message when context is empty.
    /// </summary>
    [Test]
    public async Task BuildMessages_WithEmptyContext_OmitsContextMessage()
    {
        // Arrange
        var chatClient = Substitute.For<IChatClient>();
        var step = new TestAgentStep(chatClient)
        {
            TestUserPrompt = "User question here",
        };
        var state = new TestWorkflowState();
        var emptyContext = AssembledContext.Empty;

        // Act
        var messages = step.BuildMessagesForTest(state, emptyContext);

        // Assert
        // With empty context, we should only have system prompt and user prompt
        await Assert.That(messages.Count).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that BuildMessages includes user prompt.
    /// </summary>
    [Test]
    public async Task BuildMessages_IncludesUserPrompt()
    {
        // Arrange
        var chatClient = Substitute.For<IChatClient>();
        var step = new TestAgentStep(chatClient)
        {
            TestUserPrompt = "What is the meaning of life?",
        };
        var state = new TestWorkflowState();
        var emptyContext = AssembledContext.Empty;

        // Act
        var messages = step.BuildMessagesForTest(state, emptyContext);

        // Assert
        var userMessage = messages.FirstOrDefault(m => m.Role == ChatRole.User);
        await Assert.That(userMessage).IsNotNull();
        await Assert.That(userMessage!.Text).Contains("meaning of life");
    }

    // =============================================================================
    // C. Response Application Tests
    // =============================================================================

    /// <summary>
    /// Verifies that ApplyResultAsync is called with the LLM response.
    /// </summary>
    [Test]
    public async Task ExecuteAsync_WithResponse_CallsApplyResult()
    {
        // Arrange
        var chatClient = Substitute.For<IChatClient>();
        var step = new TestAgentStep(chatClient);
        var state = new TestWorkflowState { UserQuery = "Hello" };
        var context = CreateStepContext();

        SetupChatClientResponse(chatClient, "Hello back!");

        // Act
        await step.ExecuteAsync(state, context, CancellationToken.None);

        // Assert
        await Assert.That(step.ApplyResultCalled).IsTrue();
        await Assert.That(step.LastAppliedResponse).IsEqualTo("Hello back!");
    }

    // =============================================================================
    // Helper Methods
    // =============================================================================

    private static StepContext CreateStepContext() => new()
    {
        CorrelationId = Guid.NewGuid().ToString("N"),
        WorkflowId = Guid.NewGuid(),
        StepName = "TestStep",
        Timestamp = DateTimeOffset.UtcNow,
        CurrentPhase = "Testing",
    };

    private static void SetupChatClientResponse(IChatClient chatClient, string response)
    {
        var chatMessage = new ChatMessage(ChatRole.Assistant, response);
        var chatResponse = new ChatResponse(chatMessage);

        chatClient
            .GetService<ChatClientMetadata>()
            .Returns((ChatClientMetadata?)null);

        chatClient
            .GetResponseAsync(
                Arg.Any<IList<ChatMessage>>(),
                Arg.Any<ChatOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(chatResponse);
    }
}

/// <summary>
/// Extension methods for testing AgentStepBase.
/// </summary>
internal static class AgentStepBaseTestExtensions
{
    /// <summary>
    /// Exposes BuildMessages for testing.
    /// </summary>
    /// <typeparam name="TState">The workflow state type.</typeparam>
    /// <param name="step">The step to test.</param>
    /// <param name="state">The workflow state.</param>
    /// <param name="context">The assembled context.</param>
    /// <returns>The built messages.</returns>
    public static IList<ChatMessage> BuildMessagesForTest<TState>(
        this AgentStepBase<TState> step,
        TState state,
        AssembledContext context)
        where TState : class, IWorkflowState
    {
        return step.BuildMessages(state, context);
    }
}
