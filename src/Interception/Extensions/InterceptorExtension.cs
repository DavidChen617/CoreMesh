using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Interception.Extensions;

public static class InterceptorExtension
{
    extension(Type type)
    {
        private IEnumerable<Attribute> GetInterfaceAttributesWithInherited()  
        {                                                                                                                          
            foreach (var attr in type.GetCustomAttributes())                                                                       
                yield return attr;                                                                                                 
                                                                                                                             
            foreach (var parentInterface in type.GetInterfaces())                                                               
            foreach (var attr in parentInterface.GetCustomAttributes())                                                        
                yield return attr;                                                                                             
        }
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection AddInterceptor(params Assembly[] assemblies)
        {
            // Find all interceptor types (both sync and async)
            var allInterceptorTypes = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass &&
                            !t.IsAbstract &&
                            typeof(IInterceptorBase).IsAssignableFrom(t));

            // Register all interceptor types with appropriate key
            foreach (var interceptorType in allInterceptorTypes)
            {
                var isAsync = typeof(IAsyncInterceptor).IsAssignableFrom(interceptorType);
                var serviceType = isAsync ? typeof(IAsyncInterceptor) : typeof(IInterceptor);
                services.AddKeyedSingleton(serviceType, interceptorType.Name, interceptorType);
            }

            // Find the interceptor type used by the interface using this attribute
            var interceptorAndInterfaceType = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsInterface)
                .Select(interfaceType =>
                {
                    var interceptorTypes = interfaceType.GetInterfaceAttributesWithInherited()
                        .Select(o =>
                        {
                            var attrType = o.GetType();

                            if (attrType.IsGenericType &&
                                attrType.GetGenericTypeDefinition() == typeof(InterceptedByAttribute<>))
                                return attrType.GetGenericArguments()
                                    .FirstOrDefault(x => typeof(IInterceptorBase).IsAssignableFrom(x));

                            return null;
                        })
                        .Where(t => t is not null)
                        .Select(t =>
                        {
                            // Directly check if type implements IAsyncInterceptor or IInterceptor
                            var interceptorBaseType = typeof(IAsyncInterceptor).IsAssignableFrom(t)
                                ? typeof(IAsyncInterceptor)
                                : typeof(IInterceptor);

                            return new
                            {
                                interceptorType = t,
                                interceptorBaseType
                            };
                        })
                        .ToList();

                    return (interfaceType, interceptorTypes);
                })
                .Where(x => x.interceptorTypes.Count > 0);

            // Reregister with interceptor
            foreach (var (interfaceType, interceptors) in interceptorAndInterfaceType)
            {
                var original = services.FirstOrDefault(t => t.ServiceType == interfaceType);

                if (original is null)
                    continue;

                services.Remove(original);

                if (original.ImplementationType is not null)
                    services.Add(new ServiceDescriptor(
                        original.ImplementationType,
                        original.ImplementationType,
                        original.Lifetime));

                // Check if there are any async interceptors
                var hasAsync = interceptors.Any(t => t.interceptorBaseType == typeof(IAsyncInterceptor));

                var proxyType = hasAsync
                    ? typeof(AsyncInterceptorProxy<>).MakeGenericType(interfaceType)
                    : typeof(InterceptorProxy<>).MakeGenericType(interfaceType);

                var proxyInterceptorType = hasAsync ? typeof(IAsyncInterceptor) : typeof(IInterceptor);

                var createMethod = proxyType.GetMethod(
                                       nameof(InterceptorProxy<>.Create),
                                       BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy,
                                       [interfaceType, proxyInterceptorType]) ??
                                   throw new InvalidOperationException($"Method Create not found on {proxyType.Name}!");

                // Collect key and baseType for each interceptor
                var interceptorInfos = interceptors
                    .Select(t => (key: t.interceptorType!.Name, baseType: t.interceptorBaseType!))
                    .ToArray();

                Func<IServiceProvider, object?> func = sp =>
                {
                    var instance = FindInstance(original, sp);

                    // Collect all interceptors (mixed types)
                    var theInterceptors = interceptorInfos
                        .Select(info => info.baseType == typeof(IAsyncInterceptor)
                            ? (IInterceptorBase)sp.GetRequiredKeyedService<IAsyncInterceptor>(info.key)
                            : sp.GetRequiredKeyedService<IInterceptor>(info.key))
                        .ToArray();

                    // CompositeInterceptor implements both IInterceptor and IAsyncInterceptor
                    var composite = theInterceptors.Length == 1
                        ? theInterceptors[0]
                        : new CompositeInterceptor(theInterceptors);

                    // Cast based on proxy type
                    var interceptor = hasAsync
                        ? (object)(IAsyncInterceptor)composite
                        : (IInterceptor)composite;

                    return createMethod.Invoke(null, [instance, interceptor]);
                };

                services.Add(new ServiceDescriptor(interfaceType, func!, original.Lifetime));
            }

            return services;
        }
    }

    private static object? FindInstance(ServiceDescriptor? original, IServiceProvider sp)
    {
        object? instance;

        if (original!.ImplementationType is not null)
            instance = sp.GetRequiredService(original.ImplementationType);
        else if (original.ImplementationInstance is not null)
            instance = original.ImplementationInstance;
        else if (original.ImplementationFactory is not null)
            instance = original.ImplementationFactory(sp);
        else
            throw new InvalidOperationException("Cannot create instance");

        return instance;
    }
}
