using Blog.Core.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Blog.WebApp.Helpers
{
	public class CustomClaimsPrincipalFactory: UserClaimsPrincipalFactory<AppUser, AppRole>
	{
		private readonly UserManager<AppUser> _userManager;

		public CustomClaimsPrincipalFactory(UserManager<AppUser> userManager, 
			RoleManager<AppRole> roleManager, 
			IOptions<IdentityOptions> options) : base(userManager, roleManager, options)
		{
			_userManager = userManager;
		}

		public override async Task<ClaimsPrincipal> CreateAsync(AppUser user)
		{
			var principal = await base.CreateAsync(user);
			//var roles = await _userManager.GetRolesAsync(user);
			return principal;
		}
	}
}
