using System.Security.Claims;

namespace Blog.Api.Services
{
    public interface IToken
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken(IEnumerable<Claim> claims);
        ClaimsPrincipal GetToken(string token);
    }
}
