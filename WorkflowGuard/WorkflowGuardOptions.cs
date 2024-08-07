
namespace WorkflowGuard;

public class WorkflowGuardOptions
{
    public IDictionary<string, RetryPolicy> RetryPolicies { get; } = new Dictionary<string, RetryPolicy>();
    public IDictionary<string, FallbackPolicy> FallbackPolicies { get; } = new Dictionary<string, FallbackPolicy>();
    public IDictionary<string, CircuitBreakerPolicy> CircuitBreakerPolicies { get; } = new Dictionary<string, CircuitBreakerPolicy>();

    public void AddRetryPolicy(string name, int retryCount, TimeSpan retryInterval)
    {
        RetryPolicies[name] = new RetryPolicy(retryCount, retryInterval);
    }

    public void AddFallbackPolicy(string name, Func<Task<object>> fallbackAction)
    {
        FallbackPolicies[name] = new FallbackPolicy(fallbackAction);
    }

    public void AddCircuitBreakerPolicy(string name, int failureThreshold, TimeSpan openDuration)
    {
        CircuitBreakerPolicies[name] = new CircuitBreakerPolicy(failureThreshold, openDuration);
    }
}