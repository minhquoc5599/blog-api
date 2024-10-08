using Blog.Core.Models.Content;

namespace Blog.WebApp.Models
{
    public class PostDetailViewModel
    {
        public PostDetailResponse Post { get; set; }
        public PostCategoryResponse Category { get; set; }
    }
}
