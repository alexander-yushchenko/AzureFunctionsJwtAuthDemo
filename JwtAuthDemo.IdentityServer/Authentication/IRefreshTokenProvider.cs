namespace JwtAuthDemo.IdentityServer.Authentication
{
    public interface IRefreshTokenProvider
    {
        string CreateToken(string id);

        bool ValidateToken(string id, string token);
    }
}