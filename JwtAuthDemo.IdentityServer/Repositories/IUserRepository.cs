using System;
using JwtAuthDemo.IdentityServer.Models;

namespace JwtAuthDemo.IdentityServer.Repositories
{
    public interface IUserRepository
    {
        UserModel FindUser(Guid id);
        
        UserModel FindUser(string userName);
    }
}