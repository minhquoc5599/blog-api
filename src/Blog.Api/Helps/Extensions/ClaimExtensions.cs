using Blog.Core.Domain.Identity;
using Blog.Core.Models.System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.Reflection;
using System.Security.Claims;

namespace Blog.Api.Helps.Extensions
{
    public static class ClaimExtensions
    {
        public static void GetPermissions(this List<RoleClaimsDto> allPermissions, Type policy)
        {
            FieldInfo[] fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                string displayName = field.GetValue(null).ToString();
                var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attributes.Length > 0)
                {
                    var description = (DescriptionAttribute)attributes[0];
                    displayName = description.Description;
                }
                allPermissions.Add(new RoleClaimsDto
                {
                    Value = field.GetValue(null).ToString(),
                    Type = "Permissions",
                    DisplayName = displayName,
                });
            }
        }
        public static async Task AddPermissionClaim(this RoleManager<AppRole> roleManager,
        AppRole role, string permission)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            if (!allClaims.Any(a => a.Type == "Permissions" && a.Value == permission))
            {
                await roleManager.AddClaimAsync(role, new Claim("Permissions", permission));
            }
        }
    }

}
