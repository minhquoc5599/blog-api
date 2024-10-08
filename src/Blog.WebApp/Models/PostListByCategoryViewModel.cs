using Blog.Core.Models.Base;
using Blog.Core.Models.Content;

namespace Blog.WebApp.Models
{
	public class PostListByCategoryViewModel
	{
		public PostCategoryResponse Category { get; set; }
		public PagingResponse<PostResponse> Posts { get; set; }
	}
}
