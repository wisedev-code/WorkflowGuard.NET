namespace WorkflowGuard;

public class WorkflowGuard : IWorkflowGuard
{
    private readonly IDictionary<string, RetryPolicy> _retryPolicies;
    private readonly IDictionary<string, FallbackPolicy> _fallbackPolicies;
    private readonly IDictionary<string, CircuitBreakerPolicy> _circuitBreakerPolicies;

    public WorkflowGuard(IDictionary<string, RetryPolicy> retryPolicies, IDictionary<string, FallbackPolicy> fallbackPolicies, IDictionary<string, CircuitBreakerPolicy> circuitBreakerPolicies)
    {
        _retryPolicies = retryPolicies;
        _fallbackPolicies = fallbackPolicies;
        _circuitBreakerPolicies = circuitBreakerPolicies;
    }

    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        string retryPolicy = null,
        string fallbackPolicy = null,
        string circuitBreakerPolicy = null)
    {
        CircuitBreakerPolicy circuitBreaker = null;
        if (!string.IsNullOrEmpty(circuitBreakerPolicy))
        {
            circuitBreaker = _circuitBreakerPolicies[circuitBreakerPolicy];
            if (!circuitBreaker.AllowExecution())
            {
                if (!string.IsNullOrEmpty(fallbackPolicy))
                {
                    return await _fallbackPolicies[fallbackPolicy].ExecuteAsync<T>();
                }
                throw new Exception("Circuit breaker is open. Operation blocked.");
            }
        }

        try
        {
            if (!string.IsNullOrEmpty(retryPolicy))
            {
                return await _retryPolicies[retryPolicy].ExecuteAsync(operation);
            }
            else
            {
                return await operation();
            }
        }
        catch (Exception ex)
        {
            circuitBreaker?.RecordFailure();
            if (!string.IsNullOrEmpty(fallbackPolicy))
            {
                return await _fallbackPolicies[fallbackPolicy].ExecuteAsync<T>();
            }
            throw;
        }
    }
}