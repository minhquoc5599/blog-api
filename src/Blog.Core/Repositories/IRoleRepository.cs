using Blog.Core.Domain.Identity;
using Blog.Core.SeedWorks;

namespace Blog.Core.Repositories
{
    public interface IRoleRepository : IRepository<AppRole, Guid>
    {
        Task<bool> CheckExistUser(Guid roleId);
    }
}
