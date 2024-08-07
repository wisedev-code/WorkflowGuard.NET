// Sample Models

namespace WorkflowGuard.Demo;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Order
{
    public int CustomerId { get; set; }
    public string Product { get; set; }
}

// Sample Services
public interface ICustomerService
{
    [WorkflowGuard(retryPolicy: "DefaultRetryPolicy")]
    Task<Customer> GetCustomerAsync(int id);
}

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Order order);
}

public class CustomerService : ICustomerService
{
    private static int _failureCount = 0;

    public async Task<Customer> GetCustomerAsync(int id)
    {
        if (id == -1 || _failureCount < 5)
        {
            _failureCount++;
            throw new Exception("Customer not found.");
        }
        _failureCount = 0;
        return await Task.FromResult(new Customer { Id = id, Name = "Customer" + id });
    }
}

public class OrderService : IOrderService
{
    private static int _failureCount = 0;

    public async Task<Order> CreateOrderAsync(Order order)
    {
        if (order.CustomerId == -1 || _failureCount < 5)
        {
            _failureCount++;
            throw new Exception("Invalid customer.");
        }
        _failureCount = 0;
        return await Task.FromResult(order);
    }
}