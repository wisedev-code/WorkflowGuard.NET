 
# WorkflowGuard  <img align="right" height="170" width="170" style="padding: 45pt" src="WorkflowGuard/icon.png"/>

**WorkflowGuard** is a .NET library that provides an easy-to-use mechanism for applying resilient policies to method executions. It works similar to `Polly`, but offers simplistic way to support retry, fallback, and circuit breaker policies, enabling developers to ensure their applications are more resilient to transient faults and outages.

## Features

- **Attribute-Based Configuration:** Apply retry, fallback, and circuit breaker policies using custom attributes.
- **Dynamic Proxies:** Automatically intercept method calls to apply policies without modifying business logic.
- **Seamless Integration:** Integrates smoothly with ASP.NET Core dependency injection and minimal APIs.

## Usage

### Attribute-Based Example

1. **Define your service interface and implementation:**

    ```csharp
    public interface IService
    {
        [WorkflowGuard(retryPolicy: "DefaultRetryPolicy", fallbackPolicy: "DefaultFallbackPolicy", circuitBreakerPolicy: "DefaultCircuitBreakerPolicy")]
        Task<Customer> GetCustomerAsync(int id);

        [WorkflowGuard(retryPolicy: "DefaultRetryPolicy", fallbackPolicy: "DefaultFallbackPolicy", circuitBreakerPolicy: "DefaultCircuitBreakerPolicy")]
        Task<Order> CreateOrderAsync(Order order);
    }

    public class Service : IService
    {
        public async Task<Customer> GetCustomerAsync(int id)
        {
            // Implementation
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            // Implementation
        }
    }
    ```

2. **Register your services and apply the `AddWorkflowAttributes` extension in your startup configuration:**

    ```csharp
    var builder = WebApplication.CreateBuilder(args);

    // Configure WorkflowGuard
    builder.Services.AddSingleton<IWorkflowGuard>(sp => new WorkflowGuard(
        new Dictionary<string, RetryPolicy>
        {
            ["DefaultRetryPolicy"] = new RetryPolicy(3, TimeSpan.FromSeconds(2))
        },
        new Dictionary<string, FallbackPolicy>
        {
            ["DefaultFallbackPolicy"] = new FallbackPolicy(() => Task.FromResult<object>(new { Message = "Fallback result" }))
        },
        new Dictionary<string, CircuitBreakerPolicy>
        {
            ["DefaultCircuitBreakerPolicy"] = new CircuitBreakerPolicy(3, TimeSpan.FromSeconds(30))
        }
    ));

    // Register your services
    builder.Services.AddSingleton<IService, Service>();

    // Apply workflow attributes
    builder.Services.AddWorkflowAttributes();

    var app = builder.Build();

    app.MapGet("/customer/{id}", async (int id, IService service) =>
    {
        var customer = await service.GetCustomerAsync(id);
        return Results.Ok(customer);
    });

    app.MapPost("/order", async (Order order, IService service) =>
    {
        var createdOrder = await service.CreateOrderAsync(order);
        return Results.Ok(createdOrder);
    });

    app.Run();
    ```

### Direct Usage Example (without Attributes)

**Manually apply policies using `IWorkflowGuard` in your API endpoints:**

    ```csharp

    app.MapGet("/customer/{id}", async (int id, IService service, IWorkflowGuard workflowGuard) =>
    {
        var customer = await workflowGuard.ExecuteAsync(
            () => service.GetCustomerAsync(id),
            "DefaultRetryPolicy",
            "DefaultFallbackPolicy",
            "DefaultCircuitBreakerPolicy"
        );
        return Results.Ok(customer);
    });

    app.MapPost("/order", async (Order order, IService service, IWorkflowGuard workflowGuard) =>
    {
        var createdOrder = await workflowGuard.ExecuteAsync(
            () => service.CreateOrderAsync(order),
            "DefaultRetryPolicy",
            "DefaultFallbackPolicy",
            "DefaultCircuitBreakerPolicy"
        );
        return Results.Ok(createdOrder);
    });

    app.Run();
    ```
## Installation

```bash
dotnet add package WorkflowGuard
