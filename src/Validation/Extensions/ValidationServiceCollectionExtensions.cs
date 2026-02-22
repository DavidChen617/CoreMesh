using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreMesh.Validation.Extensions;

public static class ValidationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddValidation()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddScoped(typeof(IValidator<>), typeof(Validator<>));
            return services;
        }
    }
}
