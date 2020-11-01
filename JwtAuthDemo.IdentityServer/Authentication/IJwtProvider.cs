using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthDemo.IdentityServer.Authentication
{
    public interface IJwtProvider
    {
        string Issuer { get; }

        JsonWebKey JsonWebKey { get; }

        string CreateAccessToken(IEnumerable<Claim> claims);
    }
}