using AutoMapper;
using Blog.Core.Domain.Content;
using Blog.Core.Models.Content;
using Blog.Core.Repositories;
using Blog.Data.SeedWorks;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Repositories
{
	public class TagRepository : RepositoryBase<Tag, Guid>, ITagRepository
	{
		private readonly IMapper _mapper;
		public TagRepository(BlogContext context, IMapper mapper) : base(context)
		{
			_mapper = mapper;
		}

		public async Task<TagResponse?> GetTagBySlug(string slug)
		{
			var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Slug == slug);
			if (tag == null)
			{
				return null;
			}
			return _mapper.Map<TagResponse>(tag);
		}
	}
}
