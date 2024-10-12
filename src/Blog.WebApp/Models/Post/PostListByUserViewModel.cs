using Blog.Core.Models.Base;
using Blog.Core.Models.Content;

namespace Blog.WebApp.Models.Post
{
	public class PostListByUserViewModel
	{
		public string Keyword { get; set; }
		public int TotalPosts { get; set; }
		public int TotalDraftPosts { get; set; }
		public int TotalWaitingApprovalPosts { get; set; }
		public int TotalPublishedPosts { get; set; }
		public int TotalUnpaidPosts { get; set; }
		public double TotalUnpaidAmount { get; set; }
		public double TotalPaidAmount { get; set; }
		public PagingResponse<PostResponse> Posts { get; set; }
	}
}
