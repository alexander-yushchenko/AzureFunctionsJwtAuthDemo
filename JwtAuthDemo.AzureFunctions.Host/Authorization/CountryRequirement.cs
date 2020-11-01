using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace JwtAuthDemo.AzureFunctions.Host.Authorization
{
    public class CountryRequirement : AuthorizationHandler<CountryRequirement>, IAuthorizationRequirement
    {
        public CountryRequirement(string countryCode)
        {
            CountryCode = countryCode;
        }
        
        public string CountryCode { get; }
        
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            CountryRequirement requirement)
        {
            if (context.User != null && 
                context.User.HasClaim(ClaimTypes.Country, requirement.CountryCode))
            {
                context.Succeed(requirement);
            }
            
            return Task.CompletedTask;
        }
    }
}