using System;
using JwtAuthDemo.AzureFunctions.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JwtAuthDemo.AzureFunctions.Authentication
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddFunctionsAuthentication(
            this IServiceCollection services, 
            Action<AuthenticationBuilder> configureBuilder)
        {
            if (configureBuilder != null)
            {
                var builder = new AuthenticationBuilder(services);
                configureBuilder(builder);
            }

            services.AddIsolatedTransient<IAuthenticationSchemeProvider>(sp => 
                new AuthenticationSchemeProvider(
                    sp.GetRequiredService<IOptions<AuthenticationOptions>>()));
            
            services.AddIsolatedTransient<IAuthenticationHandlerProvider>(sp => 
                new AuthenticationHandlerProvider(
                    sp.GetIsolatedService<IAuthenticationSchemeProvider>()));
            
            services.AddIsolatedTransient<IAuthenticationService>(sp => 
                new AuthenticationService(
                    sp.GetIsolatedService<IAuthenticationSchemeProvider>(),
                    sp.GetIsolatedService<IAuthenticationHandlerProvider>(),
                    sp.GetRequiredService<IClaimsTransformation>(),
                    sp.GetRequiredService<IOptions<AuthenticationOptions>>()));

            return services;
        }
    }
}