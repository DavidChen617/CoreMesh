using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreMesh.Validation.Extensions;

/// <summary>
/// Provides dependency injection registration extensions for <c>CoreMesh.Validation</c>.
/// </summary>
public static class ValidationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers validation services in the dependency injection container.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddValidation()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddScoped(typeof(IValidator<>), typeof(Validator<>));
            return services;
        }
    }
}
