
namespace WorkflowGuard;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class WorkflowGuardAttribute(
    string retryPolicy = null,
    string fallbackPolicy = null,
    string circuitBreakerPolicy = null)
    : Attribute
{
    public string RetryPolicy { get; } = retryPolicy;
    public string FallbackPolicy { get; } = fallbackPolicy;
    public string CircuitBreakerPolicy { get; } = circuitBreakerPolicy;
}