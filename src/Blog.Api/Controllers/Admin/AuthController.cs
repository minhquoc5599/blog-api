using Blog.Api.Helps.Extensions;
using Blog.Api.Services;
using Blog.Core.Domain.Identity;
using Blog.Core.Models.Auth;
using Blog.Core.Models.System;
using Blog.Core.SeedWorks.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Blog.Api.Controllers.Admin
{
    [Route("api/admin/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IToken _token;
        private readonly RoleManager<AppRole> _roleManager;
        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            IToken token, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _token = token;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<ActionResult<AuthenticatedResponse>> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserName) || 
                string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var user = await _userManager.FindByNameAsync(request.UserName);
            if(user == null)
            {
                return NotFound(StatusMessage.NotFound.User);
            }

            if (user.IsActive == false || user.LockoutEnabled)
            {
                return Unauthorized(StatusMessage.Unauthorized.LockedUser);
            }

            var result = await _signInManager.PasswordSignInAsync(request.UserName, request.Password,
                false, true);
            if (!result.Succeeded)
            {
                return Unauthorized(StatusMessage.Unauthorized.LoginFailed);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetPermissions(roles);
            var accessTokenClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(UserClaims.Id, user.Id.ToString()),
                new Claim(UserClaims.FirstName, user.FirstName),
                new Claim(UserClaims.Roles, string.Join(";", roles)),
                new Claim(UserClaims.UserName, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(UserClaims.Permissions, JsonSerializer.Serialize(permissions))
            };
            var refreshTokenClaims = new[]
            {
                new Claim(UserClaims.Id, user.Id.ToString())
            };
            var accessToken = _token.GenerateAccessToken(accessTokenClaims);
            var refreshToken = _token.GenerateRefreshToken(refreshTokenClaims);

            user.RefreshToken = refreshToken;
            //user.RefreshTokenExpireTime = DateTime.Now.AddDays(30);
            await _userManager.UpdateAsync(user);

            return Ok(new AuthenticatedResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken
            });
        }

        private async Task<List<string>> GetPermissions(IList<string> roles)
        {
            var permissions = new List<string>();
            var allPermissions = new List<RoleClaimsDto>();
            if (roles.Contains(Roles.Admin))
            {
                var types = typeof(Permissions).GetNestedTypes();
                foreach (var type in types)
                {
                    allPermissions.GetPermissions(type);
                }
                permissions.AddRange(allPermissions.Select(x => x.Value));
            }
            else
            {
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    var claims = await _roleManager.GetClaimsAsync(role);
                    var roleClaimsValues = claims.Select(x => x.Value).ToList();
                    permissions.AddRange(roleClaimsValues);
                }
            }
            return permissions.Distinct().ToList();
        }
    }
}
