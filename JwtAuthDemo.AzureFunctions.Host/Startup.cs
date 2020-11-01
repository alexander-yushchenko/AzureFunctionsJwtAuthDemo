using System;
using System.Threading.Tasks;
using JwtAuthDemo.AzureFunctions.Authentication;
using JwtAuthDemo.AzureFunctions.Authorization;
using JwtAuthDemo.AzureFunctions.Host.Authorization;
using JwtAuthDemo.AzureFunctions.Host.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

[assembly: FunctionsStartup(typeof(JwtAuthDemo.AzureFunctions.Host.Startup))]

namespace JwtAuthDemo.AzureFunctions.Host
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            Configure(builder.Services);
        }

        private static void Configure(IServiceCollection services)
        {
            var audience = Environment.GetEnvironmentVariable("AccessTokenAudience");
            var authority = Environment.GetEnvironmentVariable("AccessTokenAuthority");
            
            services.AddFunctionsAuthentication(builder =>
            {
                builder.AddJwtBearer(AuthSchemes.AzureFunction, jwtOptions =>
                {
                    jwtOptions.SaveToken = false;
                    jwtOptions.Audience = audience;
                    jwtOptions.Authority = authority;
                    jwtOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true
                    };
                    jwtOptions.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.Headers.Add(
                                    CorsConstants.AccessControlExposeHeaders,
                                    AuthConstants.TokenExpiredHeader);

                                context.Response.Headers.Add(
                                    AuthConstants.TokenExpiredHeader, "true");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
            });
            
            services.AddFunctionsAuthorization(options =>
            {
                const string userPolicyName = "UserPolicy";
                const string countryPolicyName = "CountryPolicy";
                
                options.AddPolicy(userPolicyName, policy =>
                {
                    policy.AuthenticationSchemes.Add(AuthSchemes.AzureFunction);
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("User");
                });
                    
                options.AddPolicy(countryPolicyName, policy =>
                {
                    policy.AddRequirements(new CountryRequirement("UA"));
                });
                
                options.AddFunctionPolicy(FunctionNames.AdminEndpoint, policy =>
                {
                    policy.AuthenticationSchemes.Add(AuthSchemes.AzureFunction);
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Admin");
                });
                
                options.AddFunctionPolicy(FunctionNames.UserEndpoint, userPolicyName);
                
                options.AddFunctionPolicy(FunctionNames.LimitedAccessByCountry, userPolicyName, countryPolicyName);
            });
        }
    }
}