using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace JwtAuthDemo.IdentityServer.Models
{
    public class UserModel
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string UserName { get; set; }

        public string Password { get; set; }
        
        public ICollection<Claim> Claims { get; } = new List<Claim>();
    }
}