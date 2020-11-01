using System;
using Microsoft.Extensions.DependencyInjection;

namespace JwtAuthDemo.AzureFunctions.Internal
{
    internal static class DependencyInjectionExtensions
    {
        internal static T GetIsolatedService<T>(this IServiceProvider provider)
        {
            var container = provider.GetRequiredService<ServiceContainer<T>>();
            return container.Service;
        }

        internal static IServiceCollection AddIsolatedTransient<T>(
            this IServiceCollection services, 
            Func<IServiceProvider, T> implementationFactory)
        {
            return services.AddTransient(sp => new ServiceContainer<T>(implementationFactory(sp)));
        }
    }
}