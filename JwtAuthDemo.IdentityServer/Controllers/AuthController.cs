using System;
using System.Collections.Generic;
using System.Security.Claims;
using JwtAuthDemo.IdentityServer.Authentication;
using JwtAuthDemo.IdentityServer.Constants;
using JwtAuthDemo.IdentityServer.Models;
using JwtAuthDemo.IdentityServer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDemo.IdentityServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IRefreshTokenProvider _refreshTokenProvider;
        private readonly IUserRepository _userRepository;

        public AuthController(
            IJwtProvider jwtProvider,
            IRefreshTokenProvider refreshTokenProvider,
            IUserRepository userRepository)
        {
            _jwtProvider = jwtProvider;
            _refreshTokenProvider = refreshTokenProvider;
            _userRepository = userRepository;
        }
        
        [HttpPost("signin")]
        public IActionResult SignIn([FromBody] SignInModel model)
        {
            var user = _userRepository.FindUser(model?.UserName);
            
            if (user == null || 
                !string.Equals(user.Password, model?.Password, StringComparison.Ordinal))
            {
                return Unauthorized();
            }

            return TokenResult(user);
        }
        
        [HttpPost("refresh")]
        [Authorize(AuthenticationSchemes = AuthSchemes.RefreshToken)]
        public IActionResult RefreshToken([FromBody] RefreshTokenModel model)
        {
            UserModel user = null;
            
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            
            if (Guid.TryParse(idClaim?.Value, out var id))
            {
                user = _userRepository.FindUser(id);
            }

            var tokenId = GetRefreshTokenId(
                User.FindFirst(JwtClaimNames.TokenId)?.Value, user);

            if (!_refreshTokenProvider.ValidateToken(tokenId, model?.RefreshToken))
            {
                return Unauthorized();
            }

            return TokenResult(user);
        }

        private OkObjectResult TokenResult(UserModel user)
        {
            var tokenId = Guid.NewGuid().ToString();
            var refreshTokenId = GetRefreshTokenId(tokenId, user);
            var claims = new List<Claim>
            {
                new Claim(JwtClaimNames.TokenId, tokenId),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            claims.AddRange(user.Claims);
            var accessToken = _jwtProvider.CreateAccessToken(claims);
            var refreshToken = _refreshTokenProvider.CreateToken(refreshTokenId);
            
            return new OkObjectResult(new
            {
                user.Id,
                user.UserName,
                accessToken,
                refreshToken
            });
        }

        private static string GetRefreshTokenId(string accessTokenId, UserModel user)
        {
            return $"{accessTokenId}-{user.Id}";
        }
    }
}