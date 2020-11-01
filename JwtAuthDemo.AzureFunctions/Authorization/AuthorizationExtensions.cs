using System;
using System.Collections.Generic;
using System.Linq;
using JwtAuthDemo.AzureFunctions.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JwtAuthDemo.AzureFunctions.Authorization
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddFunctionsAuthorization(
            this IServiceCollection services, 
            Action<AuthorizationOptions> configureOptions)
        {
            services.AddIsolatedTransient<IAuthorizationHandlerProvider>(sp => 
                new DefaultAuthorizationHandlerProvider(
                    sp.GetRequiredService<IEnumerable<IAuthorizationHandler>>()));

            services.AddIsolatedTransient<IAuthorizationPolicyProvider>(sp => 
                new DefaultAuthorizationPolicyProvider(
                    sp.GetRequiredService<IOptions<AuthorizationOptions>>()));
            
            services.AddIsolatedTransient<IAuthorizationService>(sp => 
                new DefaultAuthorizationService(
                    sp.GetIsolatedService<IAuthorizationPolicyProvider>(),
                    sp.GetIsolatedService<IAuthorizationHandlerProvider>(),
                    sp.GetRequiredService<ILogger<DefaultAuthorizationService>>(),
                    sp.GetRequiredService<IAuthorizationHandlerContextFactory>(),
                    sp.GetRequiredService<IAuthorizationEvaluator>(),
                    sp.GetRequiredService<IOptions<AuthorizationOptions>>()));
            
            services.AddTransient<IPolicyEvaluator, FunctionsPolicyEvaluator>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IAuthorizationHandler, PassThroughAuthorizationHandler>());
            services.Configure(configureOptions);
            return services;
        }
        
        public static AuthorizationOptions AddFunctionPolicy(
            this AuthorizationOptions options, 
            string functionName, 
            AuthorizationPolicy policy)
        {
            options.AddPolicy(functionName, policy);
            return options;
        }
        
        public static AuthorizationOptions AddFunctionPolicy(
            this AuthorizationOptions options, 
            string functionName, 
            Action<AuthorizationPolicyBuilder> configurePolicy)
        {
            options.AddPolicy(functionName, configurePolicy);
            return options;
        }
        
        public static AuthorizationOptions AddFunctionPolicy(
            this AuthorizationOptions options, 
            string functionName, 
            params string[] policyNames)
        {
            var policies = new List<AuthorizationPolicy>();

            foreach (var name in policyNames)
            {
                var policy = options.GetPolicy(name);

                if (policy == null) 
                    throw new InvalidOperationException($"Authorization policy '{name}' is not registered");

                policies.Add(policy);
            }

            if (policies.Any()) 
                options.AddPolicy(functionName, AuthorizationPolicy.Combine(policies));

            return options;
        }
    }
}