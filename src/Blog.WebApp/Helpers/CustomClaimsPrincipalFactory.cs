using Blog.Core.Domain.Identity;
using Blog.Core.SeedWorks.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Blog.WebApp.Helpers
{
	public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, AppRole>
	{

		public CustomClaimsPrincipalFactory(UserManager<AppUser> userManager,
			RoleManager<AppRole> roleManager,
			IOptions<IdentityOptions> options) : base(userManager, roleManager, options)
		{
		}

		public override async Task<ClaimsPrincipal> CreateAsync(AppUser user)
		{
			var principal = await base.CreateAsync(user);
			// Add your claims here
			((ClaimsIdentity)principal.Identity)?.AddClaims(new[] {
				new Claim(UserClaims.Id, user.Id.ToString()),
				new Claim(UserClaims.UserName, user.UserName),
				new Claim(UserClaims.FirstName, user.FirstName),
			});
			return principal;
		}
	}
}
