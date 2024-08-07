namespace WorkflowGuard;

using Castle.DynamicProxy;
using System;
using System.Reflection;
using System.Threading.Tasks;

public class PolicyInterceptor(IWorkflowGuard workflowGuard) : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        var method = invocation.Method;
        var attribute = method.GetCustomAttribute<WorkflowGuardAttribute>();

        if (attribute != null)
        {
            // Capture method parameters
            var parameters = invocation.Arguments;

            // Create a task delegate to execute the method
            var operation = (Func<Task<object>>) (() => (Task<object>) method.Invoke(invocation.InvocationTarget, parameters)!);

            // Execute the method with policies applied
            var task = workflowGuard.ExecuteAsync(
                operation,
                attribute.RetryPolicy,
                attribute.FallbackPolicy,
                attribute.CircuitBreakerPolicy
            );

            // Set the result
            invocation.ReturnValue = task;
        }
        else
        {
            // If no attribute, just proceed with the normal method call
            invocation.Proceed();
        }
    }
}