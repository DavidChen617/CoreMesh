using System.Reflection;
using CoreMesh.Validation.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreMesh.Validation.Extensions;

/// <summary>
/// Provides extension methods for registering validation services with dependency injection.
/// </summary>
public static class ValidationExtension
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the <see cref="IValidator"/> and all <see cref="IValidatable{T}"/> implementations
        /// found in the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for <see cref="IValidatable{T}"/> implementations.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <example>
        /// <code>
        /// services.AddValidatable(typeof(Program).Assembly);
        /// </code>
        /// </example>
        public IServiceCollection AddValidatable(params Assembly[] assemblies)
        {
            services.TryAddSingleton<IValidator, Validator>();

            foreach (var impl in assemblies.SelectMany(x => x.GetTypes())
                         .Where(t => t is { IsClass: true, IsAbstract: false }))
            {
                foreach (var itf in impl.GetInterfaces()
                             .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidatable<>)))
                {
                    services.TryAddTransient(itf, impl);
                }
            }

            return services;
        }
    }
}
