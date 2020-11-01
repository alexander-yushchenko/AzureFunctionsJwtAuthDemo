using System;
using JwtAuthDemo.IdentityServer.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDemo.IdentityServer.Controllers
{
    [ApiController]
    [Route(".well-known")]
    public class AuthOpenIdController : ControllerBase
    {
        private readonly IJwtProvider _jwtProvider;

        public AuthOpenIdController(IJwtProvider jwtProvider)
        {
            _jwtProvider = jwtProvider;
        }

        [HttpGet("openid-configuration")]
        public IActionResult GetOpenIdConfiguration()
        {
            var uriBuilder = new UriBuilder(Request.GetDisplayUrl())
            {
                Path = Url.Action(nameof(GetJsonWebKeys)),
                Query = null
            };

            var result = new
            {
                issuer = _jwtProvider.Issuer,
                jwks_uri = uriBuilder.Uri.ToString()
            };

            return Ok(result);
        }
        
        [HttpGet("jwks")]
        public IActionResult GetJsonWebKeys()
        {
            var jsonKey = _jwtProvider.JsonWebKey;

            var result = new
            {
                Keys = new[] {jsonKey}
            };

            return Ok(result);
        }
    }
}