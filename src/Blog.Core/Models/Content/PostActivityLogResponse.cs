using Blog.Core.Domain.Content;

namespace Blog.Core.Models.Content
{
    public class PostActivityLogResponse
    {
        public PostStatus FromStatus { get; set; }
        public PostStatus ToStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public string? Note { get; set; }
        public string UserName { get; set; }
    }
}
