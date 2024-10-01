namespace Blog.Core.Models.Auth
{
    public class TokenRequest
    {
        public required string RefreshToken { get; set; }
    }
}
