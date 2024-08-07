using Castle.DynamicProxy;
using Microsoft.AspNetCore.Mvc;
using WorkflowGuard;
using WorkflowGuard.Demo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure WorkflowGuard
builder.Services.AddSingleton<IWorkflowGuard>(sp => new WorkflowGuard.WorkflowGuard(
    new Dictionary<string, RetryPolicy>
    {
        ["DefaultRetryPolicy"] = new(3, TimeSpan.FromSeconds(2))
    },
    new Dictionary<string, FallbackPolicy>
    {
        ["DefaultFallbackPolicy"] = new(() => Task.FromResult<object>(new { Message = "Fallback result" }))
    },
    new Dictionary<string, CircuitBreakerPolicy>
    {
        ["DefaultCircuitBreakerPolicy"] = new(3, TimeSpan.FromSeconds(30))
    }
));

builder.Services.AddWorkflowAttributes();

var app = builder.Build();

app.MapGet("/customer/{id}", async ([FromServices] ICustomerService customerService, int id) =>
{
    try
    {
        var customer = await customerService.GetCustomerAsync(id);
        return Results.Ok(customer);
    }
    catch (Exception ex)
    {
        return Results.Problem("Internal server error");
    }
});

app.MapPost("/order", async ([FromServices] IOrderService orderService, Order order, IWorkflowGuard workflowGuard) =>
{
    try
    {
        var createdOrder = await workflowGuard.ExecuteAsync(
            () => orderService.CreateOrderAsync(order),
            retryPolicy: "DefaultRetryPolicy",
            fallbackPolicy: "DefaultFallbackPolicy",
            circuitBreakerPolicy: "DefaultCircuitBreakerPolicy"
        );
        return Results.Ok(createdOrder);
    }
    catch (Exception ex)
    {
        return Results.Problem("Internal server error");
    }
});

app.UseSwagger();
app.UseSwaggerUI();

app.Run();