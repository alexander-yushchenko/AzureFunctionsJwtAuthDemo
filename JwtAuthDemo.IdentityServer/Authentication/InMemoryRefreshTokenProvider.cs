using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace JwtAuthDemo.IdentityServer.Authentication
{
    public class InMemoryRefreshTokenProvider : IRefreshTokenProvider
    {
        private readonly IDictionary<string,string> _tokens = new ConcurrentDictionary<string, string>();

        public string CreateToken(string id)
        {
            var bytes = new byte[32];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }

            var refreshToken = Convert.ToBase64String(bytes);

            _tokens[refreshToken] = id;

            return refreshToken;
        }

        public bool ValidateToken(string id, string token)
        {
            if (string.IsNullOrWhiteSpace(id) || 
                string.IsNullOrWhiteSpace(token) || 
                !_tokens.ContainsKey(token))
            {
                return false;
            }
            
            var storedId = _tokens[token];
            
            _tokens.Remove(token);
            
            return id.Equals(storedId, StringComparison.OrdinalIgnoreCase);
        }
    }
}