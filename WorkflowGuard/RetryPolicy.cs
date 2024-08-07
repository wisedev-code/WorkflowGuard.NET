namespace WorkflowGuard;

public class RetryPolicy
{
    private readonly int _retryCount;
    private readonly TimeSpan _retryInterval;

    public RetryPolicy(int retryCount, TimeSpan retryInterval)
    {
        _retryCount = retryCount;
        _retryInterval = retryInterval;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        for (int i = 0; i < _retryCount; i++)
        {
            try
            {
                var result =await operation();
                return result;
            }
            catch(Exception ex)
            {
                if (i == _retryCount - 1)
                {
                    throw;
                }
                await Task.Delay(_retryInterval);
            }
        }
        throw new InvalidOperationException("Operation failed after retries.");
    }
}