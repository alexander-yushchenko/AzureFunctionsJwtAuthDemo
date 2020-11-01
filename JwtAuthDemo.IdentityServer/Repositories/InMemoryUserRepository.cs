using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using JwtAuthDemo.IdentityServer.Models;

namespace JwtAuthDemo.IdentityServer.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly ICollection<UserModel> _users = BuildUserStubs();
        
        public UserModel FindUser(Guid id)
        {
            return _users.FirstOrDefault(x => x.Id == id);
        }

        public UserModel FindUser(string userName)
        {
            return _users.FirstOrDefault(x => 
                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        }

        private static ICollection<UserModel> BuildUserStubs()
        {
            var userStubs = new List<UserModel>
            {
                new UserModel
                {
                    UserName = "Admin",
                    Password = "admin",
                    Claims =
                    {
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim(ClaimTypes.Role, "User")
                    }
                },
                new UserModel
                {
                    UserName = "User_1",
                    Password = "user",
                    Claims =
                    {
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(ClaimTypes.Country, "UA")
                    }
                },
                new UserModel
                {
                    UserName = "User_2",
                    Password = "user",
                    Claims =
                    {
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(ClaimTypes.Country, "GE")
                    }
                }
            };

            return userStubs;
        }
    }
}