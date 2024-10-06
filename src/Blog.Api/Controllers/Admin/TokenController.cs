using Blog.Api.Helps.Extensions;
using Blog.Api.Services;
using Blog.Core.Domain.Identity;
using Blog.Core.Models.Auth;
using Blog.Core.Models.System;
using Blog.Core.SeedWorks.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Blog.Api.Controllers.Admin
{
    [Route("api/admin/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IToken _token;

        public TokenController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
            IToken token)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _token = token;
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<ActionResult<AuthenticatedResponse>> Refresh(TokenRequest request)
        {
            if (request == null)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            //string accessToken = request.AccessToken;
            //string refreshToken = request.RefreshToken;
            //var principal = _token.GetFromExpiredToken(accessToken);
            //if (principal == null || principal.Identity == null || principal.Identity.Name == null)
            //{
            //    return BadRequest(StatusMessage.BadRequest.InvalidToken);
            //}

            //var userName = principal.Identity.Name;
            //var user = await _userManager.FindByNameAsync(userName);
            //if (user is null || user.RefreshToken != refreshToken)
            //{
            //    return NotFound(StatusMessage.NotFound.User);
            //}
            //var newAccessToken = _token.GenerateAccessToken(principal.Claims);

            // Check refresh token
            string refreshToken = request.RefreshToken;
            var principal = _token.GetToken(refreshToken);
            if (principal == null)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidToken);
            }
            var id = principal.Claims.SingleOrDefault(claim => claim.Type == UserClaims.Id);
            var user = await _userManager.FindByIdAsync(id.Value);
            if (user == null)
            {
                return NotFound(StatusMessage.NotFound.User);
            }

            if (user.IsActive == false || user.LockoutEnabled)
            {
                return Unauthorized(StatusMessage.Unauthorized.LockedUser);
            }

            // Generate access token
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
            var newAccessToken = _token.GenerateAccessToken(accessTokenClaims);

            // Refresh token expired
            var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(principal.FindFirst("exp").Value));
            DateTime expireDate = exp.UtcDateTime;

            if (expireDate < DateTime.Now)
            {
                var refreshTokenClaims = new[]
                {
                    new Claim(UserClaims.Id, user.Id.ToString())
                };
                var newRefreshToken = _token.GenerateRefreshToken(refreshTokenClaims);
                user.RefreshToken = newRefreshToken;
                //user.RefreshTokenExpireTime = DateTime.Now.AddDays(30);

                await _userManager.UpdateAsync(user);

                return Ok(new AuthenticatedResponse()
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                });
            }

            return Ok(new AuthenticatedResponse()
            {
                Token = newAccessToken,
                RefreshToken = refreshToken,
            });
        }

        [HttpPost, Authorize]
        [Route("revoke")]
        public async Task<ActionResult> Revoke()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user is null)
            {
                return BadRequest();
            }
            user.RefreshToken = null;
            user.RefreshTokenExpireTime = null;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ?NoContent(): StatusCode(500);
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
