using Blog.Core.Domain.Content;
using Blog.Core.Models.Content;
using Blog.Core.SeedWorks;

namespace Blog.Core.Repositories
{
	public interface ITagRepository : IRepository<Tag, Guid>
	{
		Task<TagResponse?> GetTagBySlug(string slug);
	}
}
