using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JwtAuthDemo.AzureFunctions.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JwtAuthDemo.AzureFunctions.Authorization
{
    public class FunctionsPolicyEvaluator : IPolicyEvaluator
    {
        public Task<AuthenticateResult> AuthenticateAsync(
            AuthorizationPolicy policy,
            HttpContext context)
        {
            var service = context.RequestServices.GetRequiredService<IAuthenticationService>();
            return AuthenticateAsync(service, policy, context);
        }

        public async Task<PolicyAuthorizationResult> AuthorizeAsync(
            AuthorizationPolicy policy,
            AuthenticateResult authenticationResult,
            HttpContext context,
            object resource)
        {
            var policyProvider = context.RequestServices.GetIsolatedService<IAuthorizationPolicyProvider>();
            var functionName = GetFunctionName(resource);
            var functionPolicy = await policyProvider.GetPolicyAsync(functionName);
            
            if (functionPolicy == null)
            {
                var authorizationService = context.RequestServices.GetRequiredService<IAuthorizationService>();
                return await AuthorizeAsync(authorizationService, policy, authenticationResult, context, resource);
            }
            
            var contextPrincipal = context.User;
            
            try
            {
                context.User = null;
                var authenticationService = context.RequestServices.GetIsolatedService<IAuthenticationService>();
                var functionAuthenticationResult = await AuthenticateAsync(authenticationService, functionPolicy, context);
                var authorizationService = context.RequestServices.GetIsolatedService<IAuthorizationService>();
                return await AuthorizeAsync(authorizationService, functionPolicy, functionAuthenticationResult, context, resource);
            }
            finally
            {
                context.User = MergeUserPrincipal(context.User, contextPrincipal);
            }
        }

        private static async Task<AuthenticateResult> AuthenticateAsync(
            IAuthenticationService service,
            AuthorizationPolicy policy,
            HttpContext context)
        {
            if (!policy.AuthenticationSchemes?.Any() ?? true)
                return context.User?.Identity?.IsAuthenticated ?? false
                    ? AuthenticateResult.Success(
                        new AuthenticationTicket(context.User, "context.User"))
                    : AuthenticateResult.NoResult();
            
            ClaimsPrincipal principal = null;

            foreach (var scheme in policy.AuthenticationSchemes)
            {
                var result = await service.AuthenticateAsync(context, scheme);
                
                if (result?.Succeeded ?? false)
                    principal = MergeUserPrincipal(principal, result.Principal);
            }
            
            if (principal != null)
            {
                context.User = principal;
                return AuthenticateResult.Success(
                    new AuthenticationTicket(principal, string.Join(";", policy.AuthenticationSchemes)));
            }
                         
            context.User = new ClaimsPrincipal(new ClaimsIdentity());
            return AuthenticateResult.NoResult();
        }
        
        private static async Task<PolicyAuthorizationResult> AuthorizeAsync(
            IAuthorizationService service,
            AuthorizationPolicy policy,
            AuthenticateResult authenticationResult,
            HttpContext context,
            object resource)
        {
            if (policy == null) 
                throw new ArgumentNullException(nameof(policy));

            var result = await service.AuthorizeAsync(context.User, resource, policy);
            
            if (result.Succeeded) 
                return PolicyAuthorizationResult.Success();

            return authenticationResult.Succeeded 
                ? PolicyAuthorizationResult.Forbid() 
                : PolicyAuthorizationResult.Challenge();
        }
        
        private static ClaimsPrincipal MergeUserPrincipal(
            ClaimsPrincipal existingPrincipal, 
            ClaimsPrincipal additionalPrincipal)
        {
            var principal = new ClaimsPrincipal();
            
            if (additionalPrincipal != null) 
                principal.AddIdentities(additionalPrincipal.Identities);

            if (existingPrincipal == null) 
                return principal;
            
            var identities = existingPrincipal.Identities.Where(i => i.IsAuthenticated || i.Claims.Any());
            principal.AddIdentities(identities);

            return principal;
        }
        
        private static string GetFunctionName(object resource)
        {
            if (resource == null) 
                return null;

            var resourceType = resource.GetType();
        
            if (!resourceType.Name.Equals("FunctionDescriptor"))
                return null;

            var property = resourceType.GetProperty("Name");
            
            return property?.GetValue(resource) as string;
        }
    }
}