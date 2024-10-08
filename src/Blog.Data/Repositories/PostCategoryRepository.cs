﻿using AutoMapper;
using Blog.Core.Domain.Content;
using Blog.Core.Models.Base;
using Blog.Core.Models.Content;
using Blog.Core.Repositories;
using Blog.Data.SeedWorks;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Repositories
{
	public class PostCategoryRepository : RepositoryBase<PostCategory, Guid>, IPostCategoryRepository
	{
		private readonly IMapper _mapper;
		public PostCategoryRepository(BlogContext context, IMapper mapper) : base(context)
		{
			_mapper = mapper;
		}

		public async Task<PagingResponse<PostCategoryResponse>> GetPostCategoriesAsync(string? keyword, int pageIndex = 1, int pageSize = 1)
		{
			var query = _context.PostCategories.AsQueryable();
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				query = query.Where(x => x.Name.Contains(keyword));
			}
			var totalRow = await query.CountAsync();

			query = query.OrderByDescending(x => x.DateCreated)
			   .Skip((pageIndex - 1) * pageSize)
			   .Take(pageSize);

			return new PagingResponse<PostCategoryResponse>
			{
				Results = await _mapper.ProjectTo<PostCategoryResponse>(query).ToListAsync(),
				CurrentPage = pageIndex,
				RowCount = totalRow,
				PageSize = pageSize
			};
		}

		public async Task<bool> CheckExistPost(Guid postCategoryId)
		{
			return await _context.Posts.AnyAsync(x => x.CategoryId == postCategoryId);
		}

		public async Task<PostCategoryResponse> GetBySlug(string slug)
		{
			var category = await _context.PostCategories.FirstOrDefaultAsync(x => x.Slug == slug);
			if (category == null)
			{
				throw new Exception($"Cannot find {slug}");
			}
			return _mapper.Map<PostCategoryResponse>(category);
		}
	}
}
