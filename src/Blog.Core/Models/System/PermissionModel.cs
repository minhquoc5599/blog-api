namespace Blog.Core.Models.System
{
    public class PermissionModel
    {
        public string RoleId { get; set; }
        public IList<RoleClaimsDto> RoleClaims { get; set; }
    }
}
