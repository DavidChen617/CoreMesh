using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreMesh.Mapper.Extensions;

/// <summary>
/// Provides dependency injection registration extensions for <see cref="IMapper"/>.
/// </summary>
public static class MapperServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers <see cref="IMapper"/> and scans the specified assemblies for mapping contracts.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for mapping contracts.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddCoreMeshMapper(params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(assemblies);

            services.TryAddSingleton<IMapper>(_ =>
            {
                var mapper = new Mapper();

                foreach (var assembly in assemblies.Distinct())
                {
                    mapper.RegisterMapper(assembly);
                }

                return mapper;
            });

            return services;
        }
    }
}
