using Blog.Core.Domain.Content;
using Blog.Core.Models.Base;
using Blog.Core.Models.Content;
using Blog.Core.SeedWorks;

namespace Blog.Core.Repositories
{
    public interface IPostCategoryRepository : IRepository<PostCategory, Guid>
    {
        Task<PagingResponse<PostCategoryResponse>> GetAllPagingAsync(string? keyword, int pageIndex = 1,
            int pageSize = 1);
    }
}
