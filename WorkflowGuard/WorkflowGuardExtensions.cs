using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace WorkflowGuard;

public static class WorkflowGuardExtensions
{
    public static IServiceCollection AddWorkflowGuard(this IServiceCollection services, Action<WorkflowGuardOptions> configureOptions)
    {
        var options = new WorkflowGuardOptions();
        configureOptions(options);

        services.AddSingleton<IWorkflowGuard>(sp => new WorkflowGuard(
            options.RetryPolicies,
            options.FallbackPolicies,
            options.CircuitBreakerPolicies
        ));

        return services;
    }
    
    public static void AddWorkflowAttributes(this IServiceCollection services)
    {
        var proxyGenerator = new ProxyGenerator();

        // Get all service descriptors from the service collection
        var serviceDescriptors = services.Where(descriptor =>
            descriptor.ServiceType.IsInterface && descriptor.ImplementationType != null).ToList();

        foreach (var descriptor in serviceDescriptors)
        {
            var serviceType = descriptor.ServiceType;
            var implementationType = descriptor.ImplementationType;

            // Check if any method in the service has ApplyPoliciesAttribute
            var methodsWithAttributes = serviceType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Any(m => m.GetCustomAttribute<WorkflowGuardAttribute>() != null);

            if (methodsWithAttributes)
            {
                // Preserve the original implementation registration
                var originalDescriptor = descriptor;

                // Remove the original descriptor
                services.Remove(originalDescriptor);

                // Register the original implementation type
                services.Add(new ServiceDescriptor(implementationType, implementationType, originalDescriptor.Lifetime));

                // Register the proxy for the interface
                services.Add(new ServiceDescriptor(serviceType, provider =>
                {
                    var workflowGuard = provider.GetRequiredService<IWorkflowGuard>();
                    var implementation = provider.GetRequiredService(implementationType);
                    return proxyGenerator.CreateInterfaceProxyWithTarget(serviceType, implementation, new PolicyInterceptor(workflowGuard));
                }, originalDescriptor.Lifetime));
            }
        }
    }
}