using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthDemo.IdentityServer.Authentication
{
    public class DefaultJwtProvider : IJwtProvider
    {
        private readonly JwtOptions _options;

        public DefaultJwtProvider(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            JsonWebKey = JsonWebKeyConverter
                .ConvertFromX509SecurityKey(_options.SecurityKey);
        }

        public string Issuer => _options.Issuer;
        
        public JsonWebKey JsonWebKey { get; }
        
        public string CreateAccessToken(IEnumerable<Claim> claims)
        {
            var credentials = new SigningCredentials(
                _options.SecurityKey, 
                SecurityAlgorithms.RsaSha256);

            var currentTime = DateTime.Now;

            var token = new JwtSecurityToken(
                _options.Issuer,
                _options.Audience,
                claims,
                currentTime,
                currentTime.Add(_options.ExpirationTime),
                credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(token);

            return accessToken;
        }
    }
}