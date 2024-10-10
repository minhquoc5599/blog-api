using Blog.Core.SeedWorks.Constants;
using System.Security.Claims;

namespace Blog.WebApp.Helpers.Extensions
{
	public static class IdentityExtension
	{
		public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
		{
			var subjectId = claimsPrincipal.GetSpecificClaim(ClaimTypes.NameIdentifier);
			return Guid.Parse(subjectId);
		}
		public static string GetFirstName(this ClaimsPrincipal claimsPrincipal)
		{
			var subjectId = claimsPrincipal.GetSpecificClaim(UserClaims.FirstName);
			return subjectId;
		}
		public static string GetUserName(this ClaimsPrincipal claimsPrincipal)
		{
			var subjectId = claimsPrincipal.GetSpecificClaim(UserClaims.UserName);
			return subjectId;
		}
		public static string GetEmail(this ClaimsPrincipal claimsPrincipal)
		{
			var subjectId = claimsPrincipal.GetSpecificClaim(ClaimTypes.Email);
			return subjectId;
		}
		public static string GetSpecificClaim(this ClaimsPrincipal claimsPrincipal, string claimType)
		{
			var claim = ((ClaimsIdentity)claimsPrincipal.Identity)?.Claims.FirstOrDefault(x => x.Type == claimType);
			return claim != null ? claim.Value : string.Empty;
		}
	}
}
