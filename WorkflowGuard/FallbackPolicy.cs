namespace WorkflowGuard;

public class FallbackPolicy
{
    private readonly Func<Task<object>> _fallbackAction;

    public FallbackPolicy(Func<Task<object>> fallbackAction)
    {
        _fallbackAction = fallbackAction;
    }

    public async Task<T> ExecuteAsync<T>()
    {
        return (T)await _fallbackAction();
    }
}