# Basic Workflow Example

This example demonstrates a simple linear workflow for processing orders.

## Overview

Linear workflows execute steps in sequence from start to finish. Each step receives the current state, performs work, and returns an updated state.

## State Definition

```csharp
[WorkflowState]
public record OrderState : IWorkflowState
{
    public Guid WorkflowId { get; init; }
    public Order Order { get; init; } = null!;
    public bool IsValid { get; init; }
    public PaymentResult? Payment { get; init; }
    public ShipmentInfo? Shipment { get; init; }
    public OrderStatus Status { get; init; }
}

public record Order(
    string CustomerId,
    IReadOnlyList<OrderItem> Items,
    Address ShippingAddress);

public record OrderItem(string ProductId, int Quantity, decimal Price);

public record PaymentResult(string TransactionId, bool Success);

public record ShipmentInfo(string TrackingNumber, DateOnly EstimatedDelivery);

public enum OrderStatus { Pending, Validated, Paid, Shipped, Completed }
```

## Workflow Definition

```csharp
var workflow = Workflow<OrderState>
    .Create("process-order")
    .StartWith<ValidateOrder>()
    .Then<ProcessPayment>()
    .Then<FulfillOrder>()
    .Finally<SendConfirmation>();
```

This reads naturally: "Create a process-order workflow. Start with validating the order, then process payment, then fulfill the order, and finally send confirmation."

## Step Implementations

### ValidateOrder

```csharp
public class ValidateOrder : IWorkflowStep<OrderState>
{
    private readonly IOrderValidator _validator;

    public ValidateOrder(IOrderValidator validator)
    {
        _validator = validator;
    }

    public async Task<StepResult<OrderState>> ExecuteAsync(
        OrderState state,
        StepContext context,
        CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(state.Order, ct);

        if (!validationResult.IsValid)
        {
            return StepResult.Fail<OrderState>(
                Error.Create("ORDER_INVALID", validationResult.ErrorMessage));
        }

        return state
            .With(s => s.IsValid, true)
            .With(s => s.Status, OrderStatus.Validated)
            .AsResult();
    }
}
```

### ProcessPayment

```csharp
public class ProcessPayment : IWorkflowStep<OrderState>
{
    private readonly IPaymentService _paymentService;

    public ProcessPayment(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<StepResult<OrderState>> ExecuteAsync(
        OrderState state,
        StepContext context,
        CancellationToken ct)
    {
        var amount = state.Order.Items.Sum(i => i.Price * i.Quantity);
        var payment = await _paymentService.ChargeAsync(
            state.Order.CustomerId,
            amount,
            ct);

        if (!payment.Success)
        {
            return StepResult.Fail<OrderState>(
                Error.Create("PAYMENT_FAILED", "Payment processing failed"));
        }

        return state
            .With(s => s.Payment, payment)
            .With(s => s.Status, OrderStatus.Paid)
            .AsResult();
    }
}
```

### FulfillOrder

```csharp
public class FulfillOrder : IWorkflowStep<OrderState>
{
    private readonly IFulfillmentService _fulfillment;

    public FulfillOrder(IFulfillmentService fulfillment)
    {
        _fulfillment = fulfillment;
    }

    public async Task<StepResult<OrderState>> ExecuteAsync(
        OrderState state,
        StepContext context,
        CancellationToken ct)
    {
        var shipment = await _fulfillment.ShipAsync(
            state.Order.Items,
            state.Order.ShippingAddress,
            ct);

        return state
            .With(s => s.Shipment, shipment)
            .With(s => s.Status, OrderStatus.Shipped)
            .AsResult();
    }
}
```

### SendConfirmation

```csharp
public class SendConfirmation : IWorkflowStep<OrderState>
{
    private readonly INotificationService _notifications;

    public SendConfirmation(INotificationService notifications)
    {
        _notifications = notifications;
    }

    public async Task<StepResult<OrderState>> ExecuteAsync(
        OrderState state,
        StepContext context,
        CancellationToken ct)
    {
        await _notifications.SendOrderConfirmationAsync(
            state.Order.CustomerId,
            state.Shipment!.TrackingNumber,
            ct);

        return state
            .With(s => s.Status, OrderStatus.Completed)
            .AsResult();
    }
}
```

## Registration

```csharp
services.AddAgenticWorkflow()
    .AddWorkflow<ProcessOrderWorkflow>();

// Register step dependencies
services.AddScoped<IOrderValidator, OrderValidator>();
services.AddScoped<IPaymentService, StripePaymentService>();
services.AddScoped<IFulfillmentService, WarehouseFulfillmentService>();
services.AddScoped<INotificationService, EmailNotificationService>();
```

## Starting the Workflow

```csharp
public class OrderController : ControllerBase
{
    private readonly IWorkflowStarter _workflowStarter;

    public OrderController(IWorkflowStarter workflowStarter)
    {
        _workflowStarter = workflowStarter;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        var workflowId = Guid.NewGuid();
        var initialState = new OrderState
        {
            WorkflowId = workflowId,
            Order = new Order(
                request.CustomerId,
                request.Items,
                request.ShippingAddress)
        };

        await _workflowStarter.StartAsync("process-order", initialState);

        return Accepted(new { WorkflowId = workflowId });
    }
}
```

## Generated Artifacts

The source generator produces:

1. **ProcessOrderPhase enum** - `NotStarted`, `ValidateOrder`, `ProcessPayment`, `FulfillOrder`, `SendConfirmation`, `Completed`, `Failed`
2. **ProcessOrderSaga** - Wolverine saga with handlers for each step
3. **Commands** - `StartProcessOrderCommand`, `ExecuteValidateOrderCommand`, etc.
4. **Events** - `ProcessOrderStarted`, `ProcessOrderPhaseChanged`, `ProcessOrderCompleted`

## Key Points

- Steps are resolved via dependency injection
- State is immutable; use `With()` to create updated copies
- Each step returns `StepResult<TState>` indicating success or failure
- Failed steps trigger error handling (if configured)
- The workflow survives process restarts via Wolverine saga persistence
