using Blog.Core.Models.Base;
using Blog.Core.Models.Content;

namespace Blog.WebApp.Models
{
	public class PostListByTagViewModel
	{
		public TagResponse Tag { get; set; }
		public PagingResponse<PostResponse> Posts { get; set; }
	}
}
