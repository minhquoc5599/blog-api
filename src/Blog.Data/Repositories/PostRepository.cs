using AutoMapper;
using Blog.Core.Domain.Content;
using Blog.Core.Domain.Identity;
using Blog.Core.Models.Base;
using Blog.Core.Models.Content;
using Blog.Core.Repositories;
using Blog.Core.SeedWorks.Constants;
using Blog.Data.SeedWorks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Repositories
{
	public class PostRepository : RepositoryBase<Post, Guid>, IPostRepository
	{
		private readonly IMapper _mapper;
		private readonly UserManager<AppUser> _userManager;
		public PostRepository(BlogContext context, IMapper mapper,
			UserManager<AppUser> userManager) : base(context)
		{
			_mapper = mapper;
			_userManager = userManager;
		}

		public Task<List<Post>> GetPopularPostsAsync(int count)
		{
			return _context.Posts.OrderByDescending(x => x.ViewCount).Take(count).ToListAsync();
		}

		public async Task<PagingResponse<PostResponse>> GetPostsAsync(string? keyword, Guid currentUserId, Guid? categoryId, int pageIndex = 1, int pageSize = 10)
		{
			var user = await _userManager.FindByIdAsync(currentUserId.ToString()) ?? throw new Exception("User does not exist");
			var roles = await _userManager.GetRolesAsync(user);
			var canApprove = false;
			if (roles.Contains(Roles.Admin))
			{
				canApprove = true;
			}
			else
			{
				canApprove = await _context.RoleClaims.AnyAsync(
					x => roles.Contains(x.RoleId.ToString()) && x.ClaimValue == Permissions.Posts.Approve);
			}

			var query = _context.Posts.AsQueryable();
			if (!string.IsNullOrEmpty(keyword))
			{
				query = query.Where(x => x.Name.Contains(keyword));
			}
			if (categoryId.HasValue)
			{
				query = query.Where(x => x.CategoryId == categoryId.Value);
			}
			if (!canApprove)
			{
				query = query.Where(x => x.AuthorUserId == currentUserId);
			}
			var totalRow = await query.CountAsync();
			query = query.OrderByDescending(x => x.DateCreated)
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize);

			return new PagingResponse<PostResponse>
			{
				Results = await _mapper.ProjectTo<PostResponse>(query).ToListAsync(),
				CurrentPage = pageIndex,
				RowCount = totalRow,
				PageSize = pageSize
			};
		}

		public Task<bool> IsSlugAlreadyExisted(string slug, Guid? currentId = null)
		{
			if (currentId.HasValue)
			{
				return _context.Posts.AnyAsync(x => x.Slug == slug && x.Id != currentId.Value);
			}
			return _context.Posts.AnyAsync(x => x.Slug == slug);
		}



		public async Task<List<SeriesResponse>> GetSeriesWithPostId(Guid postId)
		{
			var query = from pis in _context.PostInSeries
						join s in _context.Series
						on pis.SeriesId equals s.Id
						where pis.PostId == postId
						select s;
			return await _mapper.ProjectTo<SeriesResponse>(query).ToListAsync();
		}

		public async Task Approve(Guid id, Guid currentUserId)
		{
			var post = await _context.Posts.FindAsync(id) ?? throw new Exception("Post does not exist");
			var user = await _context.Users.FindAsync(currentUserId) ?? throw new Exception("User does not exist");
			await _context.PostActivityLogs.AddAsync(new PostActivityLog
			{
				Id = Guid.NewGuid(),
				FromStatus = post.Status,
				ToStatus = PostStatus.Published,
				UserId = currentUserId,
				UserName = user.UserName,
				PostId = id,
				Note = $"{user?.UserName} approved"
			});
			post.Status = PostStatus.Published;
			_context.Posts.Update(post);
		}

		public async Task SubmitForApproval(Guid id, Guid currentUserId)
		{
			var post = await _context.Posts.FindAsync(id) ?? throw new Exception("Post does not exist");
			var user = await _context.Users.FindAsync(currentUserId) ?? throw new Exception("User does not exist");
			await _context.PostActivityLogs.AddAsync(new PostActivityLog
			{
				Id = Guid.NewGuid(),
				FromStatus = post.Status,
				ToStatus = PostStatus.WaitingForApproval,
				UserId = currentUserId,
				UserName = user.UserName,
				PostId = id,
				Note = $"{user?.UserName} submit for approval"
			});
			post.Status = PostStatus.WaitingForApproval;
			_context.Posts.Update(post);
		}

		public async Task Reject(Guid id, Guid currentUserId, string note)
		{
			var post = await _context.Posts.FindAsync(id) ?? throw new Exception("Post does not exist");
			var user = await _context.Users.FindAsync(currentUserId) ?? throw new Exception("User does not exist");
			await _context.PostActivityLogs.AddAsync(new PostActivityLog
			{
				FromStatus = post.Status,
				ToStatus = PostStatus.Rejected,
				UserId = currentUserId,
				UserName = user.UserName,
				PostId = post.Id,
				Note = note
			});

			post.Status = PostStatus.Rejected;
			_context.Posts.Update(post);
		}

		public async Task<string> GetRejectReason(Guid id)
		{
			var activityLog = await _context.PostActivityLogs
				 .Where(x => x.PostId == id && x.ToStatus == PostStatus.Rejected)
				 .OrderByDescending(x => x.DateCreated)
				 .FirstOrDefaultAsync();
			return activityLog?.Note;
		}


		public async Task<bool> HasPublished(Guid id)
		{
			var hasPublished = await _context.PostActivityLogs.CountAsync(x => x.PostId == id
			&& x.ToStatus == PostStatus.Published);
			return hasPublished > 0;
		}

		public async Task<List<PostActivityLogResponse>> GetPostActivityLogsWithPostId(Guid id)
		{
			var query = _context.PostActivityLogs.Where(x => x.PostId == id)
				.OrderByDescending(x => x.DateCreated);
			return await _mapper.ProjectTo<PostActivityLogResponse>(query).ToListAsync();
		}

		public async Task<List<Post>> GetListUnpaidPublishPosts(Guid userId)
		{
			return await _context.Posts
				.Where(x => x.AuthorUserId == userId && x.IsPaid == false && x.Status == PostStatus.Published)
				.ToListAsync();
		}

		public async Task<List<PostResponse>> GetLatestPost(int top)
		{
			var query = _context.Posts
				.Where(x => x.Status == PostStatus.Published)
				.OrderByDescending(x => x.DateCreated).Take(top);
			return await _mapper.ProjectTo<PostResponse>(query).ToListAsync();
		}
	}
}
