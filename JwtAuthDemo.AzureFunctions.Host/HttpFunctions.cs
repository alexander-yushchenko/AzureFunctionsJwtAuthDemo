using JwtAuthDemo.AzureFunctions.Host.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JwtAuthDemo.AzureFunctions.Host
{
    public static class HttpFunctions
    {
        [FunctionName("HttpFunction_AnonymousLevel")]
        public static IActionResult RunFunctionAnonymousLevel(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "builtin/anonymous")]
            HttpRequest req, ILogger log)
        {
            return new OkObjectResult("Access is allowed");
        }
        
        [FunctionName("HttpFunction_AdminLevel")]
        public static IActionResult RunFunctionAdminLevel(
            [HttpTrigger(AuthorizationLevel.Admin, "get", Route = "builtin/admin")]
            HttpRequest req, ILogger log)
        {
            return new OkObjectResult("Access is allowed");
        }

        [FunctionName(FunctionNames.AdminEndpoint)]
        public static IActionResult RunJwtAdmin(
            [HttpTrigger(AuthorizationLevel.System, "get", Route = "jwt/admin")]
            HttpRequest req, ILogger log)
        {
            return new OkObjectResult("Access is allowed");
        }
        
        [FunctionName(FunctionNames.UserEndpoint)]
        public static IActionResult RunJwtUser(
            [HttpTrigger(AuthorizationLevel.System, "get", Route = "jwt/user")]
            HttpRequest req, ILogger log)
        {
            return new OkObjectResult("Access is allowed");
        }
        
        [FunctionName(FunctionNames.LimitedAccessByCountry)]
        public static IActionResult RunJwtLimitedAccess(
            [HttpTrigger(AuthorizationLevel.System, "get", Route = "jwt/limited")]
            HttpRequest req, ILogger log)
        {
            return new OkObjectResult("Access is allowed");
        }
    }
}