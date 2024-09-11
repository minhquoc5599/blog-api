using Blog.Api.Services;
using Blog.Core.Domain.Identity;
using Blog.Core.Models.Auth;
using Blog.Core.SeedWorks.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Blog.Api.Controllers.AdminApi
{
    [Route("api/admin/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IToken _token;
        public AuthController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, IToken token)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _token = token;
        }

        [HttpPost]
        public async Task<ActionResult<LoginResult>> Login([FromBody] LoginRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request");
            }

            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null || user.IsActive == false || user.LockoutEnabled)
            {
                return Unauthorized();
            }

            var result = await _signInManager.PasswordSignInAsync(request.UserName, request.Password,
                false, true);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(UserClaims.Id, user.Id.ToString()),
                new Claim(UserClaims.FirstName, user.FirstName),
                new Claim(UserClaims.Roles, string.Join(";", roles)),
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                //new Claim(UserClaims.Permissions, JsonSerializer.Serialize(permissions))
            };
            var accessToken = _token.GenerateAccessToken(claims);
            var refreshToken = _token.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpireTime = DateTime.Now.AddDays(30);
            await _userManager.UpdateAsync(user);

            return Ok(new LoginResult
            {
                Token = accessToken,
                RefreshToken = refreshToken
            });
        }
    }
}
