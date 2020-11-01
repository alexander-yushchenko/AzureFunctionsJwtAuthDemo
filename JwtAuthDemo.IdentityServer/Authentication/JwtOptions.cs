using System;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthDemo.IdentityServer.Authentication
{
    public class JwtOptions
    {
        public string Issuer { get; set; }
        
        public string Audience { get; set; }

        public X509SecurityKey SecurityKey { get; set; }

        public TimeSpan ExpirationTime { get; set; } = TimeSpan.FromMinutes(15);
    }
}