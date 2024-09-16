using Blog.Api.Services;
using Blog.Core.Domain.Identity;
using Blog.Core.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers.Admin
{
    [Route("api/admin/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IToken _token;

        public TokenController(UserManager<AppUser> userManager, IToken token)
        {
            _userManager = userManager;
            _token = token;
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<ActionResult<AuthenticatedResponse>> Refresh(TokenRequest request)
        {
            if (request == null)
            {
                return BadRequest("Bad request");
            }
            string accessToken = request.AccessToken;
            string refreshToken = request.RefreshToken;
            var principal = _token.GetFromExpiredToken(accessToken);
            if (principal == null || principal.Identity == null || principal.Identity.Name == null)
            {
                return BadRequest("Invalid token");
            }

            var userName = principal.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null || user.RefreshToken != refreshToken)
            {
                return BadRequest("Bad request");
            }
            var newAccessToken = _token.GenerateAccessToken(principal.Claims);

            // Refresh token expired
            if (user.RefreshTokenExpireTime < DateTime.Now)
            {
                var newRefreshToken = _token.GenerateRefreshToken();
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpireTime = DateTime.Now.AddDays(30);

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
            var user = await _userManager.FindByIdAsync(User.Identity.Name);
            if (user is null)
            {
                return BadRequest();
            }
            user.RefreshToken = null;
            user.RefreshTokenExpireTime = null;
            await _userManager.UpdateAsync(user);
            return NoContent();
        }
    }
}
