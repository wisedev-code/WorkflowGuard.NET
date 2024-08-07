namespace WorkflowGuard;

public class CircuitBreakerPolicy
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _openDuration;
    private int _failureCount;
    private DateTime? _lastFailureTime;

    public CircuitBreakerPolicy(int failureThreshold, TimeSpan openDuration)
    {
        _failureThreshold = failureThreshold;
        _openDuration = openDuration;
    }

    public bool AllowExecution()
    {
        if (_failureCount >= _failureThreshold)
        {
            if (_lastFailureTime.HasValue && DateTime.UtcNow - _lastFailureTime.Value < _openDuration)
            {
                return false;
            }
            _failureCount = 0;
            _lastFailureTime = null;
        }
        return true;
    }

    public void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;
    }
}