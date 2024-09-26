using Blog.Core.Domain.Identity;
using Blog.Core.Repositories;
using Blog.Data.SeedWorks;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Repositories
{
    public class RoleRepository : RepositoryBase<AppRole, Guid>, IRoleRepository
    {
        public RoleRepository(BlogContext context) : base(context)
        {
        }

        public async Task<bool> CheckExistUser(Guid roleId)
        {
            return await _context.UserRoles.AnyAsync(x => x.RoleId == roleId);
        }
    }
}
