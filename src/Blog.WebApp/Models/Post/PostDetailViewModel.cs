using Blog.Core.Models.Content;

namespace Blog.WebApp.Models.Post
{
    public class PostDetailViewModel
    {
        public PostDetailResponse Post { get; set; }
        public PostCategoryResponse Category { get; set; }
        public List<TagResponse> Tags { get; set; }
    }
}
