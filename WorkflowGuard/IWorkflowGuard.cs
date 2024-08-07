namespace WorkflowGuard;

public interface IWorkflowGuard
{
    Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        string retryPolicy = null,
        string fallbackPolicy = null,
        string circuitBreakerPolicy = null);
}