namespace Blog.Core.Models.Auth
{
    public class LoginResult
    {
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }

    }
}
