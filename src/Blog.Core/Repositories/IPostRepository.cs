using Blog.Core.Domain.Content;
using Blog.Core.Models.Base;
using Blog.Core.Models.Content;
using Blog.Core.SeedWorks;

namespace Blog.Core.Repositories
{
	public interface IPostRepository : IRepository<Post, Guid>
	{
		Task<List<Post>> GetPopularPostsAsync(int count);
		Task<PagingResponse<PostResponse>> GetPostsAsync(string? keyword, Guid currentUserId, Guid? categoryId,
			int pageIndex = 1, int pageSize = 10);
		Task<bool> IsSlugAlreadyExisted(string slug, Guid? currentId = null);
		Task<List<SeriesResponse>> GetSeriesWithPostId(Guid postId);
		Task Approve(Guid id, Guid currentUserId);
		Task SubmitForApproval(Guid id, Guid currentUserId);
		Task Reject(Guid id, Guid currentUserId, string note);
		Task<string> GetRejectReason(Guid id);
		Task<bool> HasPublished(Guid id);
		Task<List<PostActivityLogResponse>> GetPostActivityLogsWithPostId(Guid id);
		Task<List<Post>> GetListUnpaidPublishPosts(Guid userId);
		Task<List<PostResponse>> GetLatestPost(int top);
		Task<PagingResponse<PostResponse>> GetPostsByCategory(string category, int pageIndex = 1,
			int pageSize = 10);
		Task<PostDetailResponse> GetPostBySlug(string slug);
		Task<List<string>> GetTags();
		Task AddTagToPost(Guid postId, Guid tagId);
		Task<List<string>> GetTagsByPostId(Guid postId);
		Task<List<TagResponse>> GetDetailTagsByPostId(Guid postId);
		Task<PagingResponse<PostResponse>> GetPostsByTag(string tag, int pageIndex = 1, int pageSize = 10);
	}
}
